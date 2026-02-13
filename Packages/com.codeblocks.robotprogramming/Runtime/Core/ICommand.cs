using CodeBlocks.Data;
using PU.Promises;
using UnityEngine;

namespace CodeBlocks.Core
{
    public interface ICommand
    {
        int Id { get; }
        CommandType Type { get; }
        ICommand Next { get; set; }

        // Execute command and return promise that resolves when complete
        IPromise Execute(IRobotController robot, ExecutionContext context);

        // Validate if command can be executed
        bool CanExecute(IRobotController robot);

        // UI display helpers
        string GetDisplayName();
        Color GetBlockColor();
    }
}
