using PU.Promises;
using RobotProgramming.Core;
using System;
using UnityEngine;

namespace RobotProgramming.Execution
{
    public class CommandExecutor : MonoBehaviour, ICommandExecutor
    {
        public bool IsRunning { get; private set; }
        public float Progress { get; private set; }

        public event Action<ICommand> OnCommandStarted;
        public event Action<ICommand> OnCommandCompleted;
        public event Action OnProgramCompleted;
        public event Action<Exception> OnProgramFailed;

        private bool isPaused;
        private ICommand currentCommand;
        private Deferred pauseDeferred;
        private int totalCommands;
        private int executedCommands;

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

            ExecutionContext context = new ExecutionContext();
            return ExecuteCommandChain(startCommand, robot, context);
        }

        private IPromise ExecuteCommandChain(ICommand command, IRobotController robot, ExecutionContext context)
        {
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
            IsRunning = false;
            isPaused = false;
            currentCommand = null;

            if (pauseDeferred != null)
            {
                pauseDeferred.Reject(new Exception("Execution stopped"));
                pauseDeferred = null;
            }
        }
    }
}
