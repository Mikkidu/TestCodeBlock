using RobotProgramming.Core;
using RobotProgramming.Data;
using RobotProgramming.Execution;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RobotProgramming.UI
{
    public class ProgramArea : MonoBehaviour, IDropHandler
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private BlockFactory blockFactory;

        private ProgramSequence programSequence;
        private List<BlockUI> blocksInProgram = new List<BlockUI>();
        private RectTransform rectTransform;
        private CanvasScaler scaler;
        private SnapManager snapManager;

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
        }

        public void OnDrop(PointerEventData eventData)
        {
            BlockUI droppedBlock = eventData.pointerDrag.GetComponent<BlockUI>();
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
            BlockUI newBlock = blockFactory.CreateBlock(commandType, transform);

            if (newBlock != null)
            {
                // Position the new block at the drop point
                RectTransform newBlockRect = newBlock.GetComponent<RectTransform>();
                RectTransform programAreaRect = GetComponent<RectTransform>();

                if (newBlockRect != null && programAreaRect != null)
                {
                    // Convert world position to local position within ProgramArea
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        programAreaRect,
                        eventData.position,
                        eventData.pressEventCamera,
                        out Vector2 localPoint);

                    newBlockRect.anchoredPosition = localPoint;
                }

                // Add the new block to the program
                AddBlockToProgram(newBlock);

                // Stage 6: Check for snap with existing blocks
                if (blocksInProgram.Count > 1)  // More than just the new block
                {
                    SnapManager snapManager = GetSnapManager();
                    if (snapManager != null && newBlock.inputPoints.Count > 0)
                    {
                        SnapManager.SnapInfo snapInfo = snapManager.FindNearestOutput(newBlock, blocksInProgram);

                        if (snapInfo.canSnap && snapInfo.targetConnector != null)
                        {
                            // Apply snap to align and connect
                            snapManager.ApplySnap(newBlock, newBlock.inputPoints[0], snapInfo.targetConnector);
                            Debug.Log($"[SNAP APPLIED INPUT→OUTPUT] {snapInfo.targetBlock.gameObject.name} → {newBlock.gameObject.name}");
                        }
                        // If snap not possible, block stays at drop position
                    }
                }

                // Return the original block to the palette
                droppedBlock.ReturnToOriginalPosition();
            }
        }

        public void AddBlockToProgram(BlockUI blockUI)
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
        public BlockUI GetFirstBlock()
        {
            // Find the starting block: one that has NO incoming connection
            foreach (BlockUI block in blocksInProgram)
            {
                if (block.inputPoints.Count > 0)
                {
                    BlockConnector inputPoint = block.inputPoints[0];

                    // Check if any other block's output is connected to this input
                    bool hasIncomingConnection = false;
                    foreach (BlockUI otherBlock in blocksInProgram)
                    {
                        if (otherBlock == block) continue;

                        foreach (BlockConnector output in otherBlock.outputPoints)
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

        public void ClearProgram()
        {
            foreach (BlockUI block in blocksInProgram)
            {
                Destroy(block.gameObject);
            }
            blocksInProgram.Clear();
            programSequence.Clear();
        }

        public List<BlockUI> GetBlocks()
        {
            return new List<BlockUI>(blocksInProgram);
        }

        public SnapManager GetSnapManager()
        {
            return snapManager;
        }
    }
}
