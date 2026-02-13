using CodeBlocks.Core;

namespace CodeBlocks.Reactions
{
    public class DoorReaction : ObjectReactionComponent
    {
        private const string StateKey = "state";
        private const string IsOpenKey = "isOpen";

        public override bool CanHandle(string objectTypeId)
        {
            return string.Equals(objectTypeId, "Door", System.StringComparison.OrdinalIgnoreCase);
        }

        public override ObjectReactionResult Evaluate(ObjectReactionContext context)
        {
            if (context.Object == null)
                return ObjectReactionResult.NoReaction();

            bool isOpen = IsDoorOpen(context.Object);
            if (isOpen)
            {
                return ObjectReactionResult.NoReaction();
            }

            return new ObjectReactionResult(
                hasReaction: true,
                allowMove: false,
                reactionType: CellReactionType.Bounce,
                phase: ReactionPhase.Mid,
                speedModifier: 1f,
                targetObjectIds: null
            );
        }

        private static bool IsDoorOpen(GridObject obj)
        {
            if (obj.parameters == null)
                return false;

            if (obj.parameters.TryGetValue(IsOpenKey, out var isOpenValue))
            {
                return IsTrue(isOpenValue);
            }

            if (obj.parameters.TryGetValue(StateKey, out var stateValue))
            {
                if (string.Equals(stateValue, "open", System.StringComparison.OrdinalIgnoreCase))
                    return true;
                if (string.Equals(stateValue, "closed", System.StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return false;
        }

        private static bool IsTrue(string value)
        {
            return string.Equals(value, "true", System.StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "1", System.StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "yes", System.StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "on", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
