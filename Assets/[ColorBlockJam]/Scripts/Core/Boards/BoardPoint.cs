using System;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Boards
{
    /// <summary>A position on the board in cell units, used while a block moves between cells.</summary>
    public readonly struct BoardPoint
    {
        public BoardPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }
        public float Y { get; }

        public static BoardPoint FromCell(Cell cell)
        {
            return new BoardPoint(cell.X, cell.Y);
        }

        public Cell NearestCell()
        {
            return new Cell(
                (int)MathF.Round(X, MidpointRounding.AwayFromZero),
                (int)MathF.Round(Y, MidpointRounding.AwayFromZero));
        }
    }
}
