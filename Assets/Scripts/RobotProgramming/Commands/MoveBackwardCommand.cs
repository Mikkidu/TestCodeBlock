using PU.Promises;
using RobotProgramming.Core;
using RobotProgramming.Data;
using UnityEngine;

namespace RobotProgramming.Commands
{
    public class MoveBackwardCommand : CommandBase
    {
        public override CommandType Type => CommandType.MoveBackward;

        private float distance;

        public MoveBackwardCommand(int id, float distance = 1f)
            : base(id, new float[] { distance })
        {
            this.distance = distance;
        }

        public override IPromise Execute(IRobotController robot, ExecutionContext context)
        {
            return robot.MoveBackward(distance);
        }

        public override string GetDisplayName() => $"Назад ({distance})";

        public override Color GetBlockColor() => new Color(1f, 0.5f, 0.2f); // Orange
    }
}
