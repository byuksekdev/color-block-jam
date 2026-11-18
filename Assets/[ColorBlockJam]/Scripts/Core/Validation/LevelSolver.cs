using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Validation
{
    /// <summary>
    /// Finds out whether every block of a level can leave. A block that leaves never makes the rest
    /// harder: it frees cells and melts ice, and every move made to let it out can be taken back.
    /// So the solver lets out whichever block it can reach a door with first, and starts again from
    /// there; when no block can ever be let out, the level cannot be solved.
    /// </summary>
    public sealed class LevelSolver
    {
        public const int DefaultPositionLimit = 20000;

        private const int NoDoor = int.MaxValue;

        private static readonly Cell[] Steps = { new Cell(1, 0), new Cell(-1, 0), new Cell(0, 1), new Cell(0, -1) };

        private readonly LevelData _level;
        private readonly BoardState _board;
        private readonly Queue<Cell> _walk = new Queue<Cell>();
        private readonly HashSet<Cell> _walked = new HashSet<Cell>();
        private readonly int[] _doorIce;
        private int _positionsLeft;

        private LevelSolver(LevelData level, int positionLimit)
        {
            _level = level;
            _board = new BoardState(level);
            _positionsLeft = positionLimit;

            _doorIce = new int[level.Blocks.Count];
            for (int i = 0; i < _doorIce.Length; i++)
            {
                _doorIce[i] = LowestDoorIce(i);
            }
        }

        /// <summary>
        /// The level has to be well formed: check it with the level validator first. The limit is the
        /// number of board positions the solver may try before it answers Unknown.
        /// </summary>
        public static SolverResult Solve(LevelData level, int positionLimit = DefaultPositionLimit)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            return new LevelSolver(level, positionLimit).Solve();
        }

        private SolverResult Solve()
        {
            if (TryFindBlockWithoutDoor(out int blockIndex))
            {
                string noDoor = "Block " + (blockIndex + 1) + " can never leave: no door of its colour is wide enough for it and lies where it can move to.";
                return new SolverResult(Solvability.Unsolvable, noDoor, LevelPart.Block(blockIndex));
            }

            if (TryFindIceThatNeverMelts(out string reason))
            {
                return new SolverResult(Solvability.Unsolvable, reason);
            }

            while (_board.RemainingBlockCount > 0)
            {
                if (TryLetOneOut())
                {
                    continue;
                }

                if (_positionsLeft <= 0)
                {
                    return new SolverResult(Solvability.Unknown, string.Empty);
                }

                int left = _board.RemainingBlockCount;
                string stuck = left == _board.Blocks.Count
                    ? "No block can be brought to a door of its colour."
                    : "After " + (_board.Blocks.Count - left) + " blocks have left, none of the other " + left + " can be brought to its door.";
                return new SolverResult(Solvability.Unsolvable, stuck);
            }

            return new SolverResult(Solvability.Solvable, string.Empty);
        }

        /// <summary>A block that reaches no door of its colour on an otherwise empty board never leaves.</summary>
        private bool TryFindBlockWithoutDoor(out int blockIndex)
        {
            for (blockIndex = 0; blockIndex < _level.Blocks.Count; blockIndex++)
            {
                if (_doorIce[blockIndex] == NoDoor)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Each block waits for a number of others: for its own ice, and for the ice on the first door
        /// it could use. In the order of that number, the block in place n has n blocks before it.
        /// </summary>
        private bool TryFindIceThatNeverMelts(out string reason)
        {
            var waits = new List<int>(_level.Blocks.Count);
            for (int i = 0; i < _level.Blocks.Count; i++)
            {
                waits.Add(Math.Max(_level.Blocks[i].IceCount, _doorIce[i]));
            }

            waits.Sort();
            for (int i = 0; i < waits.Count; i++)
            {
                if (waits[i] > i)
                {
                    reason = "The ice can never melt: after " + i + " blocks have left, every block that remains waits for more blocks to leave, on itself or on its door.";
                    return true;
                }
            }

            reason = string.Empty;
            return false;
        }

        /// <summary>The lowest ice count among the doors this block reaches when it is alone on the board. NoDoor when it reaches none.</summary>
        private int LowestDoorIce(int blockIndex)
        {
            BlockData block = _level.Blocks[blockIndex];
            int lowest = NoDoor;

            for (int i = 0; i < _level.Doors.Count; i++)
            {
                DoorData door = _level.Doors[i];
                if (door.Color != block.Color || door.IceCount >= lowest)
                {
                    continue;
                }

                var alone = new LevelData { Width = _level.Width, Height = _level.Height, TimeLimitSeconds = _level.TimeLimitSeconds };
                alone.Blocks.Add(new BlockData { Color = block.Color, Cells = block.Cells, Axis = block.Axis });
                alone.Doors.Add(new DoorData { Color = door.Color, Side = door.Side, Start = door.Start, Length = door.Length });

                var board = new BoardState(alone);
                if (TryWalkOut(board, 0))
                {
                    lowest = door.IceCount;
                }
            }

            return lowest;
        }

        /// <summary>
        /// Tries the positions the blocks can be moved into, nearest first, until in one of them a
        /// block can be walked to its door. That block leaves, and the others stay where they were moved.
        /// </summary>
        private bool TryLetOneOut()
        {
            var seen = new HashSet<string>();
            var positions = new Queue<Cell[]>();
            var key = new char[_board.Blocks.Count];

            Cell[] start = CurrentOrigins();
            seen.Add(KeyOf(start, key));
            positions.Enqueue(start);

            while (positions.Count > 0 && _positionsLeft > 0)
            {
                _positionsLeft--;
                Cell[] origins = positions.Dequeue();
                _board.SetOrigins(origins);

                for (int id = 0; id < origins.Length; id++)
                {
                    if (CanMove(id) && TryWalkOut(_board, id))
                    {
                        return true;
                    }
                }

                for (int id = 0; id < origins.Length; id++)
                {
                    if (!CanMove(id))
                    {
                        continue;
                    }

                    for (int s = 0; s < Steps.Length; s++)
                    {
                        Cell origin = origins[id] + Steps[s];
                        if (!Allows(_board.Blocks[id].Axis, Steps[s]) || !_board.CanPlace(id, origin))
                        {
                            continue;
                        }

                        var next = (Cell[])origins.Clone();
                        next[id] = origin;
                        if (seen.Add(KeyOf(next, key)))
                        {
                            positions.Enqueue(next);
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Moves one block, with every other block standing still, over the cells it can reach until
        /// it can leave, and takes it off the board. A block that cannot leave is put back.
        /// </summary>
        private bool TryWalkOut(BoardState board, int blockId)
        {
            Cell start = board.GetOrigin(blockId);
            BlockAxis axis = board.Blocks[blockId].Axis;

            _walk.Clear();
            _walked.Clear();
            _walk.Enqueue(start);
            _walked.Add(start);

            while (_walk.Count > 0)
            {
                Cell origin = _walk.Dequeue();
                board.MoveBlock(blockId, origin);
                if (ExitRule.TryFindExit(board, blockId, out _))
                {
                    board.RemoveBlock(blockId);
                    return true;
                }

                for (int s = 0; s < Steps.Length; s++)
                {
                    Cell next = origin + Steps[s];
                    if (Allows(axis, Steps[s]) && board.CanPlace(blockId, next) && _walked.Add(next))
                    {
                        _walk.Enqueue(next);
                    }
                }
            }

            board.MoveBlock(blockId, start);
            return false;
        }

        private bool CanMove(int blockId)
        {
            return _board.IsOnBoard(blockId) && _board.IceLeft(_board.Blocks[blockId]) == 0;
        }

        private static bool Allows(BlockAxis axis, Cell step)
        {
            return step.X != 0 ? axis.AllowsHorizontal() : axis.AllowsVertical();
        }

        private Cell[] CurrentOrigins()
        {
            var origins = new Cell[_board.Blocks.Count];
            for (int i = 0; i < origins.Length; i++)
            {
                origins[i] = _board.GetOrigin(i);
            }

            return origins;
        }

        /// <summary>One character for each block: the number of the cell its origin stands on. Blocks that have left keep their last cell, which is the same in every position of one search.</summary>
        private string KeyOf(Cell[] origins, char[] buffer)
        {
            for (int i = 0; i < origins.Length; i++)
            {
                buffer[i] = (char)(origins[i].Y * _board.Width + origins[i].X);
            }

            return new string(buffer);
        }
    }
}
