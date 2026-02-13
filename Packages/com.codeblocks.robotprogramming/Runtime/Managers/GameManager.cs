using PU.Promises;
using System;
using System.Collections.Generic;
using CodeBlocks.Core;
using CodeBlocks.Data;
using CodeBlocks.Execution;
using CodeBlocks.Robot;
using CodeBlocks.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlocks.Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private RobotController robotController;
        [SerializeField] private CommandExecutor commandExecutor;
        [SerializeField] private BlockPalette blockPalette;
        [SerializeField] private ProgramArea programArea;
        [SerializeField] private Button runButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI progressText;
        
        [Header("Level Settings")]
        [SerializeField] private LevelGridData currentLevel;
        [SerializeField] private LevelRuntimeManager levelRuntimeManager;

        private ICommand currentExecutingCommand;
        private bool isProgramRunning = false;
        private bool isInitialized = false;

        private GridPositionTracker robotPositionTracker;

        public event Action<ICommand> OnCommandStarted;
        public event Action<ICommand> OnCommandCompleted;
        public event Action OnProgramStarted;
        public event Action OnProgramCompleted;
        public event Action<Exception> OnProgramFailed;

        public event Action<LevelStatistics> OnLevelFinished;

        private void Init()
        {
            // Prevent multiple initialization
            if (isInitialized)
            {
                return;
            }

            if (robotController == null)
            {
                robotController = FindObjectOfType<RobotController>();
            }
            robotPositionTracker = robotController.GetComponent<GridPositionTracker>();


            if (commandExecutor == null)
            {
                commandExecutor = FindObjectOfType<CommandExecutor>();
            }

            if (blockPalette == null)
            {
                blockPalette = FindObjectOfType<BlockPalette>();
            }

            if (programArea == null)
            {
                programArea = FindObjectOfType<ProgramArea>();
            }

            // Initialize UI if buttons exist
            if (runButton != null)
            {
                runButton.onClick.AddListener(OnRunButtonClicked);
            }

            if (stopButton != null)
            {
                stopButton.onClick.AddListener(OnStopButtonClicked);
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(OnResetButtonClicked);
            }

            if (clearButton != null)
            {
                clearButton.onClick.AddListener(OnClearButtonClicked);
            }

            // Subscribe to command executor events
            if (commandExecutor != null)
            {
                commandExecutor.OnCommandStarted += OnCommandStartedHandler;
                commandExecutor.OnCommandCompleted += OnCommandCompletedHandler;
                commandExecutor.OnProgramCompleted += OnProgramCompletedHandler;
                commandExecutor.OnProgramFailed += OnProgramFailedHandler;
            }

            // Populate palette
            if (blockPalette != null)
            {
                blockPalette.PopulatePalette();
            }

            levelRuntimeManager ??= FindFirstObjectByType<LevelRuntimeManager>();

            robotPositionTracker.OnGridPositionChanged += OnRobotGridPositionChanged;
            robotPositionTracker.OnMovedToImpassableTerrain += OnRobotMovedToImpassable;
            robotPositionTracker.OnReachedFinish += OnRobotReachedFinish;

            isInitialized = true;
            Debug.Log("GameManager: Initialized successfully");
        }

        private void OnDestroy()
        {
            if (commandExecutor != null)
            {
                commandExecutor.OnCommandStarted -= OnCommandStartedHandler;
                commandExecutor.OnCommandCompleted -= OnCommandCompletedHandler;
                commandExecutor.OnProgramCompleted -= OnProgramCompletedHandler;
                commandExecutor.OnProgramFailed -= OnProgramFailedHandler;
            }

            if (runButton != null)
            {
                runButton.onClick.RemoveListener(OnRunButtonClicked);
            }

            if (stopButton != null)
            {
                stopButton.onClick.RemoveListener(OnStopButtonClicked);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(OnResetButtonClicked);
            }

            if (clearButton != null)
            {
                clearButton.onClick.RemoveListener(OnClearButtonClicked);
            }
            
            robotPositionTracker.OnGridPositionChanged -= OnRobotGridPositionChanged;
            robotPositionTracker.OnMovedToImpassableTerrain -= OnRobotMovedToImpassable;
            robotPositionTracker.OnReachedFinish -= OnRobotReachedFinish;
        }

        private void OnRunButtonClicked()
        {
            if (isProgramRunning)
            {
                Debug.LogWarning("Program is already running!");
                return;
            }

            // Stage 6: Execute via BlockUI connections instead of Command.Next
            BlockUIBase startBlock = programArea.GetFirstBlock();
            if (startBlock == null)
            {
                Debug.LogWarning("No program to run! Please add blocks to the program area.");
                UpdateStatusDisplay("Program is empty!");
                return;
            }

            isProgramRunning = true;
            UpdateStatusDisplay("Executing...");
            OnProgramStarted?.Invoke();

            if (commandExecutor != null && robotController != null)
            {
                commandExecutor.ExecuteProgramFromBlock(startBlock, robotController);
            }
        }

        private void OnStopButtonClicked()
        {
            // NEW: Stop all loop commands
            if (programArea != null)
            {
                List<BlockUIBase> blocks = programArea.GetBlocks();
                foreach (var block in blocks)
                {
                    if (block.Command is Commands.LoopCommand loopCmd)
                    {
                        loopCmd.RequestStop();
                    }
                }
            }

            if (commandExecutor != null)
            {
                commandExecutor.Stop();
            }
            isProgramRunning = false;
            UpdateStatusDisplay("Stopped");
        }

        /// <summary>
        /// Initialize and load a level. Can be called multiple times to switch levels.
        /// This method performs lazy initialization on first call, then loads the specified level.
        /// Always clears the program area and stops any running program.
        /// </summary>
        /// <param name="level">The level data to load</param>
        public void InitLevel(LevelGridData level)
        {
            // Lazy initialization (only happens once)
            if (!isInitialized)
            {
                Init();
            }

            // Stop running program if any
            if (isProgramRunning)
            {
                OnStopButtonClicked();
            }

            // Always clear program when loading new level
            if (programArea != null)
            {
                programArea.ClearProgram();
            }

            // Load the level
            if (level != null)
            {
                LoadLevel(level);
                UpdateStatusDisplay("Level loaded");
            }
            else
            {
                Debug.LogWarning("GameManager: Cannot initialize with null level!");
                UpdateStatusDisplay("Level loading error");
            }
        }

        public void LoadLevel(LevelGridData level)
        {
            if (level == null)
            {
                Debug.LogError("GameManager: Cannot load null level!");
                return;
            }

            if (levelRuntimeManager == null)
            {
                Debug.LogError("GameManager: LevelRuntimeManager not found!");
                return;
            }

            // Load level visuals
            levelRuntimeManager.LoadLevel(level);

            // Position robot at start
            PositionRobotAtStart(level);
            robotController.GetComponent<GridPositionTracker>().Initialize(levelRuntimeManager, level);

            Debug.Log($"GameManager: Level '{level.levelName}' loaded successfully!");
        }
        
        private void PositionRobotAtStart(LevelGridData level)
        {
            if (robotController == null)
            {
                Debug.LogWarning("GameManager: RobotController not found!");
                return;
            }

            // NEW: Unified start point access
            var startObj = level.GetStartPoint();
            if (startObj == null)
            {
                Debug.LogWarning($"GameManager: Level '{level.levelName}' has no start point!");
                return;
            }

            // Convert grid position to world position
            Vector3 worldPos = levelRuntimeManager.GetWorldPosition(startObj.position);

            // Center robot in the cell
            worldPos.x += levelRuntimeManager.CellSize * 0.5f;
            worldPos.z += levelRuntimeManager.CellSize * 0.5f;
            worldPos.y = 0;

            // Convert direction to rotation (NEW: use GetStartDirection)
            CardinalDirection direction = level.GetStartDirection();
            Quaternion worldRot = CardinalDirectionToRotation(direction);

            // Update robot's start position
            robotController.SetStartPosition(worldPos, worldRot);

            // Apply immediately (teleport robot)
            robotController.Reset();

            Debug.Log($"GameManager: Robot positioned at grid {startObj.position}, world {worldPos}, direction {direction}");
        }
        
        private Quaternion CardinalDirectionToRotation(CardinalDirection dir)
        {
            float angle = dir switch
            {
                CardinalDirection.North => 0f,
                CardinalDirection.East => 90f,
                CardinalDirection.South => 180f,
                CardinalDirection.West => 270f,
                _ => 0f
            };
            return Quaternion.Euler(0, angle, 0);
        }

        // =========================
        // PUBLIC API for external control
        // =========================

        /// <summary>
        /// Starts program execution from external code. Equivalent to clicking Run button.
        /// </summary>
        public void StartProgram()
        {
            OnRunButtonClicked();
        }

        /// <summary>
        /// Stops program execution from external code. Equivalent to clicking Stop button.
        /// </summary>
        public void StopProgram()
        {
            OnStopButtonClicked();
        }

        /// <summary>
        /// Clears all blocks from program area. Equivalent to clicking Clear button.
        /// Automatically stops running program if any.
        /// </summary>
        public void ClearProgram()
        {
            OnClearButtonClicked();
        }

        /// <summary>
        /// Returns true if program is currently running.
        /// </summary>
        public bool IsProgramRunning => isProgramRunning;

        /// <summary>
        /// Returns number of blocks currently in program area.
        /// </summary>
        public int GetBlocksCount()
        {
            return programArea?.GetBlocks().Count ?? 0;
        }

        private void OnResetButtonClicked()
        {
            // Stop program if running (reuses OnStopButtonClicked logic)
            if (isProgramRunning)
            {
                OnStopButtonClicked();
            }

            // Reset robot to start position
            if (robotController != null)
            {
                robotController.Reset();
            }

            // Reset position tracker
            robotPositionTracker?.ResetPosition();

            // Update UI
            UpdateStatusDisplay("Reset completed");
        }

        private void OnClearButtonClicked()
        {
            if (isProgramRunning)
            {
                OnStopButtonClicked();
            }

            if (programArea != null)
            {
                programArea.ClearProgram();
            }

            UpdateStatusDisplay("Program cleared");
        }

        private void OnCommandStartedHandler(ICommand command)
        {
            currentExecutingCommand = command;
            OnCommandStarted?.Invoke(command);
            UpdateStatusDisplay($"Executing: {command.GetDisplayName()}");
            HighlightBlock(command);
        }

        private void OnCommandCompletedHandler(ICommand command)
        {
            OnCommandCompleted?.Invoke(command);
            UpdateProgressDisplay();
        }

        private void OnProgramCompletedHandler()
        {
            isProgramRunning = false;
            currentExecutingCommand = null;
            OnProgramCompleted?.Invoke();
            UpdateStatusDisplay("Program completed!");
            ClearBlockHighlight();
        }
        
        private void OnRobotReachedFinish()
        {
            Debug.Log("🎉 GameManager: Robot reached finish!");

            // Stop program execution
            if (commandExecutor != null)
            {
                commandExecutor.Stop();
            }

            // Stop all loop commands
            if (programArea != null)
            {
                List<BlockUIBase> blocks = programArea.GetBlocks();
                foreach (var block in blocks)
                {
                    if (block.Command is Commands.LoopCommand loopCmd)
                    {
                        loopCmd.RequestStop();
                    }
                }
            }

            isProgramRunning = false;
            currentExecutingCommand = null;

            // Update UI
            UpdateStatusDisplay("Level completed! 🎉");
            var stat = new LevelStatistics
            {
                blocksUsed = programArea.GetBlocks().Count
            };
            OnLevelFinished?.Invoke(stat);

            // Clear block highlight
            ClearBlockHighlight();

            PlayVictoryEffects();
        }
        
        private void PlayVictoryEffects()
        {
            // Highlight robot in green
            if (robotController != null)
            {
                Renderer robotRenderer = robotController.GetComponent<Renderer>();
                if (robotRenderer != null)
                {
                    robotRenderer.material.color = Color.green;
                }
            }

            // Play victory sound (if AudioSource exists)
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }

            // TODO: Animate finish visual (rotation, scale pulse)
            // TODO: Show particle effect
        }
        
        

        private void OnProgramFailedHandler(Exception exception)
        {
            isProgramRunning = false;
            OnProgramFailed?.Invoke(exception);
            UpdateStatusDisplay($"Error: {exception.Message}");
            ClearBlockHighlight();
            Debug.LogError($"Program execution failed: {exception.Message}");
        }

        private void UpdateStatusDisplay(string message = null)
        {
            if (statusText != null)
            {
                if (message == null)
                {
                    message = isProgramRunning ? "Executing..." : "Ready";
                }
                statusText.text = message;
            }
        }

        private void UpdateProgressDisplay()
        {
            if (progressText != null && commandExecutor != null)
            {
                float progress = commandExecutor.Progress * 100f;
                progressText.text = $"Progress: {progress:F1}%";
            }
        }

        private void HighlightBlock(ICommand command)
        {
            if (programArea == null) return;

            foreach (var block in programArea.GetBlocks())
            {
                if (block.Command == command)
                {
                    Image blockImage = block.GetComponent<Image>();
                    if (blockImage != null)
                    {
                        blockImage.color = Color.Lerp(block.Command.GetBlockColor(), Color.white, 0.5f);
                    }
                }
            }
        }

        private void ClearBlockHighlight()
        {
            if (programArea == null) return;

            foreach (var block in programArea.GetBlocks())
            {
                Image blockImage = block.GetComponent<Image>();
                if (blockImage != null && block.Command != null)
                {
                    blockImage.color = block.Command.GetBlockColor();
                }
            }
        }
        
        // Event handlers
        private void OnRobotGridPositionChanged(Vector2Int newPos, Vector2Int oldPos)
        {
            Debug.Log($"GameManager: Robot moved from {oldPos} to {newPos}");

            // Validate positioning accuracy
            if (robotPositionTracker != null && !robotPositionTracker.IsOnGrid())
            {
                Debug.LogWarning($"GameManager: Robot is not precisely on grid! Distance: {robotPositionTracker.GetDistanceFromGrid():F3}");
            }
        }

        private void OnRobotMovedToImpassable(Vector2Int gridPos)
        {
            Debug.LogWarning($"GameManager: ⚠️ Robot moved to impassable terrain at {gridPos}");
            // TODO: Handle game over, restart level, etc. (future task)
        }
    }
}
