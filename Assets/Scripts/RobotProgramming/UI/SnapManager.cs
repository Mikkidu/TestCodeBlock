using System.Collections.Generic;

using System;
using System.Linq;
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

            public BlockUIBase targetBlock;
            public BlockConnector targetConnector;
            public SnapType snapType;
            public bool canSnap;
            public float distance;
        }

        // Find the nearest snap point by comparing INPUT→OUTPUT and OUTPUT→INPUT distances
        // LOGIC:
        // 1. Calculate distance from INPUT of dragging block to OUTPUT of all blocks (including Loop InternalOutput)
        // 2. Calculate distance from OUTPUT of dragging block to INPUT of blocks without incoming connection
        // 3. Choose the snap type with SMALLER distance (INPUT→OUTPUT on tie)
        // STEP 1b: Also check Loop blocks' InternalOutput coннекторы for internal block insertion
        public SnapInfo FindNearestSnap(BlockUIBase draggingBlock, List<BlockUIBase> allBlocks)
        {
            if (draggingBlock == null)
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

            // STEP 1: Find nearest OUTPUT to INPUT of dragging block
            float nearestInputToOutputDist = float.MaxValue;
            BlockConnector nearestOutput = null;
            BlockUIBase targetBlockForOutput = null;

            if (draggingBlock.GetInputConnectors().Any())
            {
                BlockConnector inputPoint = draggingBlock.GetInputConnectors().First();
                if (inputPoint != null)
                {
                    Vector2 inputPosition = inputPoint.GetWorldPosition();

                    // Check regular blocks' outputs
                    foreach (BlockUIBase block in allBlocks)
                    {
                        if (block == draggingBlock)
                            continue;

                        foreach (BlockConnector output in block.GetOutputConnectors().ToArray())
                        {
                            if (output == null)
                                continue;

                            Vector2 outputPosition = output.GetWorldPosition();
                            float distance = Vector2.Distance(inputPosition, outputPosition);

                            if (distance < nearestInputToOutputDist && distance <= snapDistance)
                            {
                                nearestInputToOutputDist = distance;
                                nearestOutput = output;
                                targetBlockForOutput = block;
                            }
                        }
                    }

                    // STEP 1b: Check Loop blocks' InternalOutput connectors
                    foreach (BlockUIBase block in allBlocks)
                    {
                        // CRITICAL: Exclude dragging block from snap search (prevent self-connection)
                        if (block == draggingBlock)
                            continue;

                        LoopBlockUI loopUI = block.GetComponent<LoopBlockUI>();
                        if (loopUI != null && loopUI.InternalOutput != null)
                        {
                            Vector2 internalOutputPos = loopUI.InternalOutput.GetWorldPosition();
                            float distance = Vector2.Distance(inputPosition, internalOutputPos);

                            if (distance < nearestInputToOutputDist && distance <= snapDistance)
                            {
                                nearestInputToOutputDist = distance;
                                nearestOutput = loopUI.InternalOutput;
                                targetBlockForOutput = block;
                                Debug.Log($"[SNAP] Found Loop InternalOutput at distance {distance}");
                            }
                        }
                    }
                }
            }

            // STEP 2: Find nearest INPUT to OUTPUT of dragging block (ALL blocks, including middle of chain)
            float nearestOutputToInputDist = float.MaxValue;
            BlockConnector nearestInput = null;
            BlockUIBase targetBlockForInput = null;

            var dragOutputs = draggingBlock.GetOutputConnectors();
            if (dragOutputs.Any())
            {
                BlockConnector outputPoint = dragOutputs.First();
                if (outputPoint != null)
                {
                    Vector2 outputPosition = outputPoint.GetWorldPosition();

                    foreach (BlockUIBase block in allBlocks)
                    {
                        if (block == draggingBlock)
                            continue;

                        // Check ALL inputs of this block (both beginning and middle of chain)
                        foreach (BlockConnector input in block.GetInputConnectors())
                        {
                            if (input == null)
                                continue;

                            Vector2 inputPosition = input.GetWorldPosition();
                            float distance = Vector2.Distance(outputPosition, inputPosition);

                            if (distance < nearestOutputToInputDist && distance <= snapDistance)
                            {
                                nearestOutputToInputDist = distance;
                                nearestInput = input;
                                targetBlockForInput = block;
                            }
                        }
                    }
                }
            }

            // STEP 3: Choose the snap type with smaller distance
            // SPECIAL CASE: If INPUT→OUTPUT target has outgoing connection (INSERT MIDDLE), prefer it over OUTPUT→INPUT
            bool targetForOutputHasOutgoing = targetBlockForOutput != null && nearestOutput != null && nearestOutput.connectedTo != null;

            // Priority: INPUT→OUTPUT if target has outgoing connection (INSERT MIDDLE case via ApplySnap)
            if (targetForOutputHasOutgoing && nearestInputToOutputDist <= snapDistance && nearestOutput != null)
            {
                // INSERT MIDDLE: prefer INPUT→OUTPUT (will be handled by ApplySnap) even if OUTPUT→INPUT is closer
                return new SnapInfo
                {
                    targetBlock = targetBlockForOutput,
                    targetConnector = nearestOutput,
                    snapType = SnapInfo.SnapType.InputToOutput,
                    canSnap = true,
                    distance = nearestInputToOutputDist
                };
            }
            // Otherwise: choose by smaller distance
            else if (nearestInputToOutputDist < nearestOutputToInputDist)
            {
                // INPUT→OUTPUT is closer
                bool canSnap = nearestInputToOutputDist <= snapDistance && nearestOutput != null;
                return new SnapInfo
                {
                    targetBlock = targetBlockForOutput,
                    targetConnector = nearestOutput,
                    snapType = canSnap ? SnapInfo.SnapType.InputToOutput : SnapInfo.SnapType.None,
                    canSnap = canSnap,
                    distance = nearestInputToOutputDist
                };
            }
            else if (nearestOutputToInputDist < float.MaxValue)
            {
                // OUTPUT→INPUT is closer
                bool canSnap = nearestOutputToInputDist <= snapDistance && nearestInput != null;
                return new SnapInfo
                {
                    targetBlock = targetBlockForInput,
                    targetConnector = nearestInput,
                    snapType = canSnap ? SnapInfo.SnapType.OutputToInput : SnapInfo.SnapType.None,
                    canSnap = canSnap,
                    distance = nearestOutputToInputDist
                };
            }
            else if (nearestInputToOutputDist < float.MaxValue)
            {
                // Fallback: INPUT→OUTPUT if nothing else works
                bool canSnap = nearestInputToOutputDist <= snapDistance && nearestOutput != null;
                return new SnapInfo
                {
                    targetBlock = targetBlockForOutput,
                    targetConnector = nearestOutput,
                    snapType = canSnap ? SnapInfo.SnapType.InputToOutput : SnapInfo.SnapType.None,
                    canSnap = canSnap,
                    distance = nearestInputToOutputDist
                };
            }
            else
            {
                // No snap found
                return new SnapInfo
                {
                    targetBlock = null,
                    targetConnector = null,
                    snapType = SnapInfo.SnapType.None,
                    canSnap = false,
                    distance = float.MaxValue
                };
            }
        }

        // Apply snap to position the dragging block with input aligned to target output
        public void ApplySnap(BlockUIBase draggingBlock, BlockConnector targetOutput, ProgramArea programArea = null)
        {
            if (draggingBlock == null || targetOutput == null) return;
            
            var inputPoint = draggingBlock.GetPrimaryInput();
            
            if (inputPoint == null)
            {
                return;
            }
            
            // bug: parent block is empty for loop inner output 
            BlockUIBase targetBlock = targetOutput.parentBlock;

            // Use provided programArea, or try to find it
            if (programArea == null)
            {
                programArea = draggingBlock.GetComponentInParent<ProgramArea>();
            }
            
            // question: newer call?
            List<BlockUIBase> allBlocks = programArea?.GetBlocks() ?? new List<BlockUIBase>();

            // Log state before snap
            string targetBlockName = targetBlock != null ? targetBlock.gameObject.name : "LOOP_INTERNAL";
            Debug.Log($"[SNAP] {draggingBlock.gameObject.name}.input ← {targetBlockName}.output");
            LogBlockState(draggingBlock, "before", "dragging");
            if (targetBlock != null)
            {
                LogBlockState(targetBlock, "before", "target");
            }

            inputPoint.connectedTo = targetOutput;
            // Check if targetOutput already has something connected (INSERT MIDDLE case)
            BlockConnector oldConnection = targetOutput.connectedTo;

            if (oldConnection != null && oldConnection != inputPoint)
            {
                // INSERT MIDDLE case
                BlockUIBase blockB = oldConnection.parentBlock;

                // Check if this is a Loop InternalOutput (special case for Loop insertion)
                bool isLoopInternalOutput = targetOutput.role == BlockConnector.ConnectorRole.InternalOutput && targetOutput.parentBlock as LoopBlockUI != null;

                if (isLoopInternalOutput)
                {
                    Debug.Log($"[MODE] INSERT MIDDLE LOOP: Loop.InternalOutput → {draggingBlock.gameObject.name} → {blockB.gameObject.name}");
                }
                else
                {
                    Debug.Log($"[MODE] INSERT MIDDLE: {targetBlock.gameObject.name} → {draggingBlock.gameObject.name} → {blockB.gameObject.name}");
                }

                // Step 1: Position dragging block (C) so its INPUT aligns with A's OUTPUT
                Vector2 targetPosition = targetOutput.GetWorldPosition();
                Vector2 currentInputWorldPos = inputPoint.GetWorldPosition();
                Vector2 offsetForC = targetPosition - currentInputWorldPos;

                RectTransform cRect = draggingBlock.GetComponent<RectTransform>();
                if (cRect != null && offsetForC.magnitude > 0.1f)
                {
                    cRect.position = new Vector3(
                        cRect.position.x + offsetForC.x,
                        cRect.position.y + offsetForC.y,
                        cRect.position.z
                    );
                    Debug.Log($"  → Shift {draggingBlock.gameObject.name} by ({offsetForC.x:F1}, {offsetForC.y:F1})");
                }

                // Step 2: Reconnect A → C
                targetOutput.connectedTo = inputPoint;

                // Step 3: Connect C's output to B's input and position B
                var dragOutputs = draggingBlock.GetOutputConnectors();
                if (dragOutputs.Any())
                {
                    BlockConnector draggingOutput = dragOutputs.First();
                    draggingOutput.connectedTo = oldConnection;
                    oldConnection.connectedTo = draggingOutput;

                    blockB.AlignToInputConnection();
                }
            }
            else
            {
                // Simple case: just append at end
                Debug.Log($"[MODE] APPEND: {draggingBlock.gameObject.name} → {targetBlock.gameObject.name}");

                // Get the world position where we want the input point to be
                Vector2 targetPosition = targetOutput.GetWorldPosition();

                // Get the current world position of the input point
                Vector2 currentInputWorldPos = inputPoint.GetWorldPosition();

                // Calculate the offset we need to move the block
                Vector2 offset = targetPosition - currentInputWorldPos;

                // Apply the offset to the block's position in world space
                RectTransform blockRect = draggingBlock.GetComponent<RectTransform>();
                if (blockRect != null && offset.magnitude > 0.1f)
                {
                    blockRect.position = new Vector3(
                        blockRect.position.x + offset.x,
                        blockRect.position.y + offset.y,
                        blockRect.position.z
                    );
                    Debug.Log($"  → Shift {draggingBlock.gameObject.name} by ({offset.x:F1}, {offset.y:F1})");
                }

                // Create physical connection between blocks
                targetOutput.connectedTo = inputPoint;
            }

            // Log state after snap
            LogBlockState(draggingBlock, "after", "dragging");
            if (targetBlock != null)
            {
                LogBlockState(targetBlock, "after", "target");
            }

            // Return block to ProgramArea if it was moved to rootCanvas during drag
            if (draggingBlock.inProgramArea && programArea != null)
            {
                draggingBlock.transform.SetParent(programArea.transform, true);
            }

            OnSnap?.Invoke(draggingBlock.Command);
        }

        // Apply snap to position the dragging block with output aligned to target input
        public void ApplySnapToInput(BlockUIBase draggingBlock, BlockConnector targetInput, ProgramArea programArea = null)
        {
            if (draggingBlock == null || targetInput == null)
                return;
            
            var outputPoint = draggingBlock.GetPrimaryOutput();
            
            if (outputPoint == null)
                return;

            BlockUIBase targetBlock = targetInput.parentBlock;

            // Use provided programArea, or try to find it
            if (programArea == null)
            {
                programArea = draggingBlock.GetComponentInParent<ProgramArea>();
            }

            List<BlockUIBase> allBlocks = programArea?.GetBlocks() ?? new List<BlockUIBase>();

            // Log state before snap
            Debug.Log($"[SNAP] {draggingBlock.gameObject.name}.output → {targetBlock.gameObject.name}.input");
            LogBlockState(draggingBlock, "before", "dragging");
            LogBlockState(targetBlock, "before", "target");

            // Check if there's already an OUTPUT connected to targetInput (means insert into middle)
            BlockConnector previousOutput = targetInput.connectedTo; //FindConnectedOutput(targetInput, allBlocks);

            if (previousOutput != null)
            {
                // INSERTION INTO MIDDLE: A → [C] → B
                BlockUIBase blockA = previousOutput.parentBlock;
                Debug.Log($"[MODE] INSERT MIDDLE: {blockA.gameObject.name} → {draggingBlock.gameObject.name} → {targetBlock.gameObject.name}");

                // Step 1: Position dragging block (C) so its INPUT aligns with A's OUTPUT
                var dragInputs = draggingBlock.GetInputConnectors();
                if (dragInputs.Any())
                {
                    BlockConnector draggingInput = dragInputs.First();
                    Vector2 aOutputPos = previousOutput.GetWorldPosition();
                    Vector2 cInputPos = draggingInput.GetWorldPosition();
                    Vector2 offsetForC = aOutputPos - cInputPos;

                    RectTransform cRect = draggingBlock.GetComponent<RectTransform>();
                    if (cRect != null && offsetForC.magnitude > 0.1f)
                    {
                        cRect.position = new Vector3(
                            cRect.position.x + offsetForC.x,
                            cRect.position.y + offsetForC.y,
                            cRect.position.z
                        );
                        Debug.Log($"  → Shift {draggingBlock.gameObject.name} by ({offsetForC.x:F1}, {offsetForC.y:F1})");
                    }

                    // Step 2: Reconnect A → C
                    previousOutput.connectedTo = draggingInput;
                }

                // Step 3: Position target block (B) so its INPUT aligns with C's OUTPUT
                Vector2 cOutputPos = outputPoint.GetWorldPosition();
                Vector2 bInputPos = targetInput.GetWorldPosition();
                Vector2 offsetForB = cOutputPos - bInputPos;

                RectTransform bRect = targetBlock.GetComponent<RectTransform>();
                if (bRect != null && offsetForB.magnitude > 0.1f)
                {
                    bRect.position = new Vector3(
                        bRect.position.x + offsetForB.x,
                        bRect.position.y + offsetForB.y,
                        bRect.position.z
                    );
                    Debug.Log($"  → Shift {targetBlock.gameObject.name} by ({offsetForB.x:F1}, {offsetForB.y:F1})");

                    // Step 4: Cascade alignment for all blocks after B
                    BlockUIBase nextBlock = targetBlock.GetNextBlock();
                    if (nextBlock != null)
                    {
                        nextBlock.AlignToInputConnection();
                    }
                }
            }
            else
            {
                // INSERT AT BEGINNING: [C] → A
                Debug.Log($"[MODE] INSERT START: {draggingBlock.gameObject.name} → {targetBlock.gameObject.name}");

                // Position dragging block so its OUTPUT aligns with A's INPUT
                Vector2 targetPosition = targetInput.GetWorldPosition();
                Vector2 currentOutputWorldPos = outputPoint.GetWorldPosition();
                Vector2 offset = targetPosition - currentOutputWorldPos;

                RectTransform blockRect = draggingBlock.GetComponent<RectTransform>();
                if (blockRect != null && offset.magnitude > 0.1f)
                {
                    blockRect.position = new Vector3(
                        blockRect.position.x + offset.x,
                        blockRect.position.y + offset.y,
                        blockRect.position.z
                    );
                    Debug.Log($"  → Shift {draggingBlock.gameObject.name} by ({offset.x:F1}, {offset.y:F1})");
                }
            }

            // Create physical connection: dragging block's OUTPUT → target INPUT
            outputPoint.connectedTo = targetInput;

            // Log state after snap
            LogBlockState(draggingBlock, "after", "dragging");
            LogBlockState(targetBlock, "after", "target");

            // Return block to ProgramArea if it was moved to rootCanvas during drag
            if (draggingBlock.inProgramArea && programArea != null)
            {
                draggingBlock.transform.SetParent(programArea.transform, true);
            }

            OnSnap?.Invoke(draggingBlock.Command);
        }

        private void LogBlockState(BlockUIBase block, string state, string role)
        {
            if (block == null) return;
            var inputPoints = block.GetInputConnectors();
            Vector2 inputPos = inputPoints.Count() > 0 ? inputPoints.First().GetWorldPosition() : Vector2.zero;
            var outputPoints = block.GetOutputConnectors();
            Vector2 outputPos = outputPoints.Count() > 0 ? outputPoints.First().GetWorldPosition() : Vector2.zero;

            Debug.Log($"  {role} {state}: {block.gameObject.name} input:({inputPos.x:F0},{inputPos.y:F0}) output:({outputPos.x:F0},{outputPos.y:F0})");
        }

        // Shift all blocks in chain starting from targetBlock down by offsetY
        // Used when inserting a block in the middle to prevent overlapping
        private void ShiftBlockChain(BlockUIBase targetBlock, float offsetY)
        {
            if (targetBlock == null || Mathf.Abs(offsetY) < 0.1f)
            {
                return;  // No shift needed
            }

            BlockUIBase currentBlock = targetBlock;
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
