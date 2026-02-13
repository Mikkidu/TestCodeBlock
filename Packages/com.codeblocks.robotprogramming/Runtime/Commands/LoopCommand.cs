using CodeBlocks.Core;
using CodeBlocks.Data;
using CodeBlocks.UI;
using PU.Promises;
using UnityEngine;

namespace CodeBlocks.Commands
{
    public class LoopCommand : CommandBase
    {
        public override CommandType Type => CommandType.Loop;

        private LoopBlockUI loopBlockUI;
        private int currentIteration = 0;

        public LoopCommand(int id) : base(id, new float[] { })
        {
        }

        public void SetLoopBlockUI(LoopBlockUI ui)
        {
            loopBlockUI = ui;
        }

        /// <summary>
        /// Main execution method - called when control enters the loop from outside
        /// </summary>
        public override IPromise Execute(IRobotController robot, ExecutionContext context)
        {
            if (context != null && context.IsCancelled)
            {
                return Deferred.Resolved();
            }

            Debug.Log("[LOOP] Entering from EXTERNAL input");
            currentIteration = 0;
            return ExecuteIteration(robot, context);
        }

        /// <summary>
        /// Called when control returns from the last block inside the loop
        /// (via internal INPUT connector)
        /// </summary>
        public IPromise ExecuteFromInternalInput(IRobotController robot, ExecutionContext context)
        {
            if (context != null && context.IsCancelled)
            {
                return Deferred.Resolved();
            }

            Debug.Log($"[LOOP] Returned from iteration {currentIteration}");
            return ExecuteIteration(robot, context);
        }

        /// <summary>
        /// Internal logic: check condition, execute blocks or continue further
        /// </summary>
        private IPromise ExecuteIteration(IRobotController robot, ExecutionContext context)
        {
            Debug.Log($"[LOOP] ExecuteIteration called (iteration {currentIteration})");

            // Check stop condition
            if (context.IsCancelled)
            {
                Debug.Log($"[LOOP] Stopped after {currentIteration} iterations");
                return Deferred.Resolved();
            }

            // Check loop condition (currently infinite - always true)
            if (!CheckCondition())
            {
                Debug.Log($"[LOOP] Condition false, exiting after {currentIteration} iterations");
                return ContinueAfterLoop(robot, context);
            }

            // Get first block inside the loop
            if (loopBlockUI == null)
            {
                Debug.LogError("[LOOP] loopBlockUI is NULL!");
                return ContinueAfterLoop(robot, context);
            }

            BlockUIBase firstInner = loopBlockUI.GetFirstInnerBlock();
            Debug.Log($"[LOOP] GetFirstInnerBlock returned: {(firstInner != null ? firstInner.gameObject.name : "NULL")}");

            if (firstInner == null)
            {
                Debug.Log("[LOOP] Empty loop, continuing to next block");
                return ContinueAfterLoop(robot, context);
            }

            // Start new iteration
            currentIteration++;
            Debug.Log($"[LOOP] Starting iteration {currentIteration}");
            context.SetVariable("loop_iteration", currentIteration - 1);

            // Pass control to the first block inside
            // Blocks will execute in sequence
            // When the last block returns control via internal input connector,
            // a special handler should be called
            return ExecuteInnerChain(firstInner, robot, context);
        }

        /// <summary>
        /// Execute chain of blocks inside the loop
        /// </summary>
        private IPromise ExecuteInnerChain(BlockUIBase startBlock, IRobotController robot, ExecutionContext context)
        {
            if (context != null && context.IsCancelled)
            {
                return Deferred.Resolved();
            }

            if (startBlock == null)
            {
                Debug.Log("[LOOP] No inner blocks to execute");
                return ScheduleNextIteration(robot, context);
            }

            // Internal connector can point back to the loop block itself.
            // Handle it explicitly to avoid re-entering Execute() recursively.
            if (loopBlockUI != null && ReferenceEquals(startBlock, loopBlockUI))
            {
                return ScheduleNextIteration(robot, context);
            }

            ICommand command = startBlock.Command;

            if (command == null)
            {
                Debug.LogError("[LOOP] Block has no command");
                return Deferred.Rejected(new System.Exception("Block has no command"));
            }

            Deferred chainDeferred = Deferred.GetFromPool();

            command.Execute(robot, context)
                .Done(() =>
                {
                    if (context.IsCancelled)
                    {
                        Debug.Log("[LOOP] Stop requested during iteration");
                        chainDeferred.Resolve();
                    }
                    else
                    {
                        BlockUIBase nextBlock = startBlock.GetNextBlock();

                        if (nextBlock != null)
                        {
                            if (loopBlockUI != null && ReferenceEquals(nextBlock, loopBlockUI))
                            {
                                ScheduleNextIteration(robot, context)
                                    .Done(() => chainDeferred.Resolve())
                                    .Fail(ex => chainDeferred.Reject(ex));
                                return;
                            }

                            // There is next block in the chain inside the loop
                            ExecuteInnerChain(nextBlock, robot, context)
                                .Done(() => chainDeferred.Resolve())
                                .Fail(ex => chainDeferred.Reject(ex));
                        }
                        else
                        {
                            // This was the last block inside the loop
                            // Return to loop start via ExecuteIteration
                            Debug.Log("[LOOP] Last block in chain, returning to loop");
                            ScheduleNextIteration(robot, context)
                                .Done(() => chainDeferred.Resolve())
                                .Fail(ex => chainDeferred.Reject(ex));
                        }
                    }
                })
                .Fail(ex => chainDeferred.Reject(ex));

            return chainDeferred;
        }

        /// <summary>
        /// Break recursive synchronous chain and continue loop on next frame.
        /// Required because Promise.Done executes immediately for already-resolved promises.
        /// </summary>
        private IPromise ScheduleNextIteration(IRobotController robot, ExecutionContext context)
        {
            if (context != null && context.IsCancelled)
            {
                return Deferred.Resolved();
            }

            if (Timers.Instance == null)
            {
                return ExecuteIteration(robot, context);
            }

            return Timers.Instance.WaitOneFrame().Then(() => ExecuteIteration(robot, context));
        }

        /// <summary>
        /// Continue execution after exiting the loop
        /// </summary>
        private IPromise ContinueAfterLoop(IRobotController robot, ExecutionContext context)
        {
            if (context != null && context.IsCancelled)
            {
                return Deferred.Resolved();
            }

            BlockUIBase nextBlock = loopBlockUI.GetNextBlock();

            if (nextBlock != null && nextBlock.Command != null)
            {
                Debug.Log("[LOOP] Continuing to next block after loop");
                return nextBlock.Command.Execute(robot, context);
            }

            Debug.Log("[LOOP] No more blocks after loop");
            return Deferred.Resolved();
        }

        /// <summary>
        /// Check loop condition
        /// Currently always true (infinite loop)
        /// In the future: iteration < maxCount, variable condition, etc.
        /// </summary>
        private bool CheckCondition()
        {
            // Infinite loop - always true
            // Can exit only via Stop button or clearing the loop
            return true;
        }

        public override string GetDisplayName() => "Loop (∞)";

        public override Color GetBlockColor() => new Color(0.8f, 0.4f, 0.8f); // Purple
    }
}
