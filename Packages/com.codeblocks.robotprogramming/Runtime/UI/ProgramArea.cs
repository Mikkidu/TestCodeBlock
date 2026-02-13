using System.Collections.Generic;
using System.Linq;
using CodeBlocks.Core;
using CodeBlocks.Data;
using CodeBlocks.Execution;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CodeBlocks.UI
{
    public class ProgramArea : MonoBehaviour, IDropHandler
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private BlockFactory blockFactory;
        [SerializeField] private LoopBlockUI parentLoopBlock;

        [Header("Input Point (Task #26)")]
        [SerializeField] private Transform inputPoint;

        private ProgramSequence programSequence;
        private List<BlockUIBase> blocksInProgram = new List<BlockUIBase>();
        private RectTransform rectTransform;
        private CanvasScaler scaler;
        private SnapManager snapManager;
        public SnapLineRenderer SnapLineRenderer { get; private set;}

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }

            scaler = canvas.GetComponent<CanvasScaler>();

            snapManager = gameObject.AddComponent<SnapManager>();

            programSequence = new ProgramSequence();

            snapManager.OnSnap += programSequence.CheckSnappedCommand;
            gameObject.tag = "DropZone";
            
            SnapLineRenderer = gameObject.GetComponentInParent<SnapLineRenderer>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            BlockUIBase droppedBlock = eventData.pointerDrag.GetComponent<BlockUIBase>();
            if (droppedBlock == null || droppedBlock.Command == null)
            {
                return;
            }

            // If block is already in program, don't process drop
            if (droppedBlock.inProgramArea)
                return;

            if (blockFactory == null)
            {
                Debug.LogError("ProgramArea: BlockFactory not assigned!");
                return;
            }

            // Create new block from the dragged palette block
            CommandType commandType = droppedBlock.Command.Type;
            BlockUIBase newBlock = blockFactory.CreateBlock(commandType, transform);

            if (newBlock != null)
            {
                // Position the new block at the drop point
                newBlock.SetWorldPosition(eventData.position);

                // Add the new block to the program
                AddBlockToProgram(newBlock);

                // Task #26: FIRST BLOCK always snaps to InputPoint
                if (blocksInProgram.Count == 1 && HasInputPoint())
                {
                    Vector3 inputPointPos = GetInputPointWorldPosition();
                    snapManager.ApplySnapToInputPoint(newBlock, inputPointPos, this);
                }
                // Stage 6: Check for snap with existing blocks
                else if (blocksInProgram.Count > 1)  // More than just the new block
                {
                    if (snapManager != null)
                    {
                        // Use unified FindNearestSnap that picks smallest distance
                        SnapManager.SnapInfo snapInfo = snapManager.FindNearestSnap(newBlock, blocksInProgram);

                        if (snapInfo.canSnap)
                        {
                            // Apply snap based on type returned by FindNearestSnap
                            if (snapInfo.snapType == SnapManager.SnapInfo.SnapType.InputToOutput)
                            {
                                // INPUT of new block → OUTPUT of target block
                                snapManager.ApplySnap(newBlock, snapInfo.targetConnector, this);
                            }
                            else if (snapInfo.snapType == SnapManager.SnapInfo.SnapType.OutputToInput)
                            {
                                // OUTPUT of new block → INPUT of target block
                                snapManager.ApplySnapToInput(newBlock, snapInfo.targetConnector, this);
                            }
                            else if (snapInfo.snapType == SnapManager.SnapInfo.SnapType.InputToInputPoint)
                            {
                                // INPUT of new block → InputPoint (start of program, Task #26)
                                snapManager.ApplySnapToInputPoint(newBlock, snapInfo.inputPointPosition, this);
                            }
                        }
                        // If snap not possible, block stays at drop position
                    }
                }
            }
        }

        public void AddBlockToProgram(BlockUIBase blockUI)
        {
            ICommand command = blockUI.Command;

            // Add to sequence
            programSequence.AddCommand(command);
            blocksInProgram.Add(blockUI);

            blockUI.inProgramArea = true;

            // Stage 6: Linking is now done via physical connections (snap), not Y-position
            // Old Command.Next linking removed - execution follows BlockUI.GetNextBlock()

            // Stage 6: Position is set by OnDrop (with snap if applicable), don't override it
            RectTransform blockRect = blockUI.transform as RectTransform;
            if (blockRect != null)
            {
                blockRect.SetParent(transform, false);
                // anchoredPosition already set in OnDrop - keep it!
            }

            // NEW: Trigger height recalculation for parent loop when block is added
            if (parentLoopBlock != null)
            {
                parentLoopBlock.RecalculateSize();
            }

            // Set first block as start
            if (blocksInProgram.Count == 1)
            {
                programSequence.SetStartCommand(command.Id);
            }
        }

        public ICommand GetProgramStartCommand()
        {
            return programSequence.StartCommand;
        }

        /// <summary>
        /// Get the first block in the program for execution.
        /// Stage 6: Find the starting block - one with NO incoming connection.
        /// A block has an incoming connection if another block's output is connected to its input.
        /// </summary>
        public BlockUIBase GetFirstBlock()
        {
            // Find the starting block: one that has NO incoming connection
            foreach (var block in blocksInProgram)
            {
                var inputs = block.GetInputConnectors();
                if (inputs.Any())
                {
                    BlockConnector inputPoint = inputs.First();

                    // Check if any other block's output is connected to this input
                    bool hasIncomingConnection = false;
                    foreach (var otherBlock in blocksInProgram)
                    {
                        if (otherBlock == block) continue;

                        foreach (BlockConnector output in otherBlock.GetOutputConnectors())
                        {
                            if (output.connectedTo == inputPoint)
                            {
                                hasIncomingConnection = true;
                                break;
                            }
                        }

                        if (hasIncomingConnection) break;
                    }

                    if (!hasIncomingConnection)
                    {
                        return block;  // This is the starting block
                    }
                }
            }

            // Fallback: return first block if no block without incoming connection found
            if (blocksInProgram.Count > 0)
            {
                return blocksInProgram[0];
            }

            return null;
        }

        /// <summary>
        /// Get the last block in the program chain.
        /// Traverses from first block to the end of the chain.
        /// Returns null if program is empty.
        /// </summary>
        public BlockUIBase GetLastBlockInChain()
        {
            BlockUIBase first = GetFirstBlock();
            if (first == null) return null;

            BlockUIBase current = first;
            BlockUIBase last = current;

            while (current != null)
            {
                last = current;
                current = current.GetNextBlock();
            }

            return last;
        }

        public void ClearProgram()
        {
            foreach (var block in blocksInProgram)
            {
                Destroy(block.gameObject);
            }
            blocksInProgram.Clear();
            programSequence.Clear();
        }

        public List<BlockUIBase> GetBlocks()
        {
            return new List<BlockUIBase>(blocksInProgram);
        }

        public void SetBlockFactory(BlockFactory factory)
        {
            blockFactory = factory;
        }

        public SnapManager GetSnapManager()
        {
            return snapManager;
        }

        #region InputPoint API (Task #26)

        /// <summary>
        /// Get InputPoint Transform (for manual positioning or attachment)
        /// </summary>
        public Transform GetInputPointTransform()
        {
            return inputPoint;
        }

        /// <summary>
        /// Get InputPoint position in world coordinates
        /// </summary>
        public Vector3 GetInputPointWorldPosition()
        {
            if (inputPoint == null)
            {
                Debug.LogWarning("[ProgramArea] InputPoint not assigned! Using ProgramArea top-left corner.");
                return rectTransform.position;
            }
            return inputPoint.position;
        }

        /// <summary>
        /// Get InputPoint position in screen coordinates
        /// </summary>
        public Vector2 GetInputPointScreenPosition()
        {
            Vector3 worldPos = GetInputPointWorldPosition();

            if (canvas != null && canvas.worldCamera != null)
            {
                return RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos);
            }

            // Fallback for overlay canvas
            return worldPos;
        }

        /// <summary>
        /// Check if InputPoint is assigned
        /// </summary>
        public bool HasInputPoint()
        {
            return inputPoint != null;
        }

        #endregion
    }
}
