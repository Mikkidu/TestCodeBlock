using CodeBlocks.Core;
using CodeBlocks.Reactions;
using PU.Promises;

namespace CodeBlocks.Commands
{
    internal static class MoveCommandExecution
    {
        internal delegate IPromise MoveHandler(float units, float speedMultiplier);

        public static IPromise Execute(
            IRobotController robot,
            ExecutionContext context,
            float distance,
            MoveIntent intent,
            MoveHandler move,
            MoveHandler reverseMove)
        {
            if (context.IsCancelled)
            {
                return Deferred.Resolved();
            }

            var provider = context.MovementDecisionProvider;
            if (provider == null)
            {
                return move(distance, 1f);
            }

            MoveDecision decision = provider.GetMoveDecision(intent);
            ReactionConfig.ReactionProfile profile = ReactionProfileResolver.Resolve(robot, decision.ObstacleTypeId);

            float speedMultiplier = CombineSpeed(decision.SpeedModifier, profile.speedModifier);
            float distanceMultiplier = profile.distanceMultiplier <= 0f ? 1f : profile.distanceMultiplier;
            float moveDistance = distance * distanceMultiplier;
            bool hasTargetTriggers = decision.TargetObjectIds != null && decision.TargetObjectIds.Length > 0;

            if (profile.outcome == MovementOutcome.StopAtImpact)
            {
                return Deferred.Resolved();
            }

            if (profile.outcome == MovementOutcome.ReturnToOrigin)
            {
                return move(moveDistance, speedMultiplier)
                    .Then(() => reverseMove(moveDistance, speedMultiplier))
                    .Then(() =>
                    {
                        TriggerTargets(context, decision.TargetObjectIds);
                        ActivateCurrentCell(context, hasTargetTriggers);
                        return Deferred.Resolved();
                    });
            }

            if (profile.outcome == MovementOutcome.StopProgramAtTarget)
            {
                var movePromise = move(moveDistance, speedMultiplier);

                if (profile.animationTriggerTiming == AnimationTriggerTiming.Start)
                {
                    TriggerReactionAnimation(robot, decision.ObstacleTypeId, profile.animationId);
                }

                return movePromise
                    .Then(() =>
                    {
                        if (profile.animationTriggerTiming != AnimationTriggerTiming.Start)
                        {
                            TriggerReactionAnimation(robot, decision.ObstacleTypeId, profile.animationId);
                        }

                        TriggerTargets(context, decision.TargetObjectIds);
                        ActivateCurrentCell(context, hasTargetTriggers);
                        context.Cancel($"Reaction:{decision.ObstacleTypeId}");
                        return Deferred.Resolved();
                    });
            }

            if (!decision.AllowMove)
            {
                return Deferred.Resolved();
            }

            return move(moveDistance, speedMultiplier)
                .Then(() =>
                {
                    TriggerTargets(context, decision.TargetObjectIds);
                    ActivateCurrentCell(context, hasTargetTriggers);
                    return Deferred.Resolved();
                });
        }

        private static float CombineSpeed(float decisionSpeed, float profileSpeed)
        {
            if (decisionSpeed <= 0f)
            {
                decisionSpeed = 1f;
            }

            if (profileSpeed <= 0f)
            {
                profileSpeed = 1f;
            }

            return decisionSpeed * profileSpeed;
        }

        private static void TriggerTargets(ExecutionContext context, string[] targetObjectIds)
        {
            if (targetObjectIds == null || targetObjectIds.Length == 0)
            {
                return;
            }

            if (context.MovementDecisionProvider is ITargetTriggerProvider triggerProvider)
            {
                triggerProvider.TriggerTargets(targetObjectIds);
            }
        }

        private static void ActivateCurrentCell(ExecutionContext context, bool hasTargetTriggers)
        {
            if (hasTargetTriggers)
            {
                return;
            }

            if (context.MovementDecisionProvider is ICellActivationProvider activationProvider)
            {
                activationProvider.ActivateCurrentCell();
            }
        }

        private static void TriggerReactionAnimation(IRobotController robot, string obstacleTypeId, string fallbackAnimationId)
        {
            var animationId = ReactionAnimationResolver.ResolveTrigger(robot, obstacleTypeId, fallbackAnimationId);
            if (robot is CodeBlocks.Robot.IRobotReactionAnimationPlayer animationPlayer)
            {
                animationPlayer.TriggerReactionAnimation(animationId);
            }
        }
    }
}
