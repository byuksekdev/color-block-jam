using System.Collections.Generic;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Editing
{
    /// <summary>A block outline a designer can stamp onto the board. Its cells start at (0, 0).</summary>
    public sealed class BlockShape
    {
        public BlockShape(string name, params Cell[] cells)
        {
            Name = name;
            Cells = cells;
        }

        public string Name { get; }
        public IReadOnlyList<Cell> Cells { get; }
    }
}
