using CodeBlocks.Core;

namespace CodeBlocks.Reactions
{
    public static class ReactionAnimationResolver
    {
        public static string ResolveTrigger(
            IRobotController robot,
            string obstacleTypeId,
            string fallbackTrigger = "",
            ReactionAnimationKey fallbackKey = ReactionAnimationKey.None)
        {
            if (robot is IReactionAnimationConfigProvider provider && provider.ReactionAnimationConfig != null)
            {
                var triggerId = provider.ReactionAnimationConfig.GetTrigger(obstacleTypeId, fallbackKey);
                if (!string.IsNullOrWhiteSpace(triggerId))
                {
                    return triggerId;
                }
            }

            return fallbackTrigger ?? string.Empty;
        }
    }
}
