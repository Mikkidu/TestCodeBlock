using System;
using System.Collections.Generic;
using System.Linq;
using CodeBlocks.Commands;
using UnityEngine;

namespace CodeBlocks.UI
{
    public class LoopBlockUI : BlockUIBase
    {
        [Header("External Connectors")]
        [SerializeField] private RectTransform externalInputPoint;   // input (outside left top)
        [SerializeField] private RectTransform externalOutputPoint;  // output (outside left bottom)

        [Header("Internal Connectors")]
        [SerializeField] private RectTransform internalOutputPoint;  // upper internal (passes control inside)
        [SerializeField] private RectTransform internalInputPoint;   // lower internal (receives return)

        [Header("Visual Elements")]
        [SerializeField] private RectTransform header;      // 300x50
        [SerializeField] private RectTransform footer;      // 250x25
        [SerializeField] private RectTransform container;   // main container

        // Size constants
        private const float HEADER_HEIGHT = 50f;
        private const float FOOTER_HEIGHT = 25f;
        private const float MIN_INNER_GAP = 25f;
        public BlockConnector InternalOutput => GetConnector(INTERNAL_OUTPUT);

        protected override void Awake()
        {
            base.Awake();
            OnAlignmentComplete += RecalculateSize;
        }

        private void OnDestroy()
        {
            OnAlignmentComplete -= RecalculateSize;
        }

        public override void InitializeConnectors()
        {
            AddConnector(EXTERNAL_INPUT, BlockConnector.PointType.Input, externalInputPoint);
            AddConnector(EXTERNAL_OUTPUT, BlockConnector.PointType.Output, externalOutputPoint);

            // Internal connectors (for blocks inside the loop)
            AddConnector(INTERNAL_OUTPUT, BlockConnector.PointType.Output, internalOutputPoint,
                BlockConnector.ConnectorRole.InternalOutput);
            AddConnector(INTERNAL_INPUT, BlockConnector.PointType.Input, internalInputPoint,
                BlockConnector.ConnectorRole.InternalInput);
            
            ConnectInnerConnectors();
        }
        
        public override BlockConnector GetPrimaryInput()
        {
            return GetConnector(EXTERNAL_INPUT);
        }

        public override BlockConnector GetPrimaryOutput()
        {
            return GetConnector(EXTERNAL_OUTPUT);
        }

        /// <summary>
        /// Get the first block inside the loop (connected to InternalOutput)
        /// </summary>
        public BlockUIBase GetFirstInnerBlock()
        {
            var intOut = GetConnector(INTERNAL_OUTPUT);
            if (intOut == null || intOut.connectedTo == null || intOut.connectedTo.parentBlock == this )
                return null;

            return intOut.connectedTo.parentBlock;
        }

        /// <summary>
        /// Check if the loop contains at least one block
        /// </summary>
        public bool HasInnerBlocks()
        {
            return GetFirstInnerBlock() != null;
        }

        public void ConnectInnerConnectors()
        {
            if (HasInnerBlocks()) return;
            
            var intOut = GetConnector(INTERNAL_OUTPUT);
            var intIn = GetConnector(INTERNAL_INPUT);
            
            intOut.connectedTo = intIn;
            intIn.connectedTo = intOut;
        }

        /// <summary>
        /// Get the last block in the chain inside the loop DIRECTLY via InternalInput
        /// (Input connector is bound to the last block by definition)
        /// </summary>
        public BlockUIBase GetLastInnerBlockDirect()
        {
            var intIn = GetConnector(INTERNAL_INPUT);
            if (intIn == null || intIn.connectedTo == null || intIn.connectedTo.parentBlock == this)
                return null;

            return intIn.connectedTo.parentBlock;
        }
        

        /// <summary>
        /// Recalculate Loop container height based on contents
        /// </summary>
          public override void RecalculateSize()
          {
              Debug.Log("[LOOP UI] RecalculateSize() called");

              if (container == null)
              {
                  Debug.LogWarning("[LOOP UI] Container not assigned");
                  return;
              }

              var lastBlock = GetLastInnerBlockDirect();
              float innerHeight;

              if (lastBlock == null)
              {
                  innerHeight = MIN_INNER_GAP;
                  Debug.Log("[LOOP UI] Empty loop - minimum height");
              }
              else
              {
                  if (internalOutputPoint != null)
                  {
                      float internalOutputWorldY = internalOutputPoint.position.y / transform.lossyScale.y;
                      var lastBlockOutput = lastBlock.GetPrimaryOutput();

                      if (lastBlockOutput?.visualElement != null)
                      {
                          float lastBlockOutY = lastBlockOutput.visualElement.position.y / lastBlock.transform.lossyScale.y;
                          float distance = MathF.Abs(internalOutputWorldY - lastBlockOutY);
                          innerHeight = Mathf.Max(MIN_INNER_GAP, distance);
                          Debug.Log($"[LOOP UI] Calculated inner height: {innerHeight}");
                      }
                      else
                      {
                          innerHeight = MIN_INNER_GAP;
                      }
                  }
                  else
                  {
                      innerHeight = MIN_INNER_GAP;
                  }
              }

              float totalHeight = HEADER_HEIGHT + innerHeight + FOOTER_HEIGHT;

              RectTransform containerRect = container.GetComponent<RectTransform>();
              if (containerRect != null)
              {
                  containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, totalHeight);
                  Debug.Log($"[LOOP UI] Resized container to height: {totalHeight}");
              }
          }
    }
}
