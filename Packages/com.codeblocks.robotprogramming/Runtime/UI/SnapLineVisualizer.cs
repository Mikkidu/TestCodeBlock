using System.Linq;
using UnityEngine;
using CodeBlocks.Core;

namespace CodeBlocks.UI
{
    /// <summary>
    /// Visualizes the magnetic snap line between dragging block's connector and target connector.
    /// Keeps the line visible in Game View as long as snap is active.
    /// Uses Gizmos for persistent visualization.
    /// </summary>
    public class SnapLineVisualizer : MonoBehaviour
    {
        private BlockUIBase draggingBlock;
        private SnapManager.SnapInfo lastSnapInfo;
        private bool hasActiveSnap;

        // Store positions for Gizmo drawing
        private Vector3 snapFromPos = Vector3.zero;
        private Vector3 snapToPos = Vector3.zero;
        private Color snapLineColor = Color.yellow;

        public void SetSnapInfo(BlockUIBase block, SnapManager.SnapInfo snapInfo)
        {
            draggingBlock = block;
            lastSnapInfo = snapInfo;
            hasActiveSnap = snapInfo.canSnap && snapInfo.targetConnector != null;

            if (hasActiveSnap)
            {
                UpdateSnapPositions();
                Debug.Log($"[VISUALIZER] SNAP ACTIVE: {block.gameObject.name} → {snapInfo.targetBlock.gameObject.name}");
            }
        }

        public void ClearSnapInfo()
        {
            if (hasActiveSnap)
            {
                Debug.Log($"[VISUALIZER] SNAP CLEARED");
            }
            draggingBlock = null;
            hasActiveSnap = false;
            lastSnapInfo = new SnapManager.SnapInfo { canSnap = false };
        }

        private void UpdateSnapPositions()
        {
            if (!hasActiveSnap || draggingBlock == null || lastSnapInfo.targetConnector == null)
                return;

            // Determine which connectors to draw line between based on snap type
            if (lastSnapInfo.snapType == SnapManager.SnapInfo.SnapType.OutputToInput)
            {
                // OUTPUT of dragging block → INPUT of target
                var dragOut = draggingBlock.GetOutputConnectors();
                if (dragOut.Any() && dragOut.First() != null)
                {
                    snapFromPos = dragOut.First().GetWorldPosition();
                    snapToPos = lastSnapInfo.targetConnector.GetWorldPosition();
                    snapLineColor = new Color(1f, 1f, 0f, 1f);  // Yellow for OUTPUT→INPUT
                    Debug.Log($"[VISUALIZER] OUTPUT→INPUT: from {snapFromPos} to {snapToPos}");
                }
            }
            else if (lastSnapInfo.snapType == SnapManager.SnapInfo.SnapType.InputToOutput)
            {
                // INPUT of dragging block → OUTPUT of target
                var dragIn = draggingBlock.GetInputConnectors();
                if (dragIn.Any() && dragIn.First() != null)
                {
                    snapFromPos = dragIn.First().GetWorldPosition();
                    snapToPos = lastSnapInfo.targetConnector.GetWorldPosition();
                    snapLineColor = new Color(0f, 1f, 1f, 1f);  // Cyan for INPUT→OUTPUT
                    Debug.Log($"[VISUALIZER] INPUT→OUTPUT: from {snapFromPos} to {snapToPos}");
                }
            }
        }

        private void Update()
        {
            // Update positions every frame if snap is active
            if (hasActiveSnap)
            {
                UpdateSnapPositions();

                // Draw spheres at connector positions
                DrawDebugSpheres();
            }
        }

        private void DrawDebugSpheres()
        {
            // Draw from position (dragging block's connector)
            Debug.DrawLine(snapFromPos - Vector3.right * 5f, snapFromPos + Vector3.right * 5f, snapLineColor, 0f);
            Debug.DrawLine(snapFromPos - Vector3.up * 5f, snapFromPos + Vector3.up * 5f, snapLineColor, 0f);

            // Draw to position (target connector)
            Debug.DrawLine(snapToPos - Vector3.right * 5f, snapToPos + Vector3.right * 5f, snapLineColor, 0f);
            Debug.DrawLine(snapToPos - Vector3.up * 5f, snapToPos + Vector3.up * 5f, snapLineColor, 0f);

            // Draw connecting line
            Debug.DrawLine(snapFromPos, snapToPos, snapLineColor, 0f);
        }

        // Gizmo drawing for editor visualization
        private void OnDrawGizmos()
        {
            if (!hasActiveSnap)
                return;

            Gizmos.color = snapLineColor;

            // Draw circles at connection points
            DrawCircleGizmo(snapFromPos, 10f);
            DrawCircleGizmo(snapToPos, 10f);

            // Draw line
            Gizmos.DrawLine(snapFromPos, snapToPos);
        }

        private void DrawCircleGizmo(Vector3 center, float radius)
        {
            int segments = 16;
            Vector3 lastPoint = center + Vector3.right * radius;

            for (int i = 1; i <= segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                Gizmos.DrawLine(lastPoint, newPoint);
                lastPoint = newPoint;
            }
        }
    }
}
