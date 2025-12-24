using PU.Promises;
using RobotProgramming.Core;
using RobotProgramming.Execution;
using RobotProgramming.Robot;
using RobotProgramming.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RobotProgramming.Managers
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

        private ICommand currentExecutingCommand;
        private bool isProgramRunning = false;

        public event Action<ICommand> OnCommandStarted;
        public event Action<ICommand> OnCommandCompleted;
        public event Action OnProgramStarted;
        public event Action OnProgramCompleted;
        public event Action<Exception> OnProgramFailed;

        private void Awake()
        {
            if (robotController == null)
            {
                robotController = FindObjectOfType<RobotController>();
            }

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

            UpdateStatusDisplay();
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
        }

        private void OnRunButtonClicked()
        {
            if (isProgramRunning)
            {
                Debug.LogWarning("Program is already running!");
                return;
            }

            ICommand startCommand = programArea.GetProgramStartCommand();
            if (startCommand == null)
            {
                Debug.LogWarning("No program to run! Please add blocks to the program area.");
                UpdateStatusDisplay("Программа пуста!");
                return;
            }

            isProgramRunning = true;
            UpdateStatusDisplay("Выполняется...");
            OnProgramStarted?.Invoke();

            if (commandExecutor != null && robotController != null)
            {
                commandExecutor.ExecuteProgram(startCommand, robotController);
            }
        }

        private void OnStopButtonClicked()
        {
            if (commandExecutor != null)
            {
                commandExecutor.Stop();
            }
            isProgramRunning = false;
            UpdateStatusDisplay("Остановлено");
        }

        private void OnResetButtonClicked()
        {
            if (commandExecutor != null)
            {
                commandExecutor.Stop();
            }

            if (robotController != null)
            {
                robotController.Reset();
            }

            isProgramRunning = false;
            currentExecutingCommand = null;
            UpdateStatusDisplay("Сброс завершен");
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

            UpdateStatusDisplay("Программа очищена");
        }

        private void OnCommandStartedHandler(ICommand command)
        {
            currentExecutingCommand = command;
            OnCommandStarted?.Invoke(command);
            UpdateStatusDisplay($"Выполняется: {command.GetDisplayName()}");
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
            UpdateStatusDisplay("Программа завершена!");
            ClearBlockHighlight();
        }

        private void OnProgramFailedHandler(Exception exception)
        {
            isProgramRunning = false;
            OnProgramFailed?.Invoke(exception);
            UpdateStatusDisplay($"Ошибка: {exception.Message}");
            ClearBlockHighlight();
            Debug.LogError($"Program execution failed: {exception.Message}");
        }

        private void UpdateStatusDisplay(string message = null)
        {
            if (statusText != null)
            {
                if (message == null)
                {
                    message = isProgramRunning ? "Выполняется..." : "Готово";
                }
                statusText.text = message;
            }
        }

        private void UpdateProgressDisplay()
        {
            if (progressText != null && commandExecutor != null)
            {
                float progress = commandExecutor.Progress * 100f;
                progressText.text = $"Прогресс: {progress:F1}%";
            }
        }

        private void HighlightBlock(ICommand command)
        {
            if (programArea == null) return;

            foreach (BlockUI block in programArea.GetBlocks())
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

            foreach (BlockUI block in programArea.GetBlocks())
            {
                Image blockImage = block.GetComponent<Image>();
                if (blockImage != null && block.Command != null)
                {
                    blockImage.color = block.Command.GetBlockColor();
                }
            }
        }
    }
}
