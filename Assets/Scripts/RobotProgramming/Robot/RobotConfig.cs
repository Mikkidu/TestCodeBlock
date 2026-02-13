using UnityEngine;

namespace RobotProgramming.Robot
{
    [CreateAssetMenu(fileName = "RobotConfig", menuName = "Robot Programming/Robot Config")]
    public class RobotConfig : ScriptableObject
    {
        [Header("Movement")]
        public float moveDistance = 1f;           // Units per move command
        public float turnAngle = 90f;             // Degrees per turn
        public float moveSpeed = 2f;              // Units per second
        public float turnSpeed = 180f;            // Degrees per second

        [Header("Animation")]
        public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Visuals")]
        public float directionIndicatorLength = 0.5f;
        public Color robotColor = Color.white;
        public Color directionColor = Color.red;
    }
}
