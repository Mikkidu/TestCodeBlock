using PU.Promises;
using RobotProgramming.Data;
using UnityEngine;

namespace RobotProgramming.Core
{
    public abstract class CommandBase : ICommand
    {
        public int Id { get; set; }
        public abstract CommandType Type { get; }
        public ICommand Next { get; set; }

        protected float[] parameters;

        public CommandBase(int id, float[] parameters = null)
        {
            Id = id;
            this.parameters = parameters ?? new float[0];
            Next = null;
        }

        public abstract IPromise Execute(IRobotController robot, ExecutionContext context);

        public virtual bool CanExecute(IRobotController robot)
        {
            return robot != null && !robot.IsExecuting;
        }

        public abstract string GetDisplayName();

        public abstract Color GetBlockColor();
    }
}
