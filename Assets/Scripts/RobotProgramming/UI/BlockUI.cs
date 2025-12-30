using RobotProgramming.Core;
using RobotProgramming.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace RobotProgramming.UI
{
    public class BlockUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image blockImage;
        [SerializeField] private TextMeshProUGUI blockLabel;

        // Connection points - assign in Inspector
        [SerializeField] private RectTransform inputPointVisual;
        [SerializeField] private List<RectTransform> outputPointsVisuals = new List<RectTransform>();

        private ICommand command;
        private CanvasGroup canvasGroup;
        private Transform originalParent;
        private int originalSiblingIndex;
        private Canvas rootCanvas;

        // Connection points for snap system
        public List<BlockConnector> inputPoints = new List<BlockConnector>();
        public List<BlockConnector> outputPoints = new List<BlockConnector>();

        public ICommand Command => command;
        public bool inProgramArea = false;
        private ProgramArea programArea;

        // Track highlighted output for cleanup
        private BlockConnector previousHighlightedOutput;


        private void Awake()
        {
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

        public void SetCommand(ICommand cmd)
        {
            command = cmd;

            if (blockLabel != null)
            {
                blockLabel.text = cmd.GetDisplayName();
            }

            if (blockImage != null)
            {
                blockImage.color = cmd.GetBlockColor();
            }
        }

        private void UpdateSnapVisuals(SnapManager.SnapInfo snapInfo)
        {
            if (snapInfo.canSnap && snapInfo.targetConnector != null)
            {
                // Reset previously highlighted connector if different
                if (previousHighlightedOutput != null && previousHighlightedOutput != snapInfo.targetConnector)
                {
                    Image prevImage = previousHighlightedOutput.visualElement.GetComponent<Image>();
                    if (prevImage != null)
                    {
                        // Reset to default color based on type
                        prevImage.color = previousHighlightedOutput.pointType == BlockConnector.PointType.Input
                            ? new Color(0f, 1f, 0f, 1f)  // Green for input
                            : new Color(1f, 0f, 0f, 1f); // Red for output
                    }
                }

                // Highlight based on snap type
                if (snapInfo.snapType == SnapManager.SnapInfo.SnapType.OutputToInput)
                {
                    // Highlight OUTPUT of dragging block (yellow)
                    if (outputPoints.Count > 0 && outputPoints[0]?.visualElement?.GetComponent<Image>() != null)
                    {
                        outputPoints[0].visualElement.GetComponent<Image>().color = new Color(1f, 1f, 0f, 1f);
                    }

                    // Highlight target INPUT (yellow)
                    Image targetImage = snapInfo.targetConnector.visualElement.GetComponent<Image>();
                    if (targetImage != null)
                    {
                        targetImage.color = new Color(1f, 1f, 0f, 1f);
                    }

                    previousHighlightedOutput = snapInfo.targetConnector;
                }
                else if (snapInfo.snapType == SnapManager.SnapInfo.SnapType.InputToOutput)
                {
                    // Highlight INPUT of dragging block (yellow)
                    if (inputPoints.Count > 0 && inputPoints[0]?.visualElement?.GetComponent<Image>() != null)
                    {
                        inputPoints[0].visualElement.GetComponent<Image>().color = new Color(1f, 1f, 0f, 1f);
                    }

                    // Highlight target OUTPUT (yellow)
                    Image targetImage = snapInfo.targetConnector.visualElement.GetComponent<Image>();
                    if (targetImage != null)
                    {
                        targetImage.color = new Color(1f, 1f, 0f, 1f);
                    }

                    previousHighlightedOutput = snapInfo.targetConnector;
                }
            }
            else
            {
                // Reset previously highlighted connector when snap is no longer possible
                if (previousHighlightedOutput != null)
                {
                    Image prevImage = previousHighlightedOutput.visualElement.GetComponent<Image>();
                    if (prevImage != null)
                    {
                        // Reset to default color based on type
                        prevImage.color = previousHighlightedOutput.pointType == BlockConnector.PointType.Input
                            ? new Color(0f, 1f, 0f, 1f)  // Green for input
                            : new Color(1f, 0f, 0f, 1f); // Red for output
                    }
                    previousHighlightedOutput = null;
                }

                ResetConnectorColors();
            }
        }

        private void ResetConnectorColors()
        {
            // Reset input point color to green
            if (inputPoints.Count > 0 && inputPoints[0]?.visualElement?.GetComponent<Image>() != null)
            {
                inputPoints[0].visualElement.GetComponent<Image>().color = new Color(0f, 1f, 0f, 1f); // Green
            }

            // Reset output point colors to red
            foreach (var output in outputPoints)
            {
                Image outputImage = output?.visualElement?.GetComponent<Image>();
                if (outputImage != null)
                {
                    outputImage.color = new Color(1f, 0f, 0f, 1f); // Red
                }
            }
        }

        public void InitializeConnectors()
        {
            inputPoints.Clear();
            outputPoints.Clear();

            // Initialize input point from assigned visual element in Inspector
            if (inputPointVisual != null)
            {
                // Set input point color to green
                Image inputImage = inputPointVisual.GetComponent<Image>();
                if (inputImage != null)
                {
                    inputImage.color = new Color(0f, 1f, 0f, 1f); // Green
                }

                BlockConnector inputConnector = new BlockConnector(BlockConnector.PointType.Input, inputPointVisual);
                inputConnector.parentBlock = this;  // Set owner reference for navigation
                inputPoints.Add(inputConnector);
            }
            else
            {
                Debug.LogWarning($"BlockUI ({gameObject.name}): Input point visual not assigned in Inspector!");
            }

            // Initialize output points from assigned visual elements in Inspector
            foreach (RectTransform outputVisual in outputPointsVisuals)
            {
                if (outputVisual != null)
                {
                    // Set output point color to red
                    Image outputImage = outputVisual.GetComponent<Image>();
                    if (outputImage != null)
                    {
                        outputImage.color = new Color(1f, 0f, 0f, 1f); // Red
                    }

                    BlockConnector outputConnector = new BlockConnector(BlockConnector.PointType.Output, outputVisual);
                    outputConnector.parentBlock = this;  // Set owner reference for navigation
                    outputPoints.Add(outputConnector);
                }
            }

            if (outputPointsVisuals.Count == 0)
            {
                Debug.LogWarning($"BlockUI ({gameObject.name}): No output points assigned in Inspector!");
            }
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

            // Disconnect from any connected blocks when starting drag (Stage 6)
            DisconnectOutput();
            DisconnectInput();  // NEW: Also disconnect incoming connections

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
            ProgramArea snapArea = programArea;
            if (snapArea == null && rootCanvas != null)
            {
                snapArea = rootCanvas.GetComponentInChildren<ProgramArea>();
            }

            if (snapArea != null && snapArea.GetBlocks().Count > 0)
            {
                SnapManager snapManager = snapArea.GetSnapManager();
                if (snapManager != null)
                {
                    SnapManager.SnapInfo snapInfo = new SnapManager.SnapInfo { canSnap = false };

                    // Priority 1: Check OUTPUT → INPUT snap
                    if (outputPoints.Count > 0)
                    {
                        snapInfo = snapManager.FindNearestInput(this, snapArea.GetBlocks());
                    }

                    // Priority 2: Check INPUT → OUTPUT snap
                    if (!snapInfo.canSnap && inputPoints.Count > 0)
                    {
                        snapInfo = snapManager.FindNearestOutput(this, snapArea.GetBlocks());
                    }

                    // Update visual feedback (yellow highlight when ready)
                    UpdateSnapVisuals(snapInfo);

                    if (snapInfo.canSnap && snapInfo.targetConnector != null)
                    {
                        string snapTypeStr = snapInfo.snapType == SnapManager.SnapInfo.SnapType.OutputToInput
                            ? "OUTPUT→INPUT" : "INPUT→OUTPUT";
                        Debug.Log($"[SNAP READY {snapTypeStr}] {gameObject.name} → {snapInfo.targetBlock.gameObject.name} | Distance: {snapInfo.distance:F2}px");
                    }
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

            // Reset previously highlighted output from other blocks
            if (previousHighlightedOutput != null)
            {
                Image prevOutputImage = previousHighlightedOutput.visualElement.GetComponent<Image>();
                if (prevOutputImage != null)
                {
                    prevOutputImage.color = new Color(1f, 0f, 0f, 1f); // Red
                }
                previousHighlightedOutput = null;
            }

            // Reset connector colors to normal
            ResetConnectorColors();

            // Palette blocks should NOT apply snap in OnEndDrag
            // They are handled by ProgramArea.OnDrop() which creates a copy
            if (!inProgramArea)
            {
                ReturnToOriginalPosition();
                return;
            }

            // Check if we can apply snap (ONLY for blocks already in program)
            // If programArea is null (dragging from palette), try to find it in rootCanvas
            ProgramArea snapArea = programArea;
            if (snapArea == null && rootCanvas != null)
            {
                snapArea = rootCanvas.GetComponentInChildren<ProgramArea>();
            }

            if (snapArea != null)
            {
                SnapManager snapManager = snapArea.GetSnapManager();
                if (snapManager != null)
                {
                    SnapManager.SnapInfo snapInfo = new SnapManager.SnapInfo { canSnap = false };

                    // Priority 1: Try OUTPUT → INPUT snap (insert at beginning)
                    if (outputPoints.Count > 0)
                    {
                        snapInfo = snapManager.FindNearestInput(this, snapArea.GetBlocks());

                        if (snapInfo.canSnap && snapInfo.targetConnector != null)
                        {
                            // Apply snap OUTPUT → INPUT
                            snapManager.ApplySnapToInput(this, outputPoints[0], snapInfo.targetConnector);
                            Debug.Log($"[SNAP APPLIED OUTPUT→INPUT] {gameObject.name} → {snapInfo.targetBlock.gameObject.name}");
                            return;
                        }
                    }

                    // Priority 2: Try INPUT → OUTPUT snap (append at end)
                    if (!snapInfo.canSnap && inputPoints.Count > 0)
                    {
                        snapInfo = snapManager.FindNearestOutput(this, snapArea.GetBlocks());

                        if (snapInfo.canSnap && snapInfo.targetConnector != null)
                        {
                            // Apply snap INPUT → OUTPUT
                            snapManager.ApplySnap(this, inputPoints[0], snapInfo.targetConnector);
                            Debug.Log($"[SNAP APPLIED INPUT→OUTPUT] {snapInfo.targetBlock.gameObject.name} → {gameObject.name}");
                            return;
                        }
                    }

                    // No snap possible, return block to original position in ProgramArea
                    ReturnToOriginalPosition();
                }
            }
        }

        public void ReturnToOriginalPosition()
        {
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalSiblingIndex);
        }

        /// <summary>
        /// Get the next block connected to this block's output.
        /// Returns null if no block is connected (end of chain).
        /// Stage 6: Navigate by physical connections, not Y-position.
        /// </summary>
        public BlockUI GetNextBlock()
        {
            // Get the first output point
            if (outputPoints.Count > 0 && outputPoints[0] != null)
            {
                BlockConnector output = outputPoints[0];

                // If there's a connection to another block
                if (output.connectedTo != null)
                {
                    return output.connectedTo.parentBlock;
                }
            }

            return null;  // End of chain
        }

        /// <summary>
        /// Disconnect this block's output from any connected block.
        /// Stage 6: Break physical connections when dragging.
        /// </summary>
        public void DisconnectOutput(int outputIndex = 0)
        {
            if (outputIndex >= 0 && outputIndex < outputPoints.Count)
            {
                outputPoints[outputIndex].connectedTo = null;
                Debug.Log($"[DISCONNECT] {gameObject.name} output {outputIndex}");
            }
        }

        /// <summary>
        /// Disconnect any incoming connection to this block's input.
        /// Finds which block's output is connected to our input and breaks that connection.
        /// Stage 6: Break incoming connections when dragging.
        /// </summary>
        public void DisconnectInput(int inputIndex = 0)
        {
            if (inputIndex >= 0 && inputIndex < inputPoints.Count)
            {
                BlockConnector inputPoint = inputPoints[inputIndex];

                // Find which block's output is connected to this input
                if (programArea != null)
                {
                    foreach (BlockUI block in programArea.GetBlocks())
                    {
                        if (block == this) continue;

                        foreach (BlockConnector output in block.outputPoints)
                        {
                            if (output.connectedTo == inputPoint)
                            {
                                output.connectedTo = null;
                                Debug.Log($"[DISCONNECT INCOMING] {block.gameObject.name} output → {gameObject.name} input");
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Align this block to its input connection and propagate alignment to next block.
        /// Called recursively down the chain after a block is inserted.
        /// </summary>
        public void AlignToInputConnection()
        {
            // Skip if no input points or not in program area
            if (inputPoints.Count == 0 || !inProgramArea)
            {
                return;
            }

            BlockConnector myInput = inputPoints[0];
            if (myInput == null)
            {
                return;
            }

            // Find which OUTPUT is connected to my INPUT
            ProgramArea programArea = GetComponentInParent<ProgramArea>();
            if (programArea == null)
            {
                return;
            }

            BlockConnector connectedOutput = null;
            foreach (BlockUI block in programArea.GetBlocks())
            {
                if (block == this) continue;

                foreach (BlockConnector output in block.outputPoints)
                {
                    if (output.connectedTo == myInput)
                    {
                        connectedOutput = output;
                        break;
                    }
                }

                if (connectedOutput != null) break;
            }

            // If found, align myself to that output
            if (connectedOutput != null)
            {
                Vector2 outputPos = connectedOutput.GetWorldPosition();
                Vector2 myInputPos = myInput.GetWorldPosition();
                Vector2 offset = outputPos - myInputPos;

                RectTransform rect = GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.position = new Vector3(
                        rect.position.x + offset.x,
                        rect.position.y + offset.y,
                        rect.position.z
                    );
                    Debug.Log($"[ALIGN] {gameObject.name} aligned to {connectedOutput.parentBlock.gameObject.name}");
                }

                // Propagate alignment to next block in chain
                BlockUI nextBlock = GetNextBlock();
                if (nextBlock != null)
                {
                    nextBlock.AlignToInputConnection();
                }
            }
        }
    }
}
