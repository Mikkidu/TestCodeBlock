using RobotProgramming.Data;
using RobotProgramming.Robot;
using UnityEngine;

namespace RobotProgramming.UI
{
    public class BlockPalette : MonoBehaviour
    {
        [SerializeField] private BlockFactory blockFactory;
        [SerializeField] private Transform paletteContent;
        [SerializeField] private RobotConfig robotConfig;

        private CommandType[] availableCommands = new[]
        {
            CommandType.MoveForward,
            CommandType.MoveBackward,
            CommandType.TurnLeft,
            CommandType.TurnRight,
            CommandType.Wait
        };

        private void Awake()
        {
            if (blockFactory == null)
            {
                blockFactory = GetComponent<BlockFactory>();
            }

            if (blockFactory == null)
            {
                Debug.LogError("BlockPalette: BlockFactory not found!");
                return;
            }

            if (robotConfig == null)
            {
                robotConfig = Resources.Load<RobotConfig>("RobotConfig");
            }

            blockFactory.robotConfig = robotConfig;
        }

        public void PopulatePalette()
        {
            if (paletteContent == null)
            {
                Debug.LogError("BlockPalette: paletteContent is not assigned!");
                return;
            }

            // Clear existing blocks
            foreach (Transform child in paletteContent)
            {
                Destroy(child.gameObject);
            }

            // Create a block for each command type
            foreach (CommandType cmdType in availableCommands)
            {
                BlockUI blockUI = blockFactory.CreateBlock(cmdType, paletteContent);
                if (blockUI != null)
                {
                    blockUI.gameObject.name = $"PaletteBlock_{cmdType}";
                }
            }
        }

        public void SetRobotConfig(RobotConfig config)
        {
            robotConfig = config;
            if (blockFactory != null)
            {
                blockFactory.robotConfig = config;
            }
        }
    }
}
