using CodeBlocks.Reactions;
using CodeBlocks.Robot;
using System;
using UnityEngine;

namespace CodeBlocks.Managers
{
    public class MovementDecisionService
    {
        public MoveDecision DecideMove(LevelRuntimeManager levelManager, GridPositionTracker tracker, CardinalDirection direction)
        {
            if (levelManager == null || tracker == null || levelManager.CurrentLevel == null)
            {
                return new MoveDecision(
                    allowMove: false,
                    reactionType: CellReactionType.None,
                    phase: ReactionPhase.Start,
                    speedModifier: 1f,
                    targetGrid: Vector2Int.zero,
                    targetObjectIds: null,
                    obstacleTypeId: "None",
                    surfaceTypeId: "None",
                    debugReason: "Missing level manager or tracker"
                );
            }

            LevelGridData level = levelManager.CurrentLevel;
            Vector2Int currentGrid = tracker.GetCurrentGridPositionSnapshot();
            Vector2Int targetGrid = currentGrid + GetDelta(direction);

            if (!IsInsideGrid(level, targetGrid))
            {
                return new MoveDecision(
                    allowMove: false,
                    reactionType: CellReactionType.Bounce,
                    phase: ReactionPhase.Mid,
                    speedModifier: 1f,
                    targetGrid: targetGrid,
                    targetObjectIds: null,
                    obstacleTypeId: "OutOfBounds",
                    surfaceTypeId: "None",
                    debugReason: "Target outside grid"
                );
            }

            GridObject obj = level.GetObjectAt(targetGrid.x, targetGrid.y);
            ObjectReactionResult objectResult = EvaluateObject(obj, levelManager, currentGrid, targetGrid);

            TerrainCell terrain = level.GetTerrainAt(targetGrid.x, targetGrid.y);
            string terrainTypeId = terrain != null && !string.IsNullOrWhiteSpace(terrain.terrainType)
                ? terrain.terrainType
                : "NoTerrain";

            string obstacleTypeId = ResolveObstacleTypeId(obj, objectResult, terrainTypeId);

            if (objectResult.HasReaction)
            {
                return new MoveDecision(
                    allowMove: objectResult.AllowMove,
                    reactionType: objectResult.ReactionType,
                    phase: objectResult.Phase,
                    speedModifier: objectResult.SpeedModifier,
                    targetGrid: targetGrid,
                    targetObjectIds: objectResult.TargetObjectIds,
                    obstacleTypeId: obstacleTypeId,
                    surfaceTypeId: terrainTypeId,
                    debugReason: "Object reaction"
                );
            }

            var terrainReaction = ResolveTerrainReaction(terrain, out float speedModifier);

            return new MoveDecision(
                allowMove: true,
                reactionType: terrainReaction,
                phase: DefaultPhaseForTerrain(terrainReaction),
                speedModifier: speedModifier,
                targetGrid: targetGrid,
                targetObjectIds: objectResult.TargetObjectIds,
                obstacleTypeId: obstacleTypeId,
                surfaceTypeId: terrainTypeId,
                debugReason: "Terrain reaction"
            );
        }

        private static ObjectReactionResult EvaluateObject(GridObject obj, LevelRuntimeManager levelManager, Vector2Int currentGrid, Vector2Int targetGrid)
        {
            if (obj == null || levelManager == null)
                return ObjectReactionResult.NoReaction();

            if (!levelManager.TryGetObjectInstance(targetGrid, out GameObject instance) || instance == null)
                return ObjectReactionResult.NoReaction();

            var context = new CodeBlocks.Core.ObjectReactionContext(obj, currentGrid, targetGrid);
            var components = instance.GetComponents<ObjectReactionComponent>();
            if (components == null || components.Length == 0)
                return ObjectReactionResult.NoReaction();

            foreach (var component in components)
            {
                if (component == null)
                    continue;

                if (component.CanHandle(obj.objectTypeId))
                {
                    var result = component.Evaluate(context);
                    if (result.HasReaction ||
                        result.CompleteLevel ||
                        (result.TargetObjectIds != null && result.TargetObjectIds.Length > 0))
                    {
                        return result;
                    }
                }
            }

            return ObjectReactionResult.NoReaction();
        }

        private static bool IsInsideGrid(LevelGridData level, Vector2Int grid)
        {
            return grid.x >= 0 && grid.x < level.gridWidth && grid.y >= 0 && grid.y < level.gridHeight;
        }

        private static Vector2Int GetDelta(CardinalDirection direction)
        {
            return direction switch
            {
                CardinalDirection.North => new Vector2Int(0, 1),
                CardinalDirection.East => new Vector2Int(1, 0),
                CardinalDirection.South => new Vector2Int(0, -1),
                CardinalDirection.West => new Vector2Int(-1, 0),
                _ => Vector2Int.zero
            };
        }

        private static CellReactionType ResolveTerrainReaction(TerrainCell terrain, out float speedModifier)
        {
            speedModifier = 1f;

            if (terrain == null || string.IsNullOrEmpty(terrain.terrainType))
                return CellReactionType.Move;

            string type = terrain.terrainType.Trim();

            if (type.Equals("Pit", StringComparison.OrdinalIgnoreCase))
                return CellReactionType.Fall;

            if (type.Equals("Spike", StringComparison.OrdinalIgnoreCase))
                return CellReactionType.Break;

            if (type.Equals("Water", StringComparison.OrdinalIgnoreCase))
            {
                speedModifier = 0.5f;
                return CellReactionType.Swim;
            }

            if (type.Equals("Ice", StringComparison.OrdinalIgnoreCase))
            {
                speedModifier = 1.5f;
                return CellReactionType.Slide;
            }

            return CellReactionType.Move;
        }

        private static ReactionPhase DefaultPhaseForTerrain(CellReactionType type)
        {
            return type switch
            {
                CellReactionType.Fall => ReactionPhase.End,
                CellReactionType.Break => ReactionPhase.End,
                CellReactionType.Bounce => ReactionPhase.Mid,
                CellReactionType.Swim => ReactionPhase.Start,
                CellReactionType.Slide => ReactionPhase.Start,
                _ => ReactionPhase.Start
            };
        }

        private static string ResolveObstacleTypeId(GridObject obj, ObjectReactionResult objectResult, string terrainTypeId)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.objectTypeId))
                return terrainTypeId;

            // Object explicitly reacts (e.g. closed door/wall) -> obstacle is object itself.
            if (objectResult.HasReaction)
                return obj.objectTypeId;

            // Objects that trigger targets (e.g. button) should keep object profile semantics.
            if (objectResult.TargetObjectIds != null && objectResult.TargetObjectIds.Length > 0)
                return obj.objectTypeId;

            // Finish remains object-driven even without blocking reaction.
            if (string.Equals(obj.objectTypeId, "FinishPoint", StringComparison.OrdinalIgnoreCase))
                return obj.objectTypeId;

            // Pass-through objects without explicit behavior (e.g. open door) use terrain behavior.
            return terrainTypeId;
        }
    }
}
