using CodeBlocks.Reactions;

namespace CodeBlocks.Core
{
    public interface IMovementDecisionProvider
    {
        MoveDecision GetMoveDecision(MoveIntent intent);
    }
}
