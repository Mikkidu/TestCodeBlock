using RobotProgramming.Core;
using RobotProgramming.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace RobotProgramming.UI
{
    public class BlockUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image blockImage;
        [SerializeField] private TextMeshProUGUI blockLabel;

        // Connection points - assign in Inspector
        [SerializeField] private RectTransform inputPointVisual;
        [SerializeField] private List<RectTransform> outputPointsVisuals = new List<RectTransform>();

        private ICommand command;
        private CanvasGroup canvasGroup;
        private Transform originalParent;
        private int originalSiblingIndex;
        private Canvas rootCanvas;

        // Connection points for snap system
        public List<BlockConnector> inputPoints = new List<BlockConnector>();
        public List<BlockConnector> outputPoints = new List<BlockConnector>();

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

        public void InitializeConnectors()
        {
            inputPoints.Clear();
            outputPoints.Clear();

            // Initialize input point from assigned visual element in Inspector
            if (inputPointVisual != null)
            {
                BlockConnector inputConnector = new BlockConnector(BlockConnector.PointType.Input, inputPointVisual);
                inputPoints.Add(inputConnector);
            }
            else
            {
                Debug.LogWarning($"BlockUI ({gameObject.name}): Input point visual not assigned in Inspector!");
            }

            // Initialize output points from assigned visual elements in Inspector
            foreach (RectTransform outputVisual in outputPointsVisuals)
            {
                if (outputVisual != null)
                {
                    BlockConnector outputConnector = new BlockConnector(BlockConnector.PointType.Output, outputVisual);
                    outputPoints.Add(outputConnector);
                }
            }

            if (outputPointsVisuals.Count == 0)
            {
                Debug.LogWarning($"BlockUI ({gameObject.name}): No output points assigned in Inspector!");
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
