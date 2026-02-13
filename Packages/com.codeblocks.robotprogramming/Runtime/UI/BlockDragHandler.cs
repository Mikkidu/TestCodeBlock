using System;
using System.Linq;
using CodeBlocks.Data;
using UnityEngine;
using UnityEngine.EventSystems;


namespace CodeBlocks.UI
{
    [RequireComponent(typeof(BlockUIBase))]
    public class BlockDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        
        private CanvasGroup canvasGroup;
        private Transform originalParent;
        private int originalSiblingIndex;
        private Canvas rootCanvas;
        private ProgramArea programArea;
        
        private BlockUIBase parentBlock;

        /// <summary>
        /// If this block is first or last in a Loop, bypass it by reconnecting Loop internals.
        /// Must be called BEFORE DisconnectAllConnections().
        /// </summary>
        private void BypassBlockInLoop()
        {
            var primaryInput = parentBlock.GetPrimaryInput();
            var primaryOutput = parentBlock.GetPrimaryOutput();

            // Case 1: This block is FIRST (input connected to InternalOutput)
            if (primaryInput?.connectedTo?.role == BlockConnector.ConnectorRole.InternalOutput)
            {
                var internalOutput = primaryInput.connectedTo;

                // Bypass: IntOut → (whatever our output is connected to)
                internalOutput.connectedTo = primaryOutput?.connectedTo;
                if (primaryOutput?.connectedTo != null)
                {
                    primaryOutput.connectedTo.connectedTo = internalOutput;
                }

                Debug.Log($"[DRAG] Loop bypassed FIRST block: {parentBlock.name}");
                parentBlock.GetNextBlock().AlignToInputConnection();
                return;
            }

            // Case 2: This block is LAST (output connected to InternalInput)
            if (primaryOutput?.connectedTo?.role == BlockConnector.ConnectorRole.InternalInput)
            {
                var internalInput = primaryOutput.connectedTo;

                // Bypass: (whatever our input is connected to) → IntIn
                internalInput.connectedTo = primaryInput?.connectedTo;
                if (primaryInput?.connectedTo != null)
                {
                    primaryInput.connectedTo.connectedTo = internalInput;
                }

                Debug.Log($"[DRAG] Loop bypassed LAST block: {parentBlock.name}");
                parentBlock.GetNextBlock().AlignToInputConnection();
                return;
            }
        }

        private void Awake()
        {
            parentBlock = GetComponentInParent<BlockUIBase>();
            
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            rootCanvas = GetComponentInParent<Canvas>();
            
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
            }
            programArea ??= GetComponentInParent<ProgramArea>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0.6f;
            }

            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();

            // IMPORTANT: Bypass block in Loop BEFORE disconnecting (collapse Loop connections)
            BypassBlockInLoop();

            // Disconnect from any connected blocks when starting drag (Stage 6)
            parentBlock.DisconnectAllConnections();

            // Move to root canvas for dragging
            if (rootCanvas != null)
            {
                transform.SetParent(rootCanvas.transform, true);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
            }

            // Find nearest snap point for visual feedback (only in program area)
            // If programArea is null (dragging from palette), try to find it in rootCanvas
            if (programArea == null && rootCanvas != null)
            {
                programArea = rootCanvas.GetComponentInChildren<ProgramArea>();
            }

            if (programArea != null && programArea.GetBlocks().Count > 0)
            {
                SnapManager snapManager = programArea.GetSnapManager();
                if (snapManager != null)
                {
                    // Use unified FindNearestSnap for visual feedback
                    SnapManager.SnapInfo snapInfo = snapManager.FindNearestSnap(parentBlock, programArea.GetBlocks());

                    // ONLY visual feedback during drag - NO position application!
                    // Update visual feedback (yellow highlight when ready)
                    parentBlock.UpdateSnapVisuals(snapInfo);
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
            }

            // Reset connector colors to normal
            parentBlock.ResetAllConnectorColors();

            // Palette blocks should NOT apply snap in OnEndDrag
            // They are handled by ProgramArea.OnDrop() which creates a copy
            if (!parentBlock.inProgramArea)
            {
                ReturnToOriginalPosition();
                return;
            }

            // Check if we can apply snap (ONLY for blocks already in program)
            // If programArea is null (dragging from palette), try to find it in rootCanvas
            if (programArea == null && rootCanvas != null)
            {
                programArea = rootCanvas.GetComponentInChildren<ProgramArea>();
            }

            if (parentBlock.Command.Type == CommandType.Loop)
            {
                var loopBlock = (LoopBlockUI)parentBlock;
                loopBlock.ConnectInnerConnectors();
                loopBlock.AlignToInputConnection();
            }

            if (programArea != null)
            {
                SnapManager snapManager = programArea.GetSnapManager();
                if (snapManager != null)
                {
                    // Use unified FindNearestSnap that picks smallest distance
                    SnapManager.SnapInfo snapInfo = snapManager.FindNearestSnap(parentBlock, programArea.GetBlocks());

                    if (snapInfo.canSnap)
                    {
                        // Apply snap based on type returned by FindNearestSnap
                        if (snapInfo.snapType == SnapManager.SnapInfo.SnapType.InputToOutput)
                        {
                            // INPUT of dragging block → OUTPUT of target block
                            snapManager.ApplySnap(parentBlock, snapInfo.targetConnector, programArea);
                        }
                        else if (snapInfo.snapType == SnapManager.SnapInfo.SnapType.OutputToInput)
                        {
                            // OUTPUT of dragging block → INPUT of target block
                            snapManager.ApplySnapToInput(parentBlock, snapInfo.targetConnector, programArea);
                        }
                        else if (snapInfo.snapType == SnapManager.SnapInfo.SnapType.InputToInputPoint)
                        {
                            // INPUT of dragging block → InputPoint (start of program, Task #26)
                            snapManager.ApplySnapToInputPoint(parentBlock, snapInfo.inputPointPosition, programArea);
                        }
                    }
                    else
                    {
                        // No snap possible, return block to original position in ProgramArea
                        ReturnToOriginalPosition();
                    }
                }
            }
        }

        public void ReturnToOriginalPosition()
        {
            transform.SetParent(originalParent, true);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }
}
