using PU.Promises;
using UnityEngine;

namespace CodeBlocks.Robot
{
    public class RobotController : MonoBehaviour, Core.IRobotController, Reactions.IReactionConfigProvider, Reactions.IReactionAnimationConfigProvider, IRobotReactionAnimationPlayer
    {
        [SerializeField] private RobotConfig config;
        [SerializeField] private Transform directionIndicator;

        private readonly RobotAnimationDriver animationDriver = new RobotAnimationDriver();
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Quaternion logicalRotation;
        private IPromise activeAnimationPromise;

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
        public bool IsExecuting { get; private set; }
        public Reactions.ReactionConfig ReactionConfig => config != null ? config.reactionConfig : null;
        public Reactions.ReactionAnimationConfig ReactionAnimationConfig => config != null ? config.reactionAnimationConfig : null;

        private void Awake()
        {
            if (Timers.Instance == null)
            {
                Debug.LogError("Timers MonoBehaviour not found! Create a GameObject with Timers component.");
                return;
            }

            if (startPosition == Vector3.zero)
            {
                startPosition = transform.position;
            }
            if (startRotation == Quaternion.identity)
            {
                startRotation = transform.rotation;
            }
            logicalRotation = startRotation;

            var localScale = transform.localScale;
            var lossyScale = transform.lossyScale;
            transform.localScale = new Vector3(localScale.x / lossyScale.x, localScale.y / lossyScale.y, localScale.z / lossyScale.z);

            animationDriver.Initialize(GetComponent<Animator>(), config);
            animationDriver.SetMoveAnimation(false);
        }

        private void LateUpdate()
        {
            animationDriver.ApplyRotation(transform, logicalRotation);
        }
        
        public void SetStartPosition(Vector3 position, Quaternion rotation)
        {
            startPosition = position;
            startRotation = rotation;
            logicalRotation = rotation;

            Debug.Log($"RobotController: Start position updated to {position}, rotation {rotation.eulerAngles}");
        }

        #region Movement Methods

        public IPromise MoveForward(float units, float speedMultiplier = 1f)
        {
            if (IsExecuting) return Deferred.Rejected("Robot is already executing");

            IsExecuting = true;
            Vector3 targetPosition = Position + transform.forward * (units * config.moveDistance);
            float duration = (units * config.moveDistance) / GetAdjustedMoveSpeed(speedMultiplier);

            return AnimateMovement(targetPosition, duration);
        }

        public IPromise MoveBackward(float units, float speedMultiplier = 1f)
        {
            if (IsExecuting) return Deferred.Rejected("Robot is already executing");

            IsExecuting = true;
            Vector3 targetPosition = Position - transform.forward * (units * config.moveDistance);
            float duration = (units * config.moveDistance) / GetAdjustedMoveSpeed(speedMultiplier);

            return AnimateMovement(targetPosition, duration);
        }

        public IPromise TurnLeft()
        {
            if (IsExecuting) return Deferred.Rejected("Robot is already executing");

            IsExecuting = true;
            Quaternion targetRotation = logicalRotation * Quaternion.Euler(0, -config.turnAngle, 0);
            float duration = config.turnAngle / config.turnSpeed;

            if (animationDriver.HasTurnLeftTrigger)
            {
                return AnimateRotationByAnimator(targetRotation, duration, true);
            }

            return AnimateRotationByCode(targetRotation, duration);
        }

        public IPromise TurnRight()
        {
            if (IsExecuting) return Deferred.Rejected("Robot is already executing");

            IsExecuting = true;
            Quaternion targetRotation = logicalRotation * Quaternion.Euler(0, config.turnAngle, 0);
            float duration = config.turnAngle / config.turnSpeed;

            if (animationDriver.HasTurnRightTrigger)
            {
                return AnimateRotationByAnimator(targetRotation, duration, false);
            }

            return AnimateRotationByCode(targetRotation, duration);
        }

        #endregion

        #region Animation Methods

        private IPromise AnimateMovement(Vector3 targetPosition, float duration)
        {
            Vector3 startPos = Position;
            animationDriver.SetMoveAnimation(true);

            var timedPromise = StartTimedAnimation(duration, progress =>
            {
                float curved = config.movementCurve.Evaluate(progress);
                transform.position = Vector3.Lerp(startPos, targetPosition, curved);
            });

            return timedPromise
            .Done(() =>
            {
                IsExecuting = false;
                animationDriver.SetMoveAnimation(false);
                ClearActiveAnimationPromise(timedPromise);
            })
            .Fail(ex =>
            {
                IsExecuting = false;
                animationDriver.SetMoveAnimation(false);
                ClearActiveAnimationPromise(timedPromise);
                Debug.LogError($"Movement animation failed: {ex.Message}");
            });
        }

        private float GetAdjustedMoveSpeed(float speedMultiplier)
        {
            if (speedMultiplier < 0.01f)
                speedMultiplier = 0.01f;
            return config.moveSpeed * speedMultiplier;
        }

        private IPromise AnimateRotationByCode(Quaternion targetRotation, float duration)
        {
            Quaternion startRot = logicalRotation;

            var timedPromise = StartTimedAnimation(duration, progress =>
            {
                float curved = config.rotationCurve.Evaluate(progress);
                transform.rotation = Quaternion.Slerp(startRot, targetRotation, curved);
            });

            return timedPromise
            .Done(() =>
            {
                logicalRotation = targetRotation;
                transform.rotation = logicalRotation;
                IsExecuting = false;
                ClearActiveAnimationPromise(timedPromise);
            })
            .Fail(ex =>
            {
                IsExecuting = false;
                ClearActiveAnimationPromise(timedPromise);
                Debug.LogError($"Rotation animation failed: {ex.Message}");
            });
        }

        private IPromise AnimateRotationByAnimator(Quaternion targetRotation, float duration, bool left)
        {
            animationDriver.StartTurnAnimation(left, logicalRotation, targetRotation, duration);

            var timedPromise = StartTimedAnimation(duration);

            return timedPromise
            .Done(() =>
            {
                // Lock exact final orientation for grid logic consistency.
                logicalRotation = targetRotation;
                transform.rotation = logicalRotation;
                animationDriver.StopTurnOverride();
                IsExecuting = false;
                ClearActiveAnimationPromise(timedPromise);
            })
            .Fail(ex =>
            {
                animationDriver.StopTurnOverride();
                IsExecuting = false;
                ClearActiveAnimationPromise(timedPromise);
                Debug.LogError($"Rotation animation failed: {ex.Message}");
            });
        }

        #endregion

        #region State Management

        public void Reset()
        {
            Stop();
            animationDriver.ResetAnimatorState();
            transform.position = startPosition;
            logicalRotation = startRotation;
            transform.rotation = logicalRotation;
            IsExecuting = false;
            animationDriver.SetMoveAnimation(false);
        }

        public void Stop()
        {
            if (activeAnimationPromise != null)
            {
                Timers.Instance.Stop(activeAnimationPromise);
                activeAnimationPromise = null;
            }
            animationDriver.StopTurnOverride();
            transform.rotation = logicalRotation;
            IsExecuting = false;
            animationDriver.SetMoveAnimation(false);
        }

        public void TriggerReactionAnimation(string animationId)
        {
            animationDriver.TriggerReactionAnimation(animationId);
        }

        private IPromise StartTimedAnimation(float duration, System.Action<float> progressCallback = null)
        {
            var timedPromise = Timers.Instance.Wait(duration, progressCallback);
            activeAnimationPromise = timedPromise;
            return timedPromise;
        }

        private void ClearActiveAnimationPromise(IPromise completedPromise)
        {
            if (ReferenceEquals(activeAnimationPromise, completedPromise))
            {
                activeAnimationPromise = null;
            }
        }

        #endregion
    }
}
