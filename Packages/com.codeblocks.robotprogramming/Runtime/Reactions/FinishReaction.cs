using CodeBlocks.Core;

namespace CodeBlocks.Reactions
{
    public class FinishReaction : ObjectReactionComponent
    {
        public override bool CanHandle(string objectTypeId)
        {
            return string.Equals(objectTypeId, "FinishPoint", System.StringComparison.OrdinalIgnoreCase);
        }

        public override ObjectReactionResult Evaluate(ObjectReactionContext context)
        {
            return ObjectReactionResult.LevelComplete();
        }
    }
}
