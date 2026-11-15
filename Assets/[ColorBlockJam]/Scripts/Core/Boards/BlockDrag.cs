using System;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Boards
{
    /// <summary>
    /// Moves one block smoothly across the board. The block stops at the board edge and may sink a
    /// little into other blocks, so that it does not have to be lined up exactly to pass between them.
    /// The board is updated each time the block enters a new cell.
    /// </summary>
    public sealed class BlockDrag
    {
        private const float MaxStep = 0.5f;

        /// <summary>Keeps a block that rests at the full overlap from counting as one cell further.</summary>
        private const float Epsilon = 0.0001f;

        private readonly BoardState _board;
        private readonly Block _block;
        private readonly float _overlap;

        public BlockDrag(BoardState board, int blockId, float blockOverlap)
        {
            _board = board;
            _block = board.Blocks[blockId];
            _overlap = blockOverlap;
            Position = BoardPoint.FromCell(board.GetOrigin(blockId));
        }

        public int BlockId => _block.Id;

        /// <summary>Where the block's origin is now, between cells.</summary>
        public BoardPoint Position { get; private set; }

        /// <summary>
        /// Moves the block towards the target until it arrives, is blocked or enters a new cell.
        /// Returns true when it entered a new cell; call again to continue the same move.
        /// </summary>
        public bool MoveTowards(BoardPoint target)
        {
            float targetX = Math.Clamp(target.X, 0f, _board.Width - _block.Width);
            float targetY = Math.Clamp(target.Y, 0f, _board.Height - _block.Height);
            Cell origin = _board.GetOrigin(BlockId);

            while (true)
            {
                float x = SlideX(StepTowards(Position.X, targetX), Position.Y);
                float y = SlideY(x, StepTowards(Position.Y, targetY));
                if (x == Position.X && y == Position.Y)
                {
                    return false;
                }

                Position = new BoardPoint(x, y);
                Cell cell = Position.NearestCell();
                if (cell != origin)
                {
                    _board.MoveBlock(BlockId, cell);
                    return true;
                }
            }
        }

        /// <summary>True when the target lies beyond the board edge on this side, so the block is pressed against it.</summary>
        public bool IsPushedAgainst(BoardSide side, BoardPoint target)
        {
            return DistanceFrom(side, target) < 0f;
        }

        /// <summary>
        /// How far the block is from lying against this side of the board when its origin is at this point.
        /// The distance is negative when the point lies beyond the side.
        /// </summary>
        public float DistanceFrom(BoardSide side, BoardPoint origin)
        {
            switch (side)
            {
                case BoardSide.Left:
                    return origin.X;
                case BoardSide.Right:
                    return _board.Width - _block.Width - origin.X;
                case BoardSide.Bottom:
                    return origin.Y;
                default:
                    return _board.Height - _block.Height - origin.Y;
            }
        }

        private static float StepTowards(float current, float target)
        {
            float delta = target - current;
            return MathF.Abs(delta) <= MaxStep ? target : current + MathF.Sign(delta) * MaxStep;
        }

        private float SlideX(float desiredX, float y)
        {
            float currentX = Position.X;
            if (desiredX > currentX)
            {
                int column = LastCell(currentX);
                while (column < desiredX && CanOccupyColumn(column + 1, y))
                {
                    column++;
                }

                return MathF.Min(desiredX, column + _overlap);
            }

            if (desiredX < currentX)
            {
                int column = FirstCell(currentX);
                while (column > desiredX && CanOccupyColumn(column - 1, y))
                {
                    column--;
                }

                return MathF.Max(desiredX, column - _overlap);
            }

            return currentX;
        }

        private float SlideY(float x, float desiredY)
        {
            float currentY = Position.Y;
            if (desiredY > currentY)
            {
                int row = LastCell(currentY);
                while (row < desiredY && CanOccupyRow(x, row + 1))
                {
                    row++;
                }

                return MathF.Min(desiredY, row + _overlap);
            }

            if (desiredY < currentY)
            {
                int row = FirstCell(currentY);
                while (row > desiredY && CanOccupyRow(x, row - 1))
                {
                    row--;
                }

                return MathF.Max(desiredY, row - _overlap);
            }

            return currentY;
        }

        private bool CanOccupyColumn(int column, float y)
        {
            int lowRow = FirstCell(y);
            int highRow = LastCell(y);
            return _board.CanPlace(BlockId, new Cell(column, lowRow))
                && (highRow == lowRow || _board.CanPlace(BlockId, new Cell(column, highRow)));
        }

        private bool CanOccupyRow(float x, int row)
        {
            int lowColumn = FirstCell(x);
            int highColumn = LastCell(x);
            return _board.CanPlace(BlockId, new Cell(lowColumn, row))
                && (highColumn == lowColumn || _board.CanPlace(BlockId, new Cell(highColumn, row)));
        }

        /// <summary>The lowest cell the block stands on along one axis. A cell it only overlaps within the allowance does not count.</summary>
        private int FirstCell(float coordinate)
        {
            return (int)MathF.Floor(coordinate + _overlap + Epsilon);
        }

        /// <summary>The highest cell the block stands on along one axis. A cell it only overlaps within the allowance does not count.</summary>
        private int LastCell(float coordinate)
        {
            return (int)MathF.Ceiling(coordinate - _overlap - Epsilon);
        }
    }
}
