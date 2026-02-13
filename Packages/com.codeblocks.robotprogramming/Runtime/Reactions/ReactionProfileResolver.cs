using CodeBlocks.Core;
using UnityEngine;

namespace CodeBlocks.Reactions
{
    public static class ReactionProfileResolver
    {
        private const string DefaultConfigPath = "Configs/DefaultReactionConfig";
        private const string FinishPointTypeId = "FinishPoint";
        private const string LegacyFinishTypeId = "Finish";
        private const string NoTerrainTypeId = "NoTerrain";

        public static ReactionConfig.ReactionProfile Resolve(IRobotController robot, string obstacleTypeId)
        {
            ReactionConfig config = null;
            if (robot is IReactionConfigProvider provider)
            {
                config = provider.ReactionConfig;
            }

            return Resolve(config, obstacleTypeId);
        }

        public static ReactionConfig.ReactionProfile Resolve(ReactionConfig config, string obstacleTypeId)
        {
            var defaultConfig = LoadDefaultConfig();

            ReactionConfig.ReactionProfile profile;
            if (config != null)
            {
                profile = config.GetProfile(obstacleTypeId, defaultConfig);
            }
            else if (defaultConfig != null)
            {
                profile = defaultConfig.GetProfile(obstacleTypeId, null);
            }
            else
            {
                profile = CreateDefaultProfile(obstacleTypeId);
            }

            profile = ApplyAliases(obstacleTypeId, profile, config, defaultConfig);
            return ApplyFallbackOutcomePolicy(obstacleTypeId, profile);
        }

        private static ReactionConfig LoadDefaultConfig()
        {
            return Resources.Load<ReactionConfig>(DefaultConfigPath);
        }

        private static ReactionConfig.ReactionProfile ApplyAliases(
            string obstacleTypeId,
            ReactionConfig.ReactionProfile profile,
            ReactionConfig config,
            ReactionConfig fallback)
        {
            if (!string.Equals(obstacleTypeId, FinishPointTypeId, System.StringComparison.OrdinalIgnoreCase))
            {
                return profile;
            }

            var legacyProfile = GetLegacyFinishProfile(config, fallback);

            if (string.IsNullOrWhiteSpace(profile.animationId) && !string.IsNullOrWhiteSpace(legacyProfile.animationId))
            {
                profile.animationId = legacyProfile.animationId;
            }

            if (profile.outcome == MovementOutcome.ReachTarget && legacyProfile.outcome != MovementOutcome.ReachTarget)
            {
                profile.outcome = legacyProfile.outcome;
            }

            if (profile.speedModifier <= 0f && legacyProfile.speedModifier > 0f)
            {
                profile.speedModifier = legacyProfile.speedModifier;
            }

            if (profile.distanceMultiplier <= 0f && legacyProfile.distanceMultiplier > 0f)
            {
                profile.distanceMultiplier = legacyProfile.distanceMultiplier;
            }

            return profile;
        }

        private static ReactionConfig.ReactionProfile GetLegacyFinishProfile(ReactionConfig config, ReactionConfig fallback)
        {
            if (config != null)
            {
                return config.GetProfile(LegacyFinishTypeId, fallback);
            }

            if (fallback != null)
            {
                return fallback.GetProfile(LegacyFinishTypeId, null);
            }

            return CreateDefaultProfile(LegacyFinishTypeId);
        }

        private static ReactionConfig.ReactionProfile ApplyFallbackOutcomePolicy(
            string obstacleTypeId,
            ReactionConfig.ReactionProfile profile)
        {
            if (string.Equals(obstacleTypeId, NoTerrainTypeId, System.StringComparison.OrdinalIgnoreCase) &&
                profile.outcome == MovementOutcome.ReachTarget)
            {
                profile.outcome = MovementOutcome.StopProgramAtTarget;
            }

            return profile;
        }

        private static ReactionConfig.ReactionProfile CreateDefaultProfile(string obstacleTypeId)
        {
            return new ReactionConfig.ReactionProfile
            {
                obstacleTypeId = obstacleTypeId,
                outcome = MovementOutcome.ReachTarget,
                animationId = "",
                animationTriggerTiming = AnimationTriggerTiming.End,
                speedModifier = 1f,
                distanceMultiplier = 1f
            };
        }
    }
}
