using System.Collections.Generic;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Editing
{
    /// <summary>The shapes offered by the level editor. A level file stores cells, so adding a shape here needs no other change.</summary>
    public static class BlockShapes
    {
        public static readonly IReadOnlyList<BlockShape> All = new[]
        {
            new BlockShape("1 x 1", new Cell(0, 0)),
            new BlockShape("1 x 2", new Cell(0, 0), new Cell(1, 0)),
            new BlockShape("1 x 3", new Cell(0, 0), new Cell(1, 0), new Cell(2, 0)),
            new BlockShape("2 x 2", new Cell(0, 0), new Cell(1, 0), new Cell(0, 1), new Cell(1, 1)),
            new BlockShape("2 x 3", new Cell(0, 0), new Cell(1, 0), new Cell(2, 0), new Cell(0, 1), new Cell(1, 1), new Cell(2, 1)),
            new BlockShape("Corner", new Cell(0, 0), new Cell(1, 0), new Cell(0, 1)),
            new BlockShape("L", new Cell(0, 0), new Cell(1, 0), new Cell(0, 1), new Cell(0, 2)),
            new BlockShape("T", new Cell(0, 0), new Cell(1, 0), new Cell(2, 0), new Cell(1, 1)),
            new BlockShape("S", new Cell(0, 0), new Cell(1, 0), new Cell(1, 1), new Cell(2, 1)),
        };
    }
}
