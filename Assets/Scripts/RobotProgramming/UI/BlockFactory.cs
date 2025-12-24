using RobotProgramming.Commands;
using RobotProgramming.Core;
using RobotProgramming.Data;
using RobotProgramming.Robot;
using UnityEngine;
using UnityEngine.UI;

namespace RobotProgramming.UI
{
    public class BlockFactory : MonoBehaviour
    {
        [SerializeField] private GameObject blockPrefab;
        [SerializeField] public RobotConfig robotConfig;
        private int nextCommandId = 0;

        public BlockUI CreateBlock(CommandType commandType, Transform parent = null)
        {
            if (blockPrefab == null)
            {
                Debug.LogError("BlockFactory: blockPrefab is not assigned!");
                return null;
            }

            ICommand command = CreateCommand(commandType);
            if (command == null)
            {
                Debug.LogError($"BlockFactory: Failed to create command of type {commandType}");
                return null;
            }

            GameObject blockGO = Instantiate(blockPrefab, parent);
            blockGO.name = $"Block_{commandType}_{command.Id}";

            BlockUI blockUI = blockGO.GetComponent<BlockUI>();
            if (blockUI != null)
            {
                blockUI.SetCommand(command);
                blockUI.InitializeConnectors();
            }

            return blockUI;
        }

        private ICommand CreateCommand(CommandType commandType)
        {
            int id = nextCommandId++;

            switch (commandType)
            {
                case CommandType.MoveForward:
                    return new MoveForwardCommand(id, robotConfig != null ? robotConfig.moveDistance : 1f);

                case CommandType.MoveBackward:
                    return new MoveBackwardCommand(id, robotConfig != null ? robotConfig.moveDistance : 1f);

                case CommandType.TurnLeft:
                    return new TurnLeftCommand(id);

                case CommandType.TurnRight:
                    return new TurnRightCommand(id);

                case CommandType.Wait:
                    return new WaitCommand(id, 1f);

                default:
                    Debug.LogWarning($"Unknown command type: {commandType}");
                    return null;
            }
        }

        public void ResetIdCounter()
        {
            nextCommandId = 0;
        }
    }
}
