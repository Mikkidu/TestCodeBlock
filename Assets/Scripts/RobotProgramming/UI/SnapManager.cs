using System.Collections.Generic;
using UnityEngine;

namespace RobotProgramming.UI
{
    public class SnapManager : MonoBehaviour
    {
        [SerializeField] private float snapDistance = 50f;

        public struct SnapInfo
        {
            public BlockUI targetBlock;
            public BlockConnector targetOutput;
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
                    targetOutput = null,
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
                    targetOutput = null,
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
                targetOutput = nearestOutput,
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
