using UnityEngine;

namespace CodeBlocks.Reactions
{
    public readonly struct MoveDecision
    {
        public readonly bool AllowMove;
        public readonly CellReactionType ReactionType;
        public readonly ReactionPhase Phase;
        public readonly float SpeedModifier;
        public readonly Vector2Int TargetGrid;
        public readonly string[] TargetObjectIds;
        public readonly string ObstacleTypeId;
        public readonly string SurfaceTypeId;
        public readonly string DebugReason;

        public MoveDecision(
            bool allowMove,
            CellReactionType reactionType,
            ReactionPhase phase,
            float speedModifier,
            Vector2Int targetGrid,
            string[] targetObjectIds,
            string obstacleTypeId,
            string surfaceTypeId,
            string debugReason)
        {
            AllowMove = allowMove;
            ReactionType = reactionType;
            Phase = phase;
            SpeedModifier = speedModifier;
            TargetGrid = targetGrid;
            TargetObjectIds = targetObjectIds;
            ObstacleTypeId = obstacleTypeId;
            SurfaceTypeId = surfaceTypeId;
            DebugReason = debugReason;
        }
    }
}
