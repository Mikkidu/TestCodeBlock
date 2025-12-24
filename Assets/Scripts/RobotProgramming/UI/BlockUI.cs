using RobotProgramming.Core;
using RobotProgramming.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace RobotProgramming.UI
{
    public class BlockUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image blockImage;
        [SerializeField] private TextMeshProUGUI blockLabel;

        private ICommand command;
        private CanvasGroup canvasGroup;
        private Transform originalParent;
        private int originalSiblingIndex;
        private Canvas rootCanvas;

        public ICommand Command => command;

        private void Awake()
        {
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            rootCanvas = GetComponentInParent<Canvas>();
            
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
            }
        }

        public void SetCommand(ICommand cmd)
        {
            command = cmd;

            if (blockLabel != null)
            {
                blockLabel.text = cmd.GetDisplayName();
            }

            if (blockImage != null)
            {
                blockImage.color = cmd.GetBlockColor();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0.6f;
            }

            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();

            // Move to root canvas for dragging
            if (rootCanvas != null)
            {
                transform.SetParent(rootCanvas.transform, true);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
            }

            // Will be repositioned by drop handler or return to original
            //if (eventData.pointerCurrentRaycast.gameObject == null ||
            //    !eventData.pointerCurrentRaycast.gameObject.CompareTag("DropZone"))
            //{
            //    ReturnToOriginalPosition();
            //
            //}
            ReturnToOriginalPosition();
        }

        public void ReturnToOriginalPosition()
        {
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }
}
