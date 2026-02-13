using UnityEngine;

namespace CodeBlocks.Reactions
{
    [CreateAssetMenu(fileName = "ReactionAnimationConfig", menuName = "CodeBlocks/Reaction Animation Config")]
    public class ReactionAnimationConfig : ScriptableObject
    {
        [System.Serializable]
        public struct ReactionAnimationEntry
        {
            public ReactionAnimationKey key;
            public string obstacleTypeId;
            public string triggerId;
        }

        [SerializeField] private ReactionAnimationEntry[] entries = new ReactionAnimationEntry[0];

        public string GetTrigger(string obstacleTypeId, ReactionAnimationKey key)
        {
            if (entries == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(obstacleTypeId))
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    var entry = entries[i];
                    if (!string.IsNullOrWhiteSpace(entry.obstacleTypeId) &&
                        string.Equals(entry.obstacleTypeId, obstacleTypeId, System.StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(entry.triggerId))
                    {
                        return entry.triggerId;
                    }
                }
            }

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry.key == key && !string.IsNullOrWhiteSpace(entry.triggerId))
                {
                    return entry.triggerId;
                }
            }

            return string.Empty;
        }
    }
}
