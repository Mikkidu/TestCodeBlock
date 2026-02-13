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
    public class BlockUI : BlockUIBase
    {
        public event Action OnAlignmentComplete;

        // Connection points - assign in Inspector
        [SerializeField] private RectTransform inputPointVisual;
        [SerializeField] private List<RectTransform> outputPointsVisuals = new List<RectTransform>();

        // Connection points for snap system
        // public List<BlockConnector> inputPoints = new List<BlockConnector>();
        // public List<BlockConnector> outputPoints = new List<BlockConnector>();
        

        public override void InitializeConnectors()
        {

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
                inputConnector.parentBlock = this;  // Set owner reference for navigation
                connectors[INPUT] = inputConnector;
                //inputPoints.Add(inputConnector);
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
                    outputConnector.parentBlock = this;  // Set owner reference for navigation
                    connectors[OUTPUT] = outputConnector;
                    //outputPoints.Add(outputConnector);
                }
            }

            if (outputPointsVisuals.Count == 0)
            {
                Debug.LogWarning($"BlockUI ({gameObject.name}): No output points assigned in Inspector!");
            }
        }
    }
}
