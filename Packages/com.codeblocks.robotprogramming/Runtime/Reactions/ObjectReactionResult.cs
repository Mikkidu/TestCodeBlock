namespace CodeBlocks.Reactions
{
    public readonly struct ObjectReactionResult
    {
        public readonly bool HasReaction;
        public readonly bool AllowMove;
        public readonly CellReactionType ReactionType;
        public readonly ReactionPhase Phase;
        public readonly float SpeedModifier;
        public readonly string[] TargetObjectIds;
        public readonly bool CompleteLevel;

        public ObjectReactionResult(
            bool hasReaction,
            bool allowMove,
            CellReactionType reactionType,
            ReactionPhase phase,
            float speedModifier,
            string[] targetObjectIds,
            bool completeLevel = false)
        {
            HasReaction = hasReaction;
            AllowMove = allowMove;
            ReactionType = reactionType;
            Phase = phase;
            SpeedModifier = speedModifier;
            TargetObjectIds = targetObjectIds;
            CompleteLevel = completeLevel;
        }

        public static ObjectReactionResult NoReaction(string[] targetObjectIds = null)
        {
            return new ObjectReactionResult(
                hasReaction: false,
                allowMove: true,
                reactionType: CellReactionType.Move,
                phase: ReactionPhase.Start,
                speedModifier: 1f,
                targetObjectIds: targetObjectIds,
                completeLevel: false
            );
        }

        public static ObjectReactionResult LevelComplete()
        {
            return new ObjectReactionResult(
                hasReaction: false,
                allowMove: true,
                reactionType: CellReactionType.Move,
                phase: ReactionPhase.End,
                speedModifier: 1f,
                targetObjectIds: null,
                completeLevel: true
            );
        }
    }
}
