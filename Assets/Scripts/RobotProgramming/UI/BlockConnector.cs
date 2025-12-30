using UnityEngine;

namespace RobotProgramming.UI
{
    public class BlockConnector
    {
        public enum PointType { Input, Output }
        public enum ParameterType { None, Number, String, Boolean, Vector }

        // Main properties
        public PointType pointType;
        public RectTransform visualElement;  // Small UI circle/element for connection point

        // Connection navigation - Stage 6
        public BlockUI parentBlock;           // Reference to the block that owns this connector
        public BlockConnector connectedTo;    // Physical connection to another connector

        // Future use - parameter passing
        public ParameterType parameterType = ParameterType.None;
        public string parameterName = "";

        public BlockConnector(PointType type, RectTransform visual)
        {
            pointType = type;
            visualElement = visual;
        }

        // Get world/screen position of this connection point
        public Vector2 GetWorldPosition()
        {
            if (visualElement == null)
                return Vector2.zero;

            // Get the screen position of the visual element's center
            Vector3[] corners = new Vector3[4];
            visualElement.GetWorldCorners(corners);

            // Return the center position in screen space
            return (Vector2)((corners[0] + corners[2]) / 2f);
        }

        // Check if this connector can connect to another (for future parameter validation)
        public bool CanConnectTo(BlockConnector other)
        {
            if (other == null)
                return false;

            // Check parameter type compatibility
            if (parameterType == ParameterType.None || other.parameterType == ParameterType.None)
                return true;

            return parameterType == other.parameterType;
        }
    }
}
