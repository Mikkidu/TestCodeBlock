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

            // Link with previous block
            if (blocksInProgram.Count > 1)
            {
                BlockUI previousBlock = blocksInProgram[blocksInProgram.Count - 2];
                if (previousBlock.Command != null)
                {
                    previousBlock.Command.Next = command;
                }
            }

            // Position block below the last one
            RectTransform blockRect = blockUI.GetComponent<RectTransform>();
            if (blockRect != null)
            {
                blockRect.SetParent(transform, false);

                Vector2 newPos = Vector2.zero;
                if (blocksInProgram.Count > 1)
                {
                    RectTransform prevRect = blocksInProgram[blocksInProgram.Count - 2].GetComponent<RectTransform>();
                    if (prevRect != null)
                    {
                        newPos.y = prevRect.anchoredPosition.y - prevRect.rect.height - 10f;
                    }
                }
                blockRect.anchoredPosition = newPos;
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
