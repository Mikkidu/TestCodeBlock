using System.Collections.Generic;

using System;
using RobotProgramming.Core;
using UnityEngine;

namespace RobotProgramming.UI
{
    public class SnapManager : MonoBehaviour
    {
        public event Action<ICommand> OnSnap;
        
        [SerializeField] private float snapDistance = 50f;

        public struct SnapInfo
        {
            public enum SnapType { None, OutputToInput, InputToOutput }

            public BlockUI targetBlock;
            public BlockConnector targetConnector;
            public SnapType snapType;
            public bool canSnap;
            public float distance;
        }

        // Find the nearest output point to the input point of a dragging block
        public SnapInfo FindNearestOutput(BlockUI draggingBlock, List<BlockUI> allBlocks)
        {
            if (draggingBlock == null || draggingBlock.inputPoints.Count == 0)
            {
                return new SnapInfo
                {
                    targetBlock = null,
                    targetConnector = null,
                    snapType = SnapInfo.SnapType.None,
                    canSnap = false,
                    distance = float.MaxValue
                };
            }

            BlockConnector inputPoint = draggingBlock.inputPoints[0];
            if (inputPoint == null)
            {
                return new SnapInfo
                {
                    targetBlock = null,
                    targetConnector = null,
                    snapType = SnapInfo.SnapType.None,
                    canSnap = false,
                    distance = float.MaxValue
                };
            }

            Vector2 inputPosition = inputPoint.GetWorldPosition();
            float minDistance = float.MaxValue;
            BlockConnector nearestOutput = null;
            BlockUI targetBlock = null;

            // Search through all blocks in the program
            foreach (BlockUI block in allBlocks)
            {
                // Skip the dragging block itself
                if (block == draggingBlock)
                    continue;

                // Check all output points of this block
                foreach (BlockConnector output in block.outputPoints)
                {
                    if (output == null)
                        continue;

                    Vector2 outputPosition = output.GetWorldPosition();
                    float distance = Vector2.Distance(inputPosition, outputPosition);

                    // Keep track of the nearest output
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestOutput = output;
                        targetBlock = block;
                    }
                }
            }

            // Check if snap is valid (within snapDistance)
            bool canSnap = minDistance <= snapDistance && nearestOutput != null;

            return new SnapInfo
            {
                targetBlock = targetBlock,
                targetConnector = nearestOutput,
                snapType = canSnap ? SnapInfo.SnapType.InputToOutput : SnapInfo.SnapType.None,
                canSnap = canSnap,
                distance = minDistance
            };
        }

        // Find the nearest input point to the output point of a dragging block
        public SnapInfo FindNearestInput(BlockUI draggingBlock, List<BlockUI> allBlocks)
        {
            if (draggingBlock == null || draggingBlock.outputPoints.Count == 0)
            {
                return new SnapInfo
                {
                    targetBlock = null,
                    targetConnector = null,
                    snapType = SnapInfo.SnapType.None,
                    canSnap = false,
                    distance = float.MaxValue
                };
            }

            BlockConnector outputPoint = draggingBlock.outputPoints[0];
            if (outputPoint == null)
            {
                return new SnapInfo
                {
                    targetBlock = null,
                    targetConnector = null,
                    snapType = SnapInfo.SnapType.None,
                    canSnap = false,
                    distance = float.MaxValue
                };
            }

            Vector2 outputPosition = outputPoint.GetWorldPosition();
            float minDistance = float.MaxValue;
            BlockConnector nearestInput = null;
            BlockUI targetBlock = null;

            // Search through all blocks in the program
            foreach (BlockUI block in allBlocks)
            {
                // Skip the dragging block itself
                if (block == draggingBlock)
                    continue;

                // Check all input points of this block
                foreach (BlockConnector input in block.inputPoints)
                {
                    if (input == null)
                        continue;

                    Vector2 inputPosition = input.GetWorldPosition();
                    float distance = Vector2.Distance(outputPosition, inputPosition);

                    // Keep track of the nearest input
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestInput = input;
                        targetBlock = block;
                    }
                }
            }

            // Check if snap is valid (within snapDistance)
            bool canSnap = minDistance <= snapDistance && nearestInput != null;

            return new SnapInfo
            {
                targetBlock = targetBlock,
                targetConnector = nearestInput,
                snapType = canSnap ? SnapInfo.SnapType.OutputToInput : SnapInfo.SnapType.None,
                canSnap = canSnap,
                distance = minDistance
            };
        }

        // Find which output connector is connected to the given input (Stage 6b - mid-chain insertion)
        private BlockConnector FindConnectedOutput(BlockConnector targetInput, List<BlockUI> allBlocks)
        {
            if (targetInput == null) return null;

            // Search through all blocks to find which OUTPUT is connected to this INPUT
            foreach (BlockUI block in allBlocks)
            {
                foreach (BlockConnector output in block.outputPoints)
                {
                    if (output.connectedTo == targetInput)
                    {
                        return output;  // Found the source output
                    }
                }
            }

            return null;  // No incoming connection
        }

        // Apply snap to position the dragging block with input aligned to target output
        public void ApplySnap(BlockUI draggingBlock, BlockConnector inputPoint, BlockConnector targetOutput)
        {
            if (draggingBlock == null || inputPoint == null || targetOutput == null)
            {
                return;
            }

            // Get the world position where we want the input point to be
            Vector2 targetPosition = targetOutput.GetWorldPosition();

            // Get the current world position of the input point
            Vector2 currentInputWorldPos = inputPoint.GetWorldPosition();

            // Calculate the offset we need to move the block
            Vector2 offset = targetPosition - currentInputWorldPos;

            // Apply the offset to the block's position in world space
            RectTransform blockRect = draggingBlock.GetComponent<RectTransform>();
            if (blockRect != null)
            {
                blockRect.position = new Vector3(
                    blockRect.position.x + offset.x,
                    blockRect.position.y + offset.y,
                    blockRect.position.z
                );
            }

            // Create physical connection between blocks (Stage 6)
            targetOutput.connectedTo = inputPoint;

            // Return block to ProgramArea if it was moved to rootCanvas during drag
            if (draggingBlock.inProgramArea)
            {
                ProgramArea programArea = draggingBlock.GetComponentInParent<ProgramArea>();
                if (programArea != null)
                {
                    draggingBlock.transform.SetParent(programArea.transform, true);
                    Debug.Log($"[RETURN TO PROGRAM] {draggingBlock.gameObject.name} returned to ProgramArea");
                }
            }

            OnSnap?.Invoke(draggingBlock.Command);
            Debug.Log($"[CONNECTION INPUT→OUTPUT] {targetOutput.parentBlock.gameObject.name} → {draggingBlock.gameObject.name}");
        }

        // Apply snap to position the dragging block with output aligned to target input
        public void ApplySnapToInput(BlockUI draggingBlock, BlockConnector outputPoint, BlockConnector targetInput)
        {
            if (draggingBlock == null || outputPoint == null || targetInput == null)
            {
                return;
            }

            // Stage 6b: Handle insertion into middle of chain
            // Check if there's already an OUTPUT connected to targetInput
            BlockConnector previousOutput = FindConnectedOutput(targetInput,
                draggingBlock.GetComponentInParent<ProgramArea>()?.GetBlocks() ?? new List<BlockUI>());

            if (previousOutput != null)
            {
                // INSERTION INTO MIDDLE: A → C → B
                // Step 1: Position draggingBlock (C) so its INPUT aligns with previousOutput (A's OUTPUT)
                if (draggingBlock.inputPoints.Count > 0)
                {
                    BlockConnector draggingInput = draggingBlock.inputPoints[0];
                    Vector2 aOutputPos = previousOutput.GetWorldPosition();
                    Vector2 cInputPos = draggingInput.GetWorldPosition();
                    Vector2 offsetForC = aOutputPos - cInputPos;

                    RectTransform cRect = draggingBlock.GetComponent<RectTransform>();
                    if (cRect != null)
                    {
                        cRect.position = new Vector3(
                            cRect.position.x + offsetForC.x,
                            cRect.position.y + offsetForC.y,
                            cRect.position.z
                        );
                    }

                    // Step 2: Reconnect A → C
                    previousOutput.connectedTo = draggingInput;
                    Debug.Log($"[RECONNECT] {previousOutput.parentBlock.gameObject.name} → {draggingBlock.gameObject.name}");
                }

                // Step 3: Position targetBlock (B) so its INPUT aligns with draggingBlock's OUTPUT (C's OUTPUT)
                BlockUI targetBlock = targetInput.parentBlock;
                Vector2 cOutputPos = outputPoint.GetWorldPosition();
                Vector2 bInputPos = targetInput.GetWorldPosition();
                Vector2 offsetForB = cOutputPos - bInputPos;

                RectTransform bRect = targetBlock.GetComponent<RectTransform>();
                if (bRect != null)
                {
                    bRect.position = new Vector3(
                        bRect.position.x + offsetForB.x,
                        bRect.position.y + offsetForB.y,
                        bRect.position.z
                    );

                    // Step 4: Trigger cascade alignment for all blocks after B (C, D, E...)
                    // Each block aligns to its input connection and propagates to next
                    BlockUI nextBlock = targetBlock.GetNextBlock();
                    if (nextBlock != null)
                    {
                        nextBlock.AlignToInputConnection();
                    }
                }

                Debug.Log($"[DISCONNECT FOR INSERT] {previousOutput.parentBlock.gameObject.name} → {targetBlock.gameObject.name}");
            }
            else
            {
                // INSERT AT BEGINNING: X → A (no previous block connected to A's input)
                // Position draggingBlock so its OUTPUT aligns with targetInput (A's INPUT)
                Vector2 targetPosition = targetInput.GetWorldPosition();
                Vector2 currentOutputWorldPos = outputPoint.GetWorldPosition();
                Vector2 offset = targetPosition - currentOutputWorldPos;

                RectTransform blockRect = draggingBlock.GetComponent<RectTransform>();
                if (blockRect != null)
                {
                    blockRect.position = new Vector3(
                        blockRect.position.x + offset.x,
                        blockRect.position.y + offset.y,
                        blockRect.position.z
                    );
                }
                Debug.Log($"[POSITION AT START] {draggingBlock.gameObject.name} OUTPUT aligned with {targetInput.parentBlock.gameObject.name} INPUT");
            }

            // Create physical connection: dragging block's OUTPUT → target INPUT
            outputPoint.connectedTo = targetInput;

            // Return block to ProgramArea if it was moved to rootCanvas during drag
            if (draggingBlock.inProgramArea)
            {
                ProgramArea programArea = draggingBlock.GetComponentInParent<ProgramArea>();
                if (programArea != null)
                {
                    draggingBlock.transform.SetParent(programArea.transform, true);
                    Debug.Log($"[RETURN TO PROGRAM] {draggingBlock.gameObject.name} returned to ProgramArea");
                }
            }

            OnSnap?.Invoke(draggingBlock.Command);
            Debug.Log($"[CONNECTION OUTPUT→INPUT] {draggingBlock.gameObject.name} → {targetInput.parentBlock.gameObject.name}");
        }

        // Shift all blocks in chain starting from targetBlock down by offsetY
        // Used when inserting a block in the middle to prevent overlapping
        private void ShiftBlockChain(BlockUI targetBlock, float offsetY)
        {
            if (targetBlock == null || Mathf.Abs(offsetY) < 0.1f)
            {
                return;  // No shift needed
            }

            BlockUI currentBlock = targetBlock;
            while (currentBlock != null)
            {
                RectTransform blockRect = currentBlock.GetComponent<RectTransform>();
                if (blockRect != null)
                {
                    // Shift this block down by offsetY
                    blockRect.anchoredPosition = new Vector2(
                        blockRect.anchoredPosition.x,
                        blockRect.anchoredPosition.y - offsetY  // Negative because Y axis is inverted in UI
                    );
                    Debug.Log($"[SHIFT] {currentBlock.gameObject.name} shifted by {offsetY}px down");
                }

                // Move to next block in chain
                currentBlock = currentBlock.GetNextBlock();
            }
        }

        // Get the snap distance for UI feedback
        public float GetSnapDistance()
        {
            return snapDistance;
        }

        // Set custom snap distance
        public void SetSnapDistance(float distance)
        {
            snapDistance = Mathf.Max(0, distance);
        }
    }
}
