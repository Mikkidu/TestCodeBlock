using UnityEngine;
using CodeBlocks.Reactions;

namespace CodeBlocks.Core
{
    public interface IObjectReaction
    {
        bool CanHandle(string objectTypeId);
        ObjectReactionResult Evaluate(ObjectReactionContext context);
    }

    public readonly struct ObjectReactionContext
    {
        public readonly GridObject Object;
        public readonly Vector2Int CurrentGrid;
        public readonly Vector2Int TargetGrid;

        public ObjectReactionContext(GridObject obj, Vector2Int currentGrid, Vector2Int targetGrid)
        {
            Object = obj;
            CurrentGrid = currentGrid;
            TargetGrid = targetGrid;
        }
    }
}
