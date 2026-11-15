using System;
using System.Collections.Generic;
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

        /// <summary>
        /// The point of a shape at which a mark on it is drawn: the middle of its bounds for a full
        /// rectangle, and otherwise the middle of the cell that lies nearest to that point.
        /// </summary>
        public static BoardPoint MiddleOf(IReadOnlyList<Cell> cells)
        {
            Cell min = cells[0];
            Cell max = cells[0];
            for (int i = 1; i < cells.Count; i++)
            {
                min = new Cell(Math.Min(min.X, cells[i].X), Math.Min(min.Y, cells[i].Y));
                max = new Cell(Math.Max(max.X, cells[i].X), Math.Max(max.Y, cells[i].Y));
            }

            float middleX = (min.X + max.X + 1) / 2f;
            float middleY = (min.Y + max.Y + 1) / 2f;
            if (cells.Count == (max.X - min.X + 1) * (max.Y - min.Y + 1))
            {
                return new BoardPoint(middleX, middleY);
            }

            var nearest = new BoardPoint(cells[0].X + 0.5f, cells[0].Y + 0.5f);
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < cells.Count; i++)
            {
                float offsetX = cells[i].X + 0.5f - middleX;
                float offsetY = cells[i].Y + 0.5f - middleY;
                float distance = offsetX * offsetX + offsetY * offsetY;
                if (distance < nearestDistance)
                {
                    nearest = new BoardPoint(cells[i].X + 0.5f, cells[i].Y + 0.5f);
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        public Cell NearestCell()
        {
            return new Cell(
                (int)MathF.Round(X, MidpointRounding.AwayFromZero),
                (int)MathF.Round(Y, MidpointRounding.AwayFromZero));
        }
    }
}
