using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Boards
{
    /// <summary>Where every block is. Built from a valid level; it keeps no reference to the level data.</summary>
    public sealed class BoardState : IReadOnlyBoard
    {
        private const int NoBlock = -1;

        private readonly int[] _occupants;
        private readonly Block[] _blocks;
        private readonly Cell[] _origins;
        private readonly bool[] _isOnBoard;
        private readonly Door[] _doors;

        public BoardState(LevelData level)
        {
            Width = level.Width;
            Height = level.Height;

            _occupants = new int[Width * Height];
            Array.Fill(_occupants, NoBlock);

            _blocks = new Block[level.Blocks.Count];
            _origins = new Cell[_blocks.Length];
            _isOnBoard = new bool[_blocks.Length];
            for (int i = 0; i < _blocks.Length; i++)
            {
                BlockData data = level.Blocks[i];
                Cell origin = LowerLeftCorner(data.Cells);
                var shape = new Cell[data.Cells.Count];
                for (int j = 0; j < shape.Length; j++)
                {
                    shape[j] = data.Cells[j] - origin;
                }

                _blocks[i] = new Block(i, data.Color, shape);
                _origins[i] = origin;
                _isOnBoard[i] = true;
                SetOccupant(i, origin, i);
            }

            _doors = new Door[level.Doors.Count];
            for (int i = 0; i < _doors.Length; i++)
            {
                DoorData data = level.Doors[i];
                _doors[i] = new Door(data.Color, data.Side, data.Start, data.Length);
            }

            RemainingBlockCount = _blocks.Length;
        }

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<Block> Blocks => _blocks;
        public IReadOnlyList<Door> Doors => _doors;
        public int RemainingBlockCount { get; private set; }

        public bool Contains(Cell cell)
        {
            return cell.X >= 0 && cell.X < Width && cell.Y >= 0 && cell.Y < Height;
        }

        public bool IsOnBoard(int blockId)
        {
            return _isOnBoard[blockId];
        }

        public Cell GetOrigin(int blockId)
        {
            return _origins[blockId];
        }

        public bool TryGetBlockAt(Cell cell, out int blockId)
        {
            blockId = Contains(cell) ? _occupants[IndexOf(cell)] : NoBlock;
            return blockId != NoBlock;
        }

        /// <summary>True when the block fits at this origin: inside the board and on no cell of another block.</summary>
        public bool CanPlace(int blockId, Cell origin)
        {
            IReadOnlyList<Cell> shape = _blocks[blockId].Shape;
            for (int i = 0; i < shape.Count; i++)
            {
                Cell cell = origin + shape[i];
                if (!Contains(cell))
                {
                    return false;
                }

                int occupant = _occupants[IndexOf(cell)];
                if (occupant != NoBlock && occupant != blockId)
                {
                    return false;
                }
            }

            return true;
        }

        public void MoveBlock(int blockId, Cell origin)
        {
            if (!_isOnBoard[blockId])
            {
                throw new InvalidOperationException("Block " + blockId + " has left the board.");
            }

            if (!CanPlace(blockId, origin))
            {
                throw new InvalidOperationException("Block " + blockId + " does not fit at " + origin + ".");
            }

            SetOccupant(blockId, _origins[blockId], NoBlock);
            _origins[blockId] = origin;
            SetOccupant(blockId, origin, blockId);
        }

        public void RemoveBlock(int blockId)
        {
            if (!_isOnBoard[blockId])
            {
                return;
            }

            SetOccupant(blockId, _origins[blockId], NoBlock);
            _isOnBoard[blockId] = false;
            RemainingBlockCount--;
        }

        private static Cell LowerLeftCorner(List<Cell> cells)
        {
            Cell corner = cells[0];
            for (int i = 1; i < cells.Count; i++)
            {
                corner = new Cell(Math.Min(corner.X, cells[i].X), Math.Min(corner.Y, cells[i].Y));
            }

            return corner;
        }

        private void SetOccupant(int blockId, Cell origin, int occupant)
        {
            IReadOnlyList<Cell> shape = _blocks[blockId].Shape;
            for (int i = 0; i < shape.Count; i++)
            {
                _occupants[IndexOf(origin + shape[i])] = occupant;
            }
        }

        private int IndexOf(Cell cell)
        {
            return cell.Y * Width + cell.X;
        }
    }
}
