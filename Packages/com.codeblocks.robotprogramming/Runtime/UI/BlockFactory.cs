using CodeBlocks.Commands;
using CodeBlocks.Core;
using CodeBlocks.Data;
using CodeBlocks.Robot;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlocks.UI
{
    public class BlockFactory : MonoBehaviour
    {
        [SerializeField] private GameObject blockPrefab;
        [SerializeField] private GameObject loopBlockPrefab;
        [SerializeField] public RobotConfig robotConfig;
        private int nextCommandId = 0;

        public BlockUIBase CreateBlock(CommandType commandType, Transform parent = null)
        {
            GameObject prefabToUse = commandType == CommandType.Loop ? loopBlockPrefab : blockPrefab;

            if (prefabToUse == null)
            {
                Debug.LogError($"BlockFactory: Prefab not assigned for {commandType}!");
                return null;
            }

            ICommand command = CreateCommand(commandType);
            if (command == null)
            {
                Debug.LogError($"BlockFactory: Failed to create command of type {commandType}");
                return null;
            }

            GameObject blockGO = Instantiate(prefabToUse, parent);
            blockGO.name = $"Block_{commandType}_{command.Id}";

            BlockUIBase blockUI = blockGO.GetComponent<BlockUIBase>();
            if (blockUI != null)
            {
                blockUI.SetCommand(command);
                blockUI.InitializeConnectors();

                // Special setup for Loop blocks
                if (commandType == CommandType.Loop)
                {
                    LoopBlockUI loopBlockUI = blockGO.GetComponent<LoopBlockUI>();
                    if (loopBlockUI != null && command is Commands.LoopCommand loopCommand)
                    {
                        loopBlockUI.SetCommand(loopCommand);
                        loopCommand.SetLoopBlockUI(loopBlockUI);  // NEW: Pass reference back to command
                        Debug.Log("[FACTORY] Loop block setup complete");
                    }
                }
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

                case CommandType.Loop:
                    return new Commands.LoopCommand(id);

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
