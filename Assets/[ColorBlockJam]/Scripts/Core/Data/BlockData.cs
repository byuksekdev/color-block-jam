using System;
using System.Collections.Generic;

namespace ColorBlockJam.Core.Data
{
    [Serializable]
    public sealed class BlockData
    {
        public BlockColor Color;
        public List<Cell> Cells = new List<Cell>();
    }
}
