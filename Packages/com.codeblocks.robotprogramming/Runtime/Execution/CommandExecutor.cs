using PU.Promises;
using System;
using CodeBlocks.Core;
using CodeBlocks.UI;
using UnityEngine;

namespace CodeBlocks.Execution
{
    public class CommandExecutor : MonoBehaviour, ICommandExecutor
    {
        public bool IsRunning { get; private set; }
        public float Progress { get; private set; }

        public event Action<ICommand> OnCommandStarted;
        public event Action<ICommand> OnCommandCompleted;
        public event Action OnProgramCompleted;
        public event Action OnProgramStopped;
        public event Action<Exception> OnProgramFailed;

        private bool isPaused;
        private ICommand currentCommand;
        private Deferred pauseDeferred;
        private int totalCommands;
        private int executedCommands;
        private ExecutionContext currentContext;
        private string lastStopReason;
        public IMovementDecisionProvider MovementDecisionProvider { get; set; }
        public string LastStopReason => lastStopReason;

        public IPromise ExecuteProgram(ICommand startCommand, IRobotController robot)
        {
            if (IsRunning)
            {
                return Deferred.Rejected(new Exception("Executor already running"));
            }

            IsRunning = true;
            Progress = 0f;
            isPaused = false;
            executedCommands = 0;

            // Count total commands
            totalCommands = CountCommands(startCommand);

            ExecutionContext context = new ExecutionContext
            {
                MovementDecisionProvider = MovementDecisionProvider
            };
            lastStopReason = string.Empty;
            currentContext = context;
            return ExecuteCommandChain(startCommand, robot, context);
        }

        /// <summary>
        /// Stage 6: Execute program via BlockUI connections instead of Command.Next
        /// </summary>
        public IPromise ExecuteProgramFromBlock(BlockUIBase startBlock, IRobotController robot)
        {
            if (IsRunning)
            {
                return Deferred.Rejected(new Exception("Executor already running"));
            }

            if (startBlock == null)
            {
                return Deferred.Rejected(new Exception("No start block provided"));
            }

            IsRunning = true;
            Progress = 0f;
            isPaused = false;
            executedCommands = 0;

            // Count total commands by following connections
            totalCommands = CountCommandsFromBlock(startBlock);

            ExecutionContext context = new ExecutionContext
            {
                MovementDecisionProvider = MovementDecisionProvider
            };
            lastStopReason = string.Empty;
            currentContext = context;
            return ExecuteBlockChain(startBlock, robot, context);
        }

        private IPromise ExecuteCommandChain(ICommand command, IRobotController robot, ExecutionContext context)
        {
            if (context.IsCancelled)
            {
                lastStopReason = context.StopReason;
                IsRunning = false;
                OnProgramStopped?.Invoke();
                return Deferred.Resolved();
            }

            if (command == null)
            {
                // End of chain
                IsRunning = false;
                Progress = 1f;
                OnProgramCompleted?.Invoke();
                return Deferred.Resolved();
            }

            currentCommand = command;
            context.CurrentCommandId = command.Id;
            OnCommandStarted?.Invoke(command);

            Deferred chainDeferred = Deferred.GetFromPool();

            // Check if paused
            IPromise pauseCheck = isPaused ? WaitForResume() : Deferred.Resolved();

            pauseCheck
                .Then(() => command.Execute(robot, context))
                .Done(() =>
                {
                    if (context.IsCancelled)
                    {
                        lastStopReason = context.StopReason;
                        IsRunning = false;
                        OnProgramStopped?.Invoke();
                        chainDeferred.Resolve();
                        return;
                    }

                    executedCommands++;
                    context.CommandsExecuted++;
                    Progress = totalCommands > 0 ? (float)executedCommands / totalCommands : 1f;

                    OnCommandCompleted?.Invoke(command);

                    // Execute next command in chain
                    ExecuteCommandChain(command.Next, robot, context)
                        .Done(() => chainDeferred.Resolve())
                        .Fail(ex => chainDeferred.Reject(ex));
                })
                .Fail(ex =>
                {
                    IsRunning = false;
                    OnProgramFailed?.Invoke(ex);
                    chainDeferred.Reject(ex);
                });

            return chainDeferred;
        }

        private IPromise WaitForResume()
        {
            pauseDeferred = Deferred.GetFromPool();
            return pauseDeferred;
        }

        private int CountCommands(ICommand command)
        {
            int count = 0;
            ICommand current = command;

            while (current != null)
            {
                count++;
                current = current.Next;
                if (count > 10000) // Safety limit
                {
                    Debug.LogWarning("Command chain too long, possible infinite loop");
                    return count;
                }
            }

            return count;
        }

        /// <summary>
        /// Stage 6: Count commands by following BlockUI connections
        /// </summary>
        private int CountCommandsFromBlock(BlockUIBase startBlock)
        {
            int count = 0;
            BlockUIBase current = startBlock;

            while (current != null)
            {
                count++;
                current = current.GetNextBlock();
                if (count > 10000) // Safety limit
                {
                    Debug.LogWarning("Block chain too long, possible infinite loop");
                    return count;
                }
            }

            return count;
        }

        /// <summary>
        /// Stage 6: Execute chain by following BlockUI physical connections
        /// </summary>
        private IPromise ExecuteBlockChain(BlockUIBase block, IRobotController robot, ExecutionContext context)
        {
            if (context.IsCancelled)
            {
                lastStopReason = context.StopReason;
                IsRunning = false;
                OnProgramStopped?.Invoke();
                return Deferred.Resolved();
            }

            if (block == null)
            {
                // End of chain
                IsRunning = false;
                Progress = 1f;
                OnProgramCompleted?.Invoke();
                return Deferred.Resolved();
            }

            ICommand command = block.Command;
            if (command == null)
            {
                Debug.LogError($"Block {block.gameObject.name} has no command!");
                IsRunning = false;
                return Deferred.Rejected(new Exception("Block has no command"));
            }

            currentCommand = command;
            context.CurrentCommandId = command.Id;
            OnCommandStarted?.Invoke(command);

            Deferred chainDeferred = Deferred.GetFromPool();

            // Check if paused
            IPromise pauseCheck = isPaused ? WaitForResume() : Deferred.Resolved();

            pauseCheck
                .Then(() => command.Execute(robot, context))
                .Done(() =>
                {
                    if (context.IsCancelled)
                    {
                        lastStopReason = context.StopReason;
                        IsRunning = false;
                        OnProgramStopped?.Invoke();
                        chainDeferred.Resolve();
                        return;
                    }

                    executedCommands++;
                    context.CommandsExecuted++;
                    Progress = totalCommands > 0 ? (float)executedCommands / totalCommands : 1f;

                    OnCommandCompleted?.Invoke(command);

                    // Navigate to next block via physical connection (Stage 6)
                    BlockUIBase nextBlock = block.GetNextBlock();
                    Debug.Log($"[EXECUTE] {block.gameObject.name} → {(nextBlock != null ? nextBlock.gameObject.name : "END")}");

                    // Execute next block in chain
                    ExecuteBlockChain(nextBlock, robot, context)
                        .Done(() => chainDeferred.Resolve())
                        .Fail(ex => chainDeferred.Reject(ex));
                })
                .Fail(ex =>
                {
                    IsRunning = false;
                    OnProgramFailed?.Invoke(ex);
                    chainDeferred.Reject(ex);
                });

            return chainDeferred;
        }

        public void Pause()
        {
            isPaused = true;
        }

        public void Resume()
        {
            if (isPaused && pauseDeferred != null)
            {
                isPaused = false;
                pauseDeferred.Resolve();
                pauseDeferred = null;
            }
        }

        public void Stop()
        {
            if (!IsRunning) return;

            if (currentContext != null)
            {
                currentContext.Cancel("UserStop");
            }

            // Resume if paused so the chain can reach the IsCancelled check
            isPaused = false;
            if (pauseDeferred != null)
            {
                pauseDeferred.Resolve();
                pauseDeferred = null;
            }
        }
    }
}
