using CodeBlocks.Core;
using UnityEngine;

namespace CodeBlocks.Reactions
{
    public abstract class ObjectReactionComponent : MonoBehaviour, IObjectReaction
    {
        public abstract bool CanHandle(string objectTypeId);
        public abstract ObjectReactionResult Evaluate(ObjectReactionContext context);
    }
}
