using PU.Promises;
using RobotProgramming.Core;
using RobotProgramming.Data;
using UnityEngine;

namespace RobotProgramming.Commands
{
    public class TurnLeftCommand : CommandBase
    {
        public override CommandType Type => CommandType.TurnLeft;

        public TurnLeftCommand(int id)
            : base(id)
        {
        }

        public override IPromise Execute(IRobotController robot, ExecutionContext context)
        {
            return robot.TurnLeft();
        }

        public override string GetDisplayName() => "Влево ↺";

        public override Color GetBlockColor() => new Color(1f, 0.84f, 0f); // Gold
    }
}
