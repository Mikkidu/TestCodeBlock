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

            // Get the world position where we want the output point to be
            Vector2 targetPosition = targetInput.GetWorldPosition();

            // Get the current world position of the output point
            Vector2 currentOutputWorldPos = outputPoint.GetWorldPosition();

            // Calculate the offset we need to move the block
            Vector2 offset = targetPosition - currentOutputWorldPos;

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
            // Connection direction: dragging block's OUTPUT → target block's INPUT
            outputPoint.connectedTo = targetInput;
            OnSnap?.Invoke(draggingBlock.Command);
            Debug.Log($"[CONNECTION OUTPUT→INPUT] {draggingBlock.gameObject.name} → {targetInput.parentBlock.gameObject.name}");
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
