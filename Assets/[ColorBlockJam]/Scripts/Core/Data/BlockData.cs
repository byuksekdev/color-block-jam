using System;
using System.Collections.Generic;

namespace ColorBlockJam.Core.Data
{
    [Serializable]
    public sealed class BlockData
    {
        public BlockColor Color;
        public List<Cell> Cells = new List<Cell>();
        public BlockAxis Axis;

        /// <summary>How many other blocks have to leave the level before this block can be moved. 0 for a block without ice.</summary>
        public int IceCount;
    }
}
