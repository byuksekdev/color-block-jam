using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Editing
{
    /// <summary>Turns and mirrors a shape. The result starts at (0, 0) again.</summary>
    public static class ShapeTransform
    {
        public static Cell[] RotateClockwise(IReadOnlyList<Cell> cells)
        {
            var rotated = new Cell[cells.Count];
            for (int i = 0; i < rotated.Length; i++)
            {
                rotated[i] = new Cell(cells[i].Y, -cells[i].X);
            }

            return MoveToOrigin(rotated);
        }

        public static Cell[] FlipHorizontally(IReadOnlyList<Cell> cells)
        {
            var flipped = new Cell[cells.Count];
            for (int i = 0; i < flipped.Length; i++)
            {
                flipped[i] = new Cell(-cells[i].X, cells[i].Y);
            }

            return MoveToOrigin(flipped);
        }

        private static Cell[] MoveToOrigin(Cell[] cells)
        {
            if (cells.Length == 0)
            {
                return cells;
            }

            Cell corner = cells[0];
            for (int i = 1; i < cells.Length; i++)
            {
                corner = new Cell(Math.Min(corner.X, cells[i].X), Math.Min(corner.Y, cells[i].Y));
            }

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] -= corner;
            }

            return cells;
        }
    }
}
