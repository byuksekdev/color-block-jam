using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>
    /// Where the board sits inside the canvas: the board cells in the middle, and around them a ring
    /// of slots, one cell wide, in which doors are placed.
    /// </summary>
    internal readonly struct BoardLayout
    {
        private const float MaxCellSize = 72f;
        private const float MinCellSize = 8f;
        private const float Margin = 12f;
        private const int RingCells = 2;

        private readonly int _width;
        private readonly int _height;
        private readonly Vector2 _origin;

        public BoardLayout(Rect area, int width, int height)
        {
            _width = width;
            _height = height;

            float fitWidth = (area.width - 2f * Margin) / (width + RingCells);
            float fitHeight = (area.height - 2f * Margin) / (height + RingCells);
            CellSize = Mathf.Floor(Mathf.Min(Mathf.Min(fitWidth, fitHeight), MaxCellSize));

            _origin = new Vector2(
                Mathf.Round(area.center.x - CellSize * width / 2f),
                Mathf.Round(area.center.y - CellSize * height / 2f));
        }

        public float CellSize { get; }
        public bool HasRoom => CellSize >= MinCellSize;
        public Rect BoardRect => new Rect(_origin.x, _origin.y, CellSize * _width, CellSize * _height);

        public Rect CellRect(Cell cell)
        {
            return new Rect(
                _origin.x + cell.X * CellSize,
                _origin.y + (_height - 1 - cell.Y) * CellSize,
                CellSize,
                CellSize);
        }

        /// <summary>A square of one cell's size around a point of the board.</summary>
        public Rect RectAround(BoardPoint point)
        {
            return new Rect(
                _origin.x + (point.X - 0.5f) * CellSize,
                _origin.y + (_height - point.Y - 0.5f) * CellSize,
                CellSize,
                CellSize);
        }

        /// <summary>The strip a door covers, beside the board. Thickness is a fraction of a cell.</summary>
        public Rect DoorRect(BoardSide side, int start, int length, float thickness)
        {
            float depth = CellSize * thickness;
            float along = length * CellSize;

            switch (side)
            {
                case BoardSide.Left:
                    return new Rect(_origin.x - depth, RowTop(start + length - 1), depth, along);
                case BoardSide.Right:
                    return new Rect(_origin.x + _width * CellSize, RowTop(start + length - 1), depth, along);
                case BoardSide.Bottom:
                    return new Rect(_origin.x + start * CellSize, _origin.y + _height * CellSize, along, depth);
                default:
                    return new Rect(_origin.x + start * CellSize, _origin.y - depth, along, depth);
            }
        }

        public BoardHit HitTest(Vector2 point)
        {
            int column = Mathf.FloorToInt((point.x - _origin.x) / CellSize);
            int row = _height - 1 - Mathf.FloorToInt((point.y - _origin.y) / CellSize);
            var cell = new Cell(column, row);

            bool isInColumns = column >= 0 && column < _width;
            bool isInRows = row >= 0 && row < _height;

            if (isInColumns && isInRows)
            {
                return new BoardHit(BoardHitKind.Cell, cell, default, default);
            }

            if (isInColumns && row == _height)
            {
                return new BoardHit(BoardHitKind.DoorSlot, cell, BoardSide.Top, column);
            }

            if (isInColumns && row == -1)
            {
                return new BoardHit(BoardHitKind.DoorSlot, cell, BoardSide.Bottom, column);
            }

            if (isInRows && column == -1)
            {
                return new BoardHit(BoardHitKind.DoorSlot, cell, BoardSide.Left, row);
            }

            if (isInRows && column == _width)
            {
                return new BoardHit(BoardHitKind.DoorSlot, cell, BoardSide.Right, row);
            }

            return new BoardHit(BoardHitKind.Nothing, cell, default, default);
        }

        private float RowTop(int row)
        {
            return _origin.y + (_height - 1 - row) * CellSize;
        }
    }
}
