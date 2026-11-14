using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>Where the board lies in the world: it maps board cells to world positions and back.</summary>
    public readonly struct BoardSpace
    {
        public const float CellSize = 2f;

        public BoardSpace(Vector3 center, int width, int height)
        {
            Width = width;
            Height = height;
            Center = center;
            Min = center - new Vector3(width * CellSize / 2f, 0f, height * CellSize / 2f);
        }

        public int Width { get; }
        public int Height { get; }
        public Vector3 Center { get; }

        /// <summary>The world position of the lower left corner of cell (0, 0).</summary>
        public Vector3 Min { get; }

        public Vector3 Max => Min + new Vector3(Width * CellSize, 0f, Height * CellSize);

        /// <summary>The world position of the lower left corner of a cell, or of a point between cells.</summary>
        public Vector3 ToWorld(BoardPoint point)
        {
            return Min + new Vector3(point.X * CellSize, 0f, point.Y * CellSize);
        }

        public Vector3 CellCenter(Cell cell)
        {
            return Min + new Vector3((cell.X + 0.5f) * CellSize, 0f, (cell.Y + 0.5f) * CellSize);
        }

        public BoardPoint ToBoardPoint(Vector3 worldPosition)
        {
            return new BoardPoint((worldPosition.x - Min.x) / CellSize, (worldPosition.z - Min.z) / CellSize);
        }
    }
}
