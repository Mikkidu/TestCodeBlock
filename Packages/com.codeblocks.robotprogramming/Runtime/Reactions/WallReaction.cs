using CodeBlocks.Core;

namespace CodeBlocks.Reactions
{
    public class WallReaction : ObjectReactionComponent
    {
        public override bool CanHandle(string objectTypeId)
        {
            return string.Equals(objectTypeId, "Wall", System.StringComparison.OrdinalIgnoreCase);
        }

        public override ObjectReactionResult Evaluate(ObjectReactionContext context)
        {
            return new ObjectReactionResult(
                hasReaction: true,
                allowMove: false,
                reactionType: CellReactionType.Bounce,
                phase: ReactionPhase.Mid,
                speedModifier: 1f,
                targetObjectIds: null
            );
        }
    }
}
