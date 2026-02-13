using UnityEngine;
using UnityEngine.Serialization;

namespace CodeBlocks.Reactions
{
    [CreateAssetMenu(fileName = "ReactionConfig", menuName = "CodeBlocks/Reaction Config")]
    public class ReactionConfig : ScriptableObject
    {
        [System.Serializable]
        public struct ReactionProfile
        {
            public string obstacleTypeId;

            public MovementOutcome outcome;

            public string animationId;

            public AnimationTriggerTiming animationTriggerTiming;

            public float speedModifier;

            public float distanceMultiplier;
        }

        [Header("Config Mode")]
        public bool isDefault;

        [Header("Reaction Profiles")]
        public ReactionProfile[] reactionProfiles = new ReactionProfile[0];

        public ReactionProfile GetProfile(string obstacleTypeId, ReactionConfig fallback)
        {
            if (TryGetProfile(reactionProfiles, obstacleTypeId, out var localProfile))
            {
                return localProfile;
            }

            if (!isDefault && fallback != null && fallback.TryGetProfile(fallback.reactionProfiles, obstacleTypeId, out var fallbackProfile))
            {
                return fallbackProfile;
            }

            return new ReactionProfile
            {
                obstacleTypeId = obstacleTypeId,
                outcome = MovementOutcome.ReachTarget,
                animationId = "",
                animationTriggerTiming = AnimationTriggerTiming.End,
                speedModifier = 1f,
                distanceMultiplier = 1f
            };
        }

        private bool TryGetProfile(ReactionProfile[] profiles, string obstacleTypeId, out ReactionProfile profile)
        {
            profile = default;
            if (profiles == null)
                return false;

            for (int i = 0; i < profiles.Length; i++)
            {
                var current = profiles[i];
                if (!string.IsNullOrWhiteSpace(current.obstacleTypeId) &&
                    string.Equals(current.obstacleTypeId, obstacleTypeId, System.StringComparison.OrdinalIgnoreCase))
                {
                    profile = current;
                    return true;
                }
            }

            return false;
        }
    }
}
