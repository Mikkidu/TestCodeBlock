using CodeBlocks.Core;
using UnityEngine;

namespace CodeBlocks.Robot
{
    internal sealed class RobotAnimationDriver
    {
        private Animator cachedAnimator;
        private AnimationCurve rotationCurve;

        private int moveBoolHash;
        private int turnLeftTriggerHash;
        private int turnRightTriggerHash;

        private bool hasMoveBoolParameter;
        private bool hasTurnLeftTrigger;
        private bool hasTurnRightTrigger;

        private bool isTurnOverrideActive;
        private float turnOverrideStartTime;
        private float turnOverrideDuration;
        private Quaternion turnOverrideStartRotation;
        private Quaternion turnOverrideTargetRotation;

        public bool HasTurnLeftTrigger => hasTurnLeftTrigger;
        public bool HasTurnRightTrigger => hasTurnRightTrigger;

        public void Initialize(Animator animator, RobotConfig config)
        {
            cachedAnimator = animator;
            rotationCurve = config != null ? config.rotationCurve : AnimationCurve.Linear(0f, 0f, 1f, 1f);

            if (cachedAnimator == null)
            {
                hasMoveBoolParameter = false;
                hasTurnLeftTrigger = false;
                hasTurnRightTrigger = false;
                return;
            }

            var moveParameter = config != null && !string.IsNullOrEmpty(config.moveBoolParameter)
                ? config.moveBoolParameter
                : "IsMoving";
            var turnLeftParameter = config != null && !string.IsNullOrEmpty(config.turnLeftTriggerParameter)
                ? config.turnLeftTriggerParameter
                : "TurnLeft";
            var turnRightParameter = config != null && !string.IsNullOrEmpty(config.turnRightTriggerParameter)
                ? config.turnRightTriggerParameter
                : "TurnRight";

            moveBoolHash = Animator.StringToHash(moveParameter);
            turnLeftTriggerHash = Animator.StringToHash(turnLeftParameter);
            turnRightTriggerHash = Animator.StringToHash(turnRightParameter);

            hasMoveBoolParameter = AnimatorParameterUtility.HasParameter(cachedAnimator, moveBoolHash, AnimatorControllerParameterType.Bool);
            hasTurnLeftTrigger = AnimatorParameterUtility.HasParameter(cachedAnimator, turnLeftTriggerHash, AnimatorControllerParameterType.Trigger);
            hasTurnRightTrigger = AnimatorParameterUtility.HasParameter(cachedAnimator, turnRightTriggerHash, AnimatorControllerParameterType.Trigger);
        }

        public void ApplyRotation(Transform transform, Quaternion logicalRotation)
        {
            if (!isTurnOverrideActive)
            {
                transform.rotation = logicalRotation;
                return;
            }

            float elapsed = Time.time - turnOverrideStartTime;
            float normalized = turnOverrideDuration > 0.0001f
                ? Mathf.Clamp01(elapsed / turnOverrideDuration)
                : 1f;
            float curved = rotationCurve != null ? rotationCurve.Evaluate(normalized) : normalized;
            transform.rotation = Quaternion.Slerp(turnOverrideStartRotation, turnOverrideTargetRotation, curved);
        }

        public void StartTurnAnimation(bool left, Quaternion startRotation, Quaternion targetRotation, float duration)
        {
            if (cachedAnimator == null)
            {
                return;
            }

            cachedAnimator.ResetTrigger(turnLeftTriggerHash);
            cachedAnimator.ResetTrigger(turnRightTriggerHash);

            if (left && hasTurnLeftTrigger)
            {
                cachedAnimator.SetTrigger(turnLeftTriggerHash);
            }
            else if (!left && hasTurnRightTrigger)
            {
                cachedAnimator.SetTrigger(turnRightTriggerHash);
            }

            turnOverrideStartRotation = startRotation;
            turnOverrideTargetRotation = targetRotation;
            turnOverrideDuration = Mathf.Max(0.0001f, duration);
            turnOverrideStartTime = Time.time;
            isTurnOverrideActive = true;
        }

        public void StopTurnOverride()
        {
            isTurnOverrideActive = false;
        }

        public void SetMoveAnimation(bool isMoving)
        {
            if (cachedAnimator == null || !hasMoveBoolParameter)
            {
                return;
            }

            AnimatorParameterUtility.TrySetBool(cachedAnimator, moveBoolHash, isMoving);
        }

        public void TriggerReactionAnimation(string animationId)
        {
            if (cachedAnimator == null || string.IsNullOrWhiteSpace(animationId))
            {
                return;
            }

            int triggerHash = Animator.StringToHash(animationId);
            if (!AnimatorParameterUtility.TrySetTrigger(cachedAnimator, triggerHash))
            {
                Debug.LogWarning($"RobotController: Animator trigger not found for reaction animation '{animationId}'.");
            }
        }

        public void ResetAnimatorState()
        {
            if (cachedAnimator == null)
            {
                return;
            }

            cachedAnimator.Rebind();
            cachedAnimator.Update(0f);
        }
    }
}
