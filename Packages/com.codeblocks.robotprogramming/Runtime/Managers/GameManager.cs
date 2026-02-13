using System;
using CodeBlocks.Core;
using CodeBlocks.Data;
using CodeBlocks.Execution;
using CodeBlocks.Robot;
using CodeBlocks.Reactions;
using CodeBlocks.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlocks.Managers
{
    public class GameManager : MonoBehaviour, IMovementDecisionProvider, ITargetTriggerProvider, ICellActivationProvider
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
        private bool levelCompleted = false;

        private GridPositionTracker robotPositionTracker;
        private MovementDecisionService movementDecisionService;
        private Vector2Int buttonVisualGridPosition;
        private bool buttonVisualInitialized;

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
                robotController = FindFirstObjectByType<RobotController>();
            }
            robotPositionTracker = robotController.GetComponent<GridPositionTracker>();


            if (commandExecutor == null)
            {
                commandExecutor = FindFirstObjectByType<CommandExecutor>();
            }

            if (blockPalette == null)
            {
                blockPalette = FindFirstObjectByType<BlockPalette>();
            }

            if (programArea == null)
            {
                programArea = FindFirstObjectByType<ProgramArea>();
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
                commandExecutor.OnProgramStopped += OnProgramStoppedHandler;
                commandExecutor.OnProgramFailed += OnProgramFailedHandler;
                commandExecutor.MovementDecisionProvider = this;
            }

            // Populate palette
            if (blockPalette != null)
            {
                blockPalette.PopulatePalette();
            }

            levelRuntimeManager ??= FindFirstObjectByType<LevelRuntimeManager>();
            movementDecisionService ??= new MovementDecisionService();

            robotPositionTracker.OnGridPositionChanged += OnRobotGridPositionChanged;
            robotPositionTracker.OnMovedToImpassableTerrain += OnRobotMovedToImpassable;

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
                commandExecutor.OnProgramStopped -= OnProgramStoppedHandler;
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
        }

        private void LateUpdate()
        {
            UpdateButtonVisualByCurrentPosition();
        }

        private void OnRunButtonClicked()
        {
            if (isProgramRunning)
            {
                Debug.LogWarning("Program is already running!");
                return;
            }

            levelCompleted = false;

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
            if (!isProgramRunning) return;

            levelCompleted = false;
            commandExecutor?.Stop();
            // isProgramRunning and robot reset happen in OnProgramStoppedHandler
            // after the current command actually finishes
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
            robotController.GetComponent<GridPositionTracker>().Initialize(levelRuntimeManager, levelRuntimeManager.CurrentLevel);
            InitializeButtonVisualState();

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
        /// Restart program execution from external code. Equivalent to clicking Restart button.
        /// </summary>
        public void ResetProgram()
        {
            OnResetButtonClicked(); 
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
            if (isProgramRunning)
            {
                // Stop will reset the robot in OnProgramStoppedHandler after the command finishes
                OnStopButtonClicked();
            }
            else
            {
                robotController?.Reset();
                robotPositionTracker?.ResetPosition();
                UpdateStatusDisplay("Reset completed");
            }
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

        private void OnProgramStoppedHandler()
        {
            if (levelCompleted)
            {
                HandleLevelCompletion();
                return;
            }

            string stopReason = commandExecutor != null ? commandExecutor.LastStopReason : string.Empty;
            bool stoppedByReaction = !string.IsNullOrWhiteSpace(stopReason) &&
                                     stopReason.StartsWith("Reaction:", StringComparison.OrdinalIgnoreCase);

            if (stoppedByReaction)
            {
                isProgramRunning = false;
                currentExecutingCommand = null;
                ClearBlockHighlight();
                string reactionName = stopReason.Substring("Reaction:".Length);
                if (string.IsNullOrWhiteSpace(reactionName))
                {
                    UpdateStatusDisplay("Stopped by reaction");
                }
                else
                {
                    UpdateStatusDisplay($"Stopped by reaction: {reactionName}");
                }
                return;
            }

            // User-initiated stop: reset robot after the current command actually finished
            isProgramRunning = false;
            currentExecutingCommand = null;
            ClearBlockHighlight();
            robotController?.Reset();
            robotPositionTracker?.ResetPosition();
            UpdateStatusDisplay("Stopped");
        }
        
        private void OnRobotReachedFinish()
        {
            Debug.Log("GameManager: Robot reached finish!");

            levelCompleted = true;

            if (isProgramRunning && commandExecutor != null)
            {
                commandExecutor.Stop();
                // Victory handled in OnProgramStoppedHandler after chain terminates
            }
            else
            {
                // Program already ended naturally, handle victory directly
                HandleLevelCompletion();
            }
        }

        private void HandleLevelCompletion()
        {
            isProgramRunning = false;
            currentExecutingCommand = null;
            ClearBlockHighlight();
            UpdateStatusDisplay("Level completed!");

            var stat = new LevelStatistics
            {
                blocksUsed = programArea?.GetBlocks().Count ?? 0
            };
            OnLevelFinished?.Invoke(stat);
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
            ApplyButtonVisualTransition(oldPos, newPos);

            // Validate positioning accuracy
            if (robotPositionTracker != null && !robotPositionTracker.IsOnGrid())
            {
                Debug.LogWarning($"GameManager: Robot is not precisely on grid! Distance: {robotPositionTracker.GetDistanceFromGrid():F3}");
            }
        }

        private void OnRobotMovedToImpassable(Vector2Int gridPos)
        {
            Debug.LogWarning($"GameManager: Robot moved to impassable terrain at {gridPos}");
            // TODO: Handle game over, restart level, etc. (future task)
        }

        private void UpdateButtonVisualState(Vector2Int gridPos, bool isPressed)
        {
            if (levelRuntimeManager == null || levelRuntimeManager.CurrentLevel == null)
                return;

            GridObject obj = levelRuntimeManager.CurrentLevel.GetObjectAt(gridPos.x, gridPos.y);
            if (obj == null || !string.Equals(obj.objectTypeId, "Button", StringComparison.OrdinalIgnoreCase))
                return;

            if (!levelRuntimeManager.TryGetObjectInstance(gridPos, out GameObject instance) || instance == null)
                return;

            var visual = instance.GetComponent<Reactions.ButtonVisualState>();
            if (visual != null)
            {
                visual.SetPressed(isPressed);
            }
        }

        private void ApplyButtonVisualTransition(Vector2Int oldPos, Vector2Int newPos)
        {
            UpdateButtonVisualState(oldPos, false);
            UpdateButtonVisualState(newPos, true);
        }

        private void InitializeButtonVisualState()
        {
            if (robotPositionTracker == null || !robotPositionTracker.IsInitialized)
                return;

            buttonVisualGridPosition = robotPositionTracker.GetCurrentGridPositionSnapshot();
            buttonVisualInitialized = true;
            UpdateButtonVisualState(buttonVisualGridPosition, true);
        }

        private void UpdateButtonVisualByCurrentPosition()
        {
            if (robotPositionTracker == null || !robotPositionTracker.IsInitialized)
                return;

            Vector2Int currentGrid = robotPositionTracker.GetCurrentGridPositionSnapshot();
            if (!buttonVisualInitialized)
            {
                buttonVisualGridPosition = currentGrid;
                buttonVisualInitialized = true;
                UpdateButtonVisualState(currentGrid, true);
                return;
            }

            if (currentGrid == buttonVisualGridPosition)
                return;

            ApplyButtonVisualTransition(buttonVisualGridPosition, currentGrid);
            buttonVisualGridPosition = currentGrid;
        }

        public MoveDecision GetMoveDecision(MoveIntent intent)
        {
            if (movementDecisionService == null)
            {
                movementDecisionService = new MovementDecisionService();
            }

            if (levelRuntimeManager == null || robotPositionTracker == null || robotController == null)
            {
                return new MoveDecision(
                    allowMove: false,
                    reactionType: CellReactionType.None,
                    phase: ReactionPhase.Start,
                    speedModifier: 1f,
                    targetGrid: Vector2Int.zero,
                    targetObjectIds: null,
                    obstacleTypeId: "None",
                    surfaceTypeId: "None",
                    debugReason: "Missing runtime manager or tracker"
                );
            }

            CardinalDirection direction = GetRobotDirection();
            if (intent == MoveIntent.Backward)
            {
                direction = Opposite(direction);
            }

            return movementDecisionService.DecideMove(levelRuntimeManager, robotPositionTracker, direction);
        }

        public void TriggerTargets(string[] targetObjectIds)
        {
            if (levelRuntimeManager == null || targetObjectIds == null || targetObjectIds.Length == 0)
                return;

            levelRuntimeManager.ToggleDoorStates(targetObjectIds);
        }

        public void ActivateCurrentCell()
        {
            if (levelRuntimeManager == null || robotPositionTracker == null)
                return;

            Vector2Int currentGrid = robotPositionTracker.GetCurrentGridPositionSnapshot();
            GridObject obj = levelRuntimeManager.CurrentLevel != null
                ? levelRuntimeManager.CurrentLevel.GetObjectAt(currentGrid.x, currentGrid.y)
                : null;

            if (obj == null)
                return;

            if (!levelRuntimeManager.TryGetObjectInstance(currentGrid, out GameObject instance) || instance == null)
                return;

            var context = new CodeBlocks.Core.ObjectReactionContext(obj, currentGrid, currentGrid);
            var components = instance.GetComponents<Reactions.ObjectReactionComponent>();
            if (components == null || components.Length == 0)
                return;

            foreach (var component in components)
            {
                if (component == null)
                    continue;

                if (component.CanHandle(obj.objectTypeId))
                {
                    var result = component.Evaluate(context);
                    if (result.CompleteLevel)
                    {
                        TriggerReactionAnimationForObject(obj.objectTypeId);
                        OnRobotReachedFinish();
                        return;
                    }

                    if (result.TargetObjectIds != null && result.TargetObjectIds.Length > 0)
                    {
                        TriggerTargets(result.TargetObjectIds);
                    }
                }
            }
        }

        private void TriggerReactionAnimationForObject(string obstacleTypeId)
        {
            if (robotController == null || string.IsNullOrWhiteSpace(obstacleTypeId))
            {
                return;
            }

            var profile = ReactionProfileResolver.Resolve(robotController, obstacleTypeId);
            var animationId = ReactionAnimationResolver.ResolveTrigger(robotController, obstacleTypeId, profile.animationId);
            robotController.TriggerReactionAnimation(animationId);
        }

        private CardinalDirection GetRobotDirection()
        {
            float angle = robotController.transform.eulerAngles.y;
            int index = Mathf.RoundToInt(angle / 90f) % 4;
            return (CardinalDirection)index;
        }

        private static CardinalDirection Opposite(CardinalDirection direction)
        {
            return direction switch
            {
                CardinalDirection.North => CardinalDirection.South,
                CardinalDirection.East => CardinalDirection.West,
                CardinalDirection.South => CardinalDirection.North,
                CardinalDirection.West => CardinalDirection.East,
                _ => CardinalDirection.North
            };
        }
    }
}
