using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobotProgramming.Data
{
    [System.Serializable]
    public class ProgramData
    {
        public List<BlockData> blocks;
        public int startBlockId;
        public string programName;
        public System.DateTime createdDate;

        public ProgramData()
        {
            blocks = new List<BlockData>();
            startBlockId = -1;
            programName = "New Program";
            createdDate = System.DateTime.Now;
        }

        public void AddBlock(BlockData blockData)
        {
            blocks.Add(blockData);

            if (startBlockId == -1)
            {
                startBlockId = blockData.id;
            }
        }

        public BlockData GetBlockById(int id)
        {
            foreach (BlockData block in blocks)
            {
                if (block.id == id)
                {
                    return block;
                }
            }
            return null;
        }

        public void Clear()
        {
            blocks.Clear();
            startBlockId = -1;
        }
    }
}
