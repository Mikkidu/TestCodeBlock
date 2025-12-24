using System.Collections.Generic;
using UnityEngine;

namespace RobotProgramming.Data
{
    [System.Serializable]
    public class BlockData
    {
        public int id;
        public CommandType commandType;
        public float[] parameters;
        public int nextBlockId;

        public BlockData(int id, CommandType commandType)
        {
            this.id = id;
            this.commandType = commandType;
            this.parameters = new float[0];
            this.nextBlockId = -1;
        }
    }
}
