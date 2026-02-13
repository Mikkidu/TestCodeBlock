using PU.Promises;
using UnityEngine;

namespace RobotProgramming.Robot
{
    public class RobotController : MonoBehaviour, Core.IRobotController
    {
        [SerializeField] private RobotConfig config;
        [SerializeField] private Transform directionIndicator;

        private Vector3 startPosition;
        private Quaternion startRotation;
        private Deferred currentMoveDeferred;

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
        public bool IsExecuting { get; private set; }

        private void Awake()
        {
            if (Timers.Instance == null)
            {
                Debug.LogError("Timers MonoBehaviour not found! Create a GameObject with Timers component.");
                return;
            }

            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        #region Movement Methods

        public IPromise MoveForward(float units)
        {
            if (IsExecuting) return Deferred.Rejected("Robot is already executing");

            IsExecuting = true;
            Vector3 targetPosition = Position + transform.forward * (units * config.moveDistance);
            float duration = (units * config.moveDistance) / config.moveSpeed;

            return AnimateMovement(targetPosition, duration);
        }

        public IPromise MoveBackward(float units)
        {
            if (IsExecuting) return Deferred.Rejected("Robot is already executing");

            IsExecuting = true;
            Vector3 targetPosition = Position - transform.forward * (units * config.moveDistance);
            float duration = (units * config.moveDistance) / config.moveSpeed;

            return AnimateMovement(targetPosition, duration);
        }

        public IPromise TurnLeft()
        {
            if (IsExecuting) return Deferred.Rejected("Robot is already executing");

            IsExecuting = true;
            Quaternion targetRotation = Rotation * Quaternion.Euler(0, -config.turnAngle, 0);
            float duration = config.turnAngle / config.turnSpeed;

            return AnimateRotation(targetRotation, duration);
        }

        public IPromise TurnRight()
        {
            if (IsExecuting) return Deferred.Rejected("Robot is already executing");

            IsExecuting = true;
            Quaternion targetRotation = Rotation * Quaternion.Euler(0, config.turnAngle, 0);
            float duration = config.turnAngle / config.turnSpeed;

            return AnimateRotation(targetRotation, duration);
        }

        #endregion

        #region Animation Methods

        private IPromise AnimateMovement(Vector3 targetPosition, float duration)
        {
            Vector3 startPos = Position;

            return Timers.Instance.Wait(duration, progress =>
            {
                float curved = config.movementCurve.Evaluate(progress);
                transform.position = Vector3.Lerp(startPos, targetPosition, curved);
            })
            .Done(() => IsExecuting = false)
            .Fail(ex =>
            {
                IsExecuting = false;
                Debug.LogError($"Movement animation failed: {ex.Message}");
            });
        }

        private IPromise AnimateRotation(Quaternion targetRotation, float duration)
        {
            Quaternion startRot = Rotation;

            return Timers.Instance.Wait(duration, progress =>
            {
                float curved = config.rotationCurve.Evaluate(progress);
                transform.rotation = Quaternion.Slerp(startRot, targetRotation, curved);
            })
            .Done(() => IsExecuting = false)
            .Fail(ex =>
            {
                IsExecuting = false;
                Debug.LogError($"Rotation animation failed: {ex.Message}");
            });
        }

        #endregion

        #region State Management

        public void Reset()
        {
            Stop();
            transform.position = startPosition;
            transform.rotation = startRotation;
            IsExecuting = false;
        }

        public void Stop()
        {
            if (currentMoveDeferred != null)
            {
                Timers.Instance.Stop(currentMoveDeferred);
                currentMoveDeferred = null;
            }
            IsExecuting = false;
        }

        #endregion
    }
}
