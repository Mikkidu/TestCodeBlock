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
        public bool inProgramArea = false;
        private ProgramArea programArea;

        // Track highlighted output for cleanup
        private BlockConnector previousHighlightedOutput;


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
            
            programArea ??= GetComponentInParent<ProgramArea>();
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

        private void UpdateSnapVisuals(SnapManager.SnapInfo snapInfo)
        {
            if (snapInfo.canSnap && snapInfo.targetOutput != null)
            {
                // Reset previously highlighted output if different
                if (previousHighlightedOutput != null && previousHighlightedOutput != snapInfo.targetOutput)
                {
                    Image prevOutputImage = previousHighlightedOutput.visualElement.GetComponent<Image>();
                    if (prevOutputImage != null)
                    {
                        prevOutputImage.color = new Color(1f, 0f, 0f, 1f); // Red
                    }
                }

                // Highlight input point with yellow
                if (inputPoints.Count > 0 && inputPoints[0]?.visualElement?.GetComponent<Image>() != null)
                {
                    inputPoints[0].visualElement.GetComponent<Image>().color = new Color(1f, 1f, 0f, 1f); // Yellow
                }

                // Highlight target output point with yellow
                Image targetOutputImage = snapInfo.targetOutput.visualElement.GetComponent<Image>();
                if (targetOutputImage != null)
                {
                    targetOutputImage.color = new Color(1f, 1f, 0f, 1f); // Yellow
                }

                // Remember this output for cleanup
                previousHighlightedOutput = snapInfo.targetOutput;
            }
            else
            {
                // Reset previously highlighted output when snap is no longer possible
                if (previousHighlightedOutput != null)
                {
                    Image prevOutputImage = previousHighlightedOutput.visualElement.GetComponent<Image>();
                    if (prevOutputImage != null)
                    {
                        prevOutputImage.color = new Color(1f, 0f, 0f, 1f); // Red
                    }
                    previousHighlightedOutput = null;
                }

                ResetConnectorColors();
            }
        }

        private void ResetConnectorColors()
        {
            // Reset input point color to green
            if (inputPoints.Count > 0 && inputPoints[0]?.visualElement?.GetComponent<Image>() != null)
            {
                inputPoints[0].visualElement.GetComponent<Image>().color = new Color(0f, 1f, 0f, 1f); // Green
            }

            // Reset output point colors to red
            foreach (var output in outputPoints)
            {
                Image outputImage = output?.visualElement?.GetComponent<Image>();
                if (outputImage != null)
                {
                    outputImage.color = new Color(1f, 0f, 0f, 1f); // Red
                }
            }
        }

        public void InitializeConnectors()
        {
            inputPoints.Clear();
            outputPoints.Clear();

            // Initialize input point from assigned visual element in Inspector
            if (inputPointVisual != null)
            {
                // Set input point color to green
                Image inputImage = inputPointVisual.GetComponent<Image>();
                if (inputImage != null)
                {
                    inputImage.color = new Color(0f, 1f, 0f, 1f); // Green
                }

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
                    // Set output point color to red
                    Image outputImage = outputVisual.GetComponent<Image>();
                    if (outputImage != null)
                    {
                        outputImage.color = new Color(1f, 0f, 0f, 1f); // Red
                    }

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
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
            }

            // Find nearest output for snap feedback (only in program area)
            if (programArea != null && programArea.GetBlocks().Count > 0)
            {
                SnapManager snapManager = programArea.GetSnapManager();
                if (snapManager != null && inputPoints.Count > 0)
                {
                    SnapManager.SnapInfo snapInfo = snapManager.FindNearestOutput(this, programArea.GetBlocks());

                    // Update visual feedback (yellow highlight when ready)
                    UpdateSnapVisuals(snapInfo);

                    if (snapInfo.canSnap && snapInfo.targetOutput != null)
                    {
                        Debug.Log($"[SNAP READY] {gameObject.name} → {snapInfo.targetBlock.gameObject.name} | Distance: {snapInfo.distance:F2}px");
                    }
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
            }

            // Reset previously highlighted output from other blocks
            if (previousHighlightedOutput != null)
            {
                Image prevOutputImage = previousHighlightedOutput.visualElement.GetComponent<Image>();
                if (prevOutputImage != null)
                {
                    prevOutputImage.color = new Color(1f, 0f, 0f, 1f); // Red
                }
                previousHighlightedOutput = null;
            }

            // Reset connector colors to normal
            ResetConnectorColors();

            // Check if we can apply snap
            if (programArea != null && programArea.GetBlocks().Count > 0)
            {
                SnapManager snapManager = programArea.GetSnapManager();
                if (snapManager != null && inputPoints.Count > 0)
                {
                    SnapManager.SnapInfo snapInfo = snapManager.FindNearestOutput(this, programArea.GetBlocks());

                    if (snapInfo.canSnap && snapInfo.targetOutput != null)
                    {
                        // Apply snap to align input with target output
                        snapManager.ApplySnap(this, inputPoints[0], snapInfo.targetOutput);
                        Debug.Log($"[SNAP APPLIED] {gameObject.name} → {snapInfo.targetBlock.gameObject.name}");
                    }
                    else
                    {
                        // No snap, return to original position
                        ReturnToOriginalPosition();
                    }
                }
                else
                {
                    ReturnToOriginalPosition();
                }
            }
            else
            {
                // No program area or blocks, return to original
                ReturnToOriginalPosition();
            }
        }

        public void ReturnToOriginalPosition()
        {
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }
}
