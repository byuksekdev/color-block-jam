using System;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Boards
{
    /// <summary>
    /// Moves one block smoothly across the board. The block stops against the board edge and against
    /// other blocks, and the board is updated each time the block enters a new cell.
    /// </summary>
    public sealed class BlockDrag
    {
        private const float MaxStep = 0.5f;

        private readonly BoardState _board;
        private readonly Block _block;

        public BlockDrag(BoardState board, int blockId)
        {
            _board = board;
            _block = board.Blocks[blockId];
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
            switch (side)
            {
                case BoardSide.Left:
                    return target.X < 0f;
                case BoardSide.Right:
                    return target.X > _board.Width - _block.Width;
                case BoardSide.Bottom:
                    return target.Y < 0f;
                default:
                    return target.Y > _board.Height - _block.Height;
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
                int column = (int)MathF.Ceiling(currentX);
                while (column < desiredX && CanOccupyColumn(column + 1, y))
                {
                    column++;
                }

                return MathF.Min(desiredX, column);
            }

            if (desiredX < currentX)
            {
                int column = (int)MathF.Floor(currentX);
                while (column > desiredX && CanOccupyColumn(column - 1, y))
                {
                    column--;
                }

                return MathF.Max(desiredX, column);
            }

            return currentX;
        }

        private float SlideY(float x, float desiredY)
        {
            float currentY = Position.Y;
            if (desiredY > currentY)
            {
                int row = (int)MathF.Ceiling(currentY);
                while (row < desiredY && CanOccupyRow(x, row + 1))
                {
                    row++;
                }

                return MathF.Min(desiredY, row);
            }

            if (desiredY < currentY)
            {
                int row = (int)MathF.Floor(currentY);
                while (row > desiredY && CanOccupyRow(x, row - 1))
                {
                    row--;
                }

                return MathF.Max(desiredY, row);
            }

            return currentY;
        }

        private bool CanOccupyColumn(int column, float y)
        {
            int lowRow = (int)MathF.Floor(y);
            int highRow = (int)MathF.Ceiling(y);
            return _board.CanPlace(BlockId, new Cell(column, lowRow))
                && (highRow == lowRow || _board.CanPlace(BlockId, new Cell(column, highRow)));
        }

        private bool CanOccupyRow(float x, int row)
        {
            int lowColumn = (int)MathF.Floor(x);
            int highColumn = (int)MathF.Ceiling(x);
            return _board.CanPlace(BlockId, new Cell(lowColumn, row))
                && (highColumn == lowColumn || _board.CanPlace(BlockId, new Cell(highColumn, row)));
        }
    }
}
