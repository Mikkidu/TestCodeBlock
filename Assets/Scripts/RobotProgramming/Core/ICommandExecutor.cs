using PU.Promises;
using System;

namespace RobotProgramming.Core
{
    public interface ICommandExecutor
    {
        // State
        bool IsRunning { get; }
        float Progress { get; }

        // Execution control
        IPromise ExecuteProgram(ICommand startCommand, IRobotController robot);
        void Stop();
        void Pause();
        void Resume();

        // Events for UI updates and monitoring
        event Action<ICommand> OnCommandStarted;
        event Action<ICommand> OnCommandCompleted;
        event Action OnProgramCompleted;
        event Action<Exception> OnProgramFailed;
    }
}
