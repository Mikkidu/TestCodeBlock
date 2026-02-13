using UnityEngine;
using UnityEngine.UI;
using RobotProgramming.Core;

namespace RobotProgramming.UI
{
    /// <summary>
    /// Renders a visual line between two snap connectors using Image component.
    /// Works with UI Canvas unlike LineRenderer.
    /// </summary>
    public class SnapLineRenderer : MonoBehaviour
    {
        private Image lineImage;
        private RectTransform lineRect;

        private Vector2 fromPos;
        private Vector2 toPos;
        private bool shouldDrawLine;

        private void Awake()
        {
            // Create line image if it doesn't exist
            if (lineImage == null)
            {
                GameObject lineObj = new GameObject("SnapLine");
                lineObj.transform.SetParent(transform, false);

                lineImage = lineObj.AddComponent<Image>();
                lineImage.color = new Color(0, 1, 1, 1);  // Cyan by default

                lineRect = lineObj.GetComponent<RectTransform>();
                lineRect.sizeDelta = new Vector2(2, 2);  // Thin line
            }
        }

        public void DrawLine(Vector2 from, Vector2 to, Color color)
        {
            if (lineImage == null)
                Awake();

            fromPos = from;
            toPos = to;
            shouldDrawLine = true;

            lineImage.color = color;
            UpdateLineVisuals();
        }

        public void ClearLine()
        {
            shouldDrawLine = false;
            if (lineImage != null)
            {
                lineImage.enabled = false;
            }
        }

        private void UpdateLineVisuals()
        {
            if (!shouldDrawLine || lineImage == null || lineRect == null)
                return;

            lineImage.enabled = true;

            // Get the canvas to convert world positions to screen positions
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[SNAP LINE RENDERER] No canvas found!");
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, fromPos),
                canvas.worldCamera,
                out Vector2 screenFromPos
            );

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, toPos),
                canvas.worldCamera,
                out Vector2 screenToPos
            );

            // Calculate direction and distance
            Vector2 direction = screenToPos - screenFromPos;
            float distance = direction.magnitude;

            // Position the line at midpoint
            Vector2 midpoint = (screenFromPos + screenToPos) / 2f;
            lineRect.anchoredPosition = midpoint;

            // Rotate to point towards target
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            lineRect.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Scale to match distance
            lineRect.sizeDelta = new Vector2(distance, 3f);  // 3 pixels thick
        }

        private void Update()
        {
            if (shouldDrawLine)
            {
                UpdateLineVisuals();
            }
        }
    }
}
