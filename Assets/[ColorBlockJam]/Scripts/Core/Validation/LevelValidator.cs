using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Validation
{
    /// <summary>Checks that a level is well formed. It does not check that the level can be solved.</summary>
    public static class LevelValidator
    {
        public const int MinBoardSize = 1;

        public static List<LevelProblem> Validate(LevelData level)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            var problems = new List<LevelProblem>();
            CheckSettings(level, problems);
            CheckBlocks(level, problems);
            CheckDoors(level, problems);
            CheckEveryBlockColorHasDoor(level, problems);
            return problems;
        }

        private static void CheckSettings(LevelData level, List<LevelProblem> problems)
        {
            if (level.Version != LevelData.CurrentVersion)
            {
                problems.Add(new LevelProblem(
                    LevelProblemKind.UnsupportedVersion,
                    "The level uses format version " + level.Version + ", but this build reads version " + LevelData.CurrentVersion + "."));
            }

            if (level.Width < MinBoardSize || level.Height < MinBoardSize)
            {
                problems.Add(new LevelProblem(
                    LevelProblemKind.BoardTooSmall,
                    "The board needs a width and a height of at least " + MinBoardSize + "."));
            }

            if (level.TimeLimitSeconds <= 0)
            {
                problems.Add(new LevelProblem(
                    LevelProblemKind.TimeLimitNotPositive,
                    "The timer needs a value above zero."));
            }
        }

        private static void CheckBlocks(LevelData level, List<LevelProblem> problems)
        {
            if (level.Blocks.Count == 0)
            {
                problems.Add(new LevelProblem(LevelProblemKind.NoBlocks, "The level has no block."));
                return;
            }

            var takenCells = new HashSet<Cell>();
            for (int i = 0; i < level.Blocks.Count; i++)
            {
                BlockData block = level.Blocks[i];
                string name = "Block " + (i + 1);

                if (block.Color == BlockColor.None)
                {
                    problems.Add(new LevelProblem(LevelProblemKind.BlockHasNoColor, name + " has no colour."));
                }

                if (block.Cells.Count == 0)
                {
                    problems.Add(new LevelProblem(LevelProblemKind.BlockHasNoCells, name + " has no cell."));
                    continue;
                }

                CheckBlockCells(level, block, name, takenCells, problems);
            }
        }

        private static void CheckBlockCells(
            LevelData level,
            BlockData block,
            string name,
            HashSet<Cell> takenCells,
            List<LevelProblem> problems)
        {
            bool isOutside = false;
            bool overlaps = false;

            for (int i = 0; i < block.Cells.Count; i++)
            {
                Cell cell = block.Cells[i];
                if (cell.X < 0 || cell.X >= level.Width || cell.Y < 0 || cell.Y >= level.Height)
                {
                    isOutside = true;
                }
                else if (!takenCells.Add(cell))
                {
                    overlaps = true;
                }
            }

            if (isOutside)
            {
                problems.Add(new LevelProblem(LevelProblemKind.BlockOutsideBoard, name + " lies partly outside the board."));
            }

            if (overlaps)
            {
                problems.Add(new LevelProblem(LevelProblemKind.BlocksOverlap, name + " covers a cell that is already taken."));
            }
        }

        private static void CheckDoors(LevelData level, List<LevelProblem> problems)
        {
            for (int i = 0; i < level.Doors.Count; i++)
            {
                DoorData door = level.Doors[i];
                string name = "Door " + (i + 1);

                if (door.Color == BlockColor.None)
                {
                    problems.Add(new LevelProblem(LevelProblemKind.DoorHasNoColor, name + " has no colour."));
                }

                int sideLength = door.Side.RunsAlongX() ? level.Width : level.Height;
                if (door.Length < 1 || door.Start < 0 || door.Start + door.Length > sideLength)
                {
                    problems.Add(new LevelProblem(LevelProblemKind.DoorOutsideSide, name + " does not fit on its side of the board."));
                    continue;
                }

                if (OverlapsEarlierDoor(level, i))
                {
                    problems.Add(new LevelProblem(LevelProblemKind.DoorsOverlap, name + " overlaps another door on the same side."));
                }
            }
        }

        private static bool OverlapsEarlierDoor(LevelData level, int doorIndex)
        {
            DoorData door = level.Doors[doorIndex];
            for (int i = 0; i < doorIndex; i++)
            {
                DoorData other = level.Doors[i];
                bool sharesCells = door.Start < other.Start + other.Length && other.Start < door.Start + door.Length;
                if (other.Side == door.Side && sharesCells)
                {
                    return true;
                }
            }

            return false;
        }

        private static void CheckEveryBlockColorHasDoor(LevelData level, List<LevelProblem> problems)
        {
            var doorColors = new HashSet<BlockColor>();
            for (int i = 0; i < level.Doors.Count; i++)
            {
                doorColors.Add(level.Doors[i].Color);
            }

            var reportedColors = new HashSet<BlockColor>();
            for (int i = 0; i < level.Blocks.Count; i++)
            {
                BlockColor color = level.Blocks[i].Color;
                if (color == BlockColor.None || doorColors.Contains(color) || !reportedColors.Add(color))
                {
                    continue;
                }

                problems.Add(new LevelProblem(
                    LevelProblemKind.BlockColorWithoutDoor,
                    "No door has the colour " + color + " of block " + (i + 1) + "."));
            }
        }
    }
}
