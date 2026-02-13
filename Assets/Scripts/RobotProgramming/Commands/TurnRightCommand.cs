using PU.Promises;
using RobotProgramming.Core;
using RobotProgramming.Data;
using UnityEngine;

namespace RobotProgramming.Commands
{
    public class TurnRightCommand : CommandBase
    {
        public override CommandType Type => CommandType.TurnRight;

        public TurnRightCommand(int id)
            : base(id)
        {
        }

        public override IPromise Execute(IRobotController robot, ExecutionContext context)
        {
            return robot.TurnRight();
        }

        public override string GetDisplayName() => "Вправо ↻";

        public override Color GetBlockColor() => new Color(0.2f, 0.8f, 0.2f); // Green
    }
}
