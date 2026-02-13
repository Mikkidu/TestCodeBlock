using CodeBlocks.Core;
using CodeBlocks.Data;
using PU.Promises;
using UnityEngine;

namespace CodeBlocks.Commands
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
            if (context.IsCancelled) return Deferred.Resolved();
            
            return robot.TurnLeft();
        }

        public override string GetDisplayName() => "Left ↺";

        public override Color GetBlockColor() => new Color(1f, 0.84f, 0f); // Gold
    }
}
