using PU.Promises;
using RobotProgramming.Core;
using RobotProgramming.Data;
using UnityEngine;

namespace RobotProgramming.Commands
{
    public class MoveForwardCommand : CommandBase
    {
        public override CommandType Type => CommandType.MoveForward;

        private float distance;

        public MoveForwardCommand(int id, float distance = 1f)
            : base(id, new float[] { distance })
        {
            this.distance = distance;
        }

        public override IPromise Execute(IRobotController robot, ExecutionContext context)
        {
            return robot.MoveForward(distance);
        }

        public override string GetDisplayName() => $"Вперёд ({distance})";

        public override Color GetBlockColor() => new Color(0.3f, 0.6f, 1f); // Blue
    }
}
