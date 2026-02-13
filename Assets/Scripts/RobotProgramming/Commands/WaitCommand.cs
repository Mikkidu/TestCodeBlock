using PU.Promises;
using RobotProgramming.Core;
using RobotProgramming.Data;
using RobotProgramming.Robot;
using UnityEngine;

namespace RobotProgramming.Commands
{
    public class WaitCommand : CommandBase
    {
        public override CommandType Type => CommandType.Wait;
        private float duration;

        public WaitCommand(int id, float duration = 1f)
            : base(id)
        {
            this.duration = duration;
        }

        public override IPromise Execute(IRobotController robot, ExecutionContext context)
        {
            // Use the Timers singleton to wait
            return Timers.Instance.Wait(duration, progress =>
            {
                // Just waiting, no action needed
            });
        }

        public override string GetDisplayName() => $"Ждать {duration}с";

        public override Color GetBlockColor() => new Color(0.8f, 0.8f, 0.3f); // Yellow
    }
}
