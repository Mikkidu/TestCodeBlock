using System;
using System.Collections.Generic;
using System.Linq;
using CodeBlocks.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace CodeBlocks.UI
{
      /// <summary>
      /// Abstract base class for all block types (simple commands, loops, conditionals)
      /// </summary>
      public abstract class BlockUIBase : MonoBehaviour
      {
          public event Action OnAlignmentComplete;

          [Header("Visual Elements")]
          [SerializeField] protected Image blockImage;
          [SerializeField] protected TextMeshProUGUI blockLabel;

          // Unified connector storage (replaces separate inputPoints/outputPoints lists)
          protected Dictionary<string, BlockConnector> connectors = new Dictionary<string, BlockConnector>();

          // Standard connector names (for simple blocks)
          public const string INPUT = "input";
          public const string OUTPUT = "output";

          // Loop connector names
          public const string EXTERNAL_INPUT = "external_input";
          public const string EXTERNAL_OUTPUT = "external_output";
          public const string INTERNAL_INPUT = "internal_input";
          public const string INTERNAL_OUTPUT = "internal_output";

          private ProgramArea _programArea;
          
          protected ICommand command;

          protected ProgramArea programArea
          {
              get
              {
                  if (_programArea == null)
                  {
                      _programArea = GetComponentInParent<ProgramArea>()
                                     ?? FindAnyObjectByType<ProgramArea>();
                  }
                  return _programArea;
              }
          }
          protected Canvas rootCanvas;

          // Track highlighted connector for cleanup
          private BlockConnector previousHighlightedConnector;

          public ICommand Command => command;
          public bool inProgramArea = false;
          private SnapLineRenderer lineRenderer;

          #region Connector Access (Unified API)

          /// <summary>
          /// Get a connector by its name
          /// </summary>
          public BlockConnector GetConnector(string name)
          {
              return connectors.TryGetValue(name, out var connector) ? connector : null;
          }

          /// <summary>
          /// Get all connectors for iteration
          /// </summary>
          public IEnumerable<BlockConnector> GetAllConnectors()
          {
              return connectors.Values;
          }

          /// <summary>
          /// Get all INPUT type connectors
          /// </summary>
          public IEnumerable<BlockConnector> GetInputConnectors()
          {
              foreach (var c in connectors.Values)
              {
                  if (c.pointType == BlockConnector.PointType.Input)
                      yield return c;
              }
          }

          /// <summary>
          /// Get all OUTPUT type connectors
          /// </summary>
          public IReadOnlyList<BlockConnector> GetOutputConnectors()
          {
              return connectors.Values
                  .Where(c => c.pointType == BlockConnector.PointType.Output)
                  .ToList();
          }

          /// <summary>
          /// Get primary input connector (for simple blocks)
          /// </summary>
          public virtual BlockConnector GetPrimaryInput()
          {
              return GetConnector(INPUT) ?? GetConnector(EXTERNAL_INPUT);
          }

          /// <summary>
          /// Get primary output connector (for simple blocks)
          /// </summary>
          public virtual BlockConnector GetPrimaryOutput()
          {
              return GetConnector(OUTPUT) ?? GetConnector(EXTERNAL_OUTPUT);
          }

          #endregion

          #region Unity Lifecycle

          protected virtual void Awake()
          {
              rootCanvas = GetComponentInParent<Canvas>();
              // Initialize connectors in derived class
              //InitializeConnectors();
          }

          #endregion

          #region Abstract Methods (must be implemented by subclasses)

          /// <summary>
          /// Initialize connectors specific to this block type.
          /// Called from Awake().
          /// </summary>
          public abstract void InitializeConnectors();

          /// <summary>
          /// Recalculate block size (for dynamic blocks like Loop).
          /// Default implementation does nothing.
          /// </summary>
          public virtual void RecalculateSize() { }

          #endregion

          #region Command Setup

          public virtual void SetCommand(ICommand cmd)
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

          #endregion
          

          #region Connection Navigation

          /// <summary>
          /// Get the next block connected to this block's primary output.
          /// Override in subclasses for different navigation logic.
          /// </summary>
          public virtual BlockUIBase GetNextBlock()
          {
              var output = GetPrimaryOutput();
              if (output != null && output.connectedTo != null)
              {
                  return output.connectedTo.parentBlock;
              }
              return null;
          }

          /// <summary>
          /// Get the previous block connected to this block's primary input.
          /// Override in subclasses for different navigation logic.
          /// </summary>
          public virtual BlockUIBase GetPreviousBlock()
          {
              var input = GetPrimaryInput();
              if (input != null && input.connectedTo != null)
              {
                  return input.connectedTo.parentBlock;
              }
              return null;
          }

          /// <summary>
          /// Disconnect all outgoing connections from this block
          /// </summary>
          public virtual void DisconnectAllConnections()
          {
              // Disconnect outgoing
              foreach (var connector in GetAllConnectors())
              {
                  var connectedOther = connector.connectedTo;
                  if (connectedOther != null && connectedOther.connectedTo == connector) connectedOther.connectedTo = null;
                  connector.connectedTo = null;
               }
          }

          /// <summary>
          /// Align this block to its input connection and propagate alignment to next block.
          /// </summary>
          public virtual void AlignToInputConnection()
          {
              if (!inProgramArea) return;

              var myInput = GetPrimaryInput();
              if (myInput == null) return;

              var connectedOutput = myInput.connectedTo;
              if (connectedOutput != null)
              {
                  Vector2 outputPos = connectedOutput.GetWorldPosition();
                  Vector2 myInputPos = myInput.GetWorldPosition();
                  Vector2 offset = outputPos - myInputPos;

                  RectTransform rect = GetComponent<RectTransform>();
                  if (rect != null)
                  {
                      Vector3 newWorldPos = new Vector3(
                          rect.position.x + offset.x,
                          rect.position.y + offset.y,
                          rect.position.z
                      );
                      SetWorldPosition(newWorldPos);
                      Debug.Log($"[ALIGN] {gameObject.name} aligned to {connectedOutput.parentBlock?.gameObject.name}");
                  }
              }
              OnAlignmentComplete?.Invoke();
              GetNextBlock()?.AlignToInputConnection();
          }
          
          /// <summary>
          /// Set block position in world space, converting to local coordinates of parent.
          /// Used for alignment during snap and drag operations.
          /// Automatically retrieves necessary components, no parameters required.
          /// </summary>
          public void SetWorldPosition(Vector3 worldPosition)
          {
              RectTransform rect = transform as RectTransform;
              if (rect == null) return;

              // Get parent and canvas
              RectTransform parentRect = rect.parent as RectTransform;
              if (parentRect == null || rootCanvas == null || rootCanvas.worldCamera == null)
              {
                  // Fallback: if no parent or canvas, use direct positioning
                  rect.position = worldPosition;
                  return;
              }

              // Convert world position to local coordinates of parent
              Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(rootCanvas.worldCamera, worldPosition);

              RectTransformUtility.ScreenPointToLocalPointInRectangle(
                  parentRect,
                  screenPos,
                  rootCanvas.worldCamera,
                  out Vector2 localPos);

              rect.anchoredPosition = localPos;
          }

          #endregion

          #region Snap Visualization

          public virtual void UpdateSnapVisuals(SnapManager.SnapInfo snapInfo)
          {
              lineRenderer ??= programArea.SnapLineRenderer;

              if (snapInfo.canSnap && snapInfo.targetConnector != null)
              {
                  // Reset previously highlighted connector if different
                  if (previousHighlightedConnector != null && previousHighlightedConnector != snapInfo.targetConnector)
                  {
                      ResetConnectorColor(previousHighlightedConnector);
                  }

                  // Draw line and highlight based on snap type
                  Vector2 fromPos = Vector2.zero;
                  Vector2 toPos = snapInfo.targetConnector.GetWorldPosition();
                  Color lineColor = Color.white;

                  BlockConnector myConnector = null;

                  if (snapInfo.snapType == SnapManager.SnapInfo.SnapType.InputToOutput)
                  {
                      myConnector = GetPrimaryInput();
                      lineColor = new Color(0f, 1f, 1f, 1f); // Cyan
                  }
                  else if (snapInfo.snapType == SnapManager.SnapInfo.SnapType.OutputToInput)
                  {
                      myConnector = GetPrimaryOutput();
                      lineColor = new Color(1f, 1f, 0f, 1f); // Yellow
                  }

                  if (myConnector != null)
                  {
                      fromPos = myConnector.GetWorldPosition();

                      if (lineRenderer != null && fromPos != Vector2.zero && toPos != Vector2.zero)
                      {
                          lineRenderer.DrawLine(fromPos, toPos, lineColor);
                      }

                      // Highlight both connectors
                      HighlightConnector(myConnector, new Color(1f, 1f, 0f, 1f));
                      HighlightConnector(snapInfo.targetConnector, new Color(1f, 1f, 0f, 1f));
                      previousHighlightedConnector = snapInfo.targetConnector;
                  }
              }
              else
              {
                  // Clear line when snap not valid
                  lineRenderer?.ClearLine();

                  if (previousHighlightedConnector != null)
                  {
                      ResetConnectorColor(previousHighlightedConnector);
                      previousHighlightedConnector = null;
                  }

                  ResetAllConnectorColors();
              }
          }

          protected void ClearSnapVisualization()
          {
              lineRenderer ??= programArea.SnapLineRenderer;
              lineRenderer?.ClearLine();

              if (previousHighlightedConnector != null)
              {
                  ResetConnectorColor(previousHighlightedConnector);
                  previousHighlightedConnector = null;
              }

              ResetAllConnectorColors();
          }

          protected void HighlightConnector(BlockConnector connector, Color color)
          {
              if (connector?.visualElement == null) return;
              var image = connector.visualElement.GetComponent<Image>();
              if (image != null) image.color = color;
          }

          protected void ResetConnectorColor(BlockConnector connector)
          {
              lineRenderer ??= programArea.SnapLineRenderer;
              lineRenderer?.ClearLine();
              
              if (connector?.visualElement == null) return;
              var image = connector.visualElement.GetComponent<Image>();
              if (image != null)
              {
                  image.color = connector.pointType == BlockConnector.PointType.Input
                      ? new Color(0f, 1f, 0f, 1f)  // Green for input
                      : new Color(1f, 0f, 0f, 1f); // Red for output
              }
          }

          public virtual void ResetAllConnectorColors()
          {
              foreach (var connector in connectors.Values)
              {
                  ResetConnectorColor(connector);
              }
          }

          #endregion

          #region Helper: Add Connector

          /// <summary>
          /// Helper to add a connector to the dictionary
          /// </summary>
          protected BlockConnector AddConnector(string name, BlockConnector.PointType type, RectTransform visual, 
              BlockConnector.ConnectorRole role = BlockConnector.ConnectorRole.External)
          {
              if (visual == null)
              {
                  Debug.LogWarning($"{gameObject.name}: Connector visual '{name}' not assigned!");
                  return null;
              }

              // Set color based on type
              var image = visual.GetComponent<Image>();
              if (image != null)
              {
                  image.color = type == BlockConnector.PointType.Input
                      ? new Color(0f, 1f, 0f, 1f)  // Green
                      : new Color(1f, 0f, 0f, 1f); // Red
              }

              var connector = new BlockConnector(type, visual)
              {
                  parentBlock = this,
                  role = role
              };

              connectors[name] = connector;
              return connector;
          }

          #endregion
      }
}