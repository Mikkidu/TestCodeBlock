using CodeBlocks.Core;
using CodeBlocks.Data;
using PU.Promises;
using CodeBlocks.Robot;
using UnityEngine;

namespace CodeBlocks.Commands
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

        public override string GetDisplayName() => $"Wait {duration}s";

        public override Color GetBlockColor() => new Color(0.8f, 0.8f, 0.3f); // Yellow
    }
}
