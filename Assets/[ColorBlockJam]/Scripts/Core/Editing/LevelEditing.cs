using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Editing
{
    /// <summary>The changes a level editor makes to a level. Each one leaves the level well formed.</summary>
    public static class LevelEditing
    {
        public const int NotFound = -1;

        public static int SideLength(LevelData level, BoardSide side)
        {
            return side.RunsAlongX() ? level.Width : level.Height;
        }

        public static int FindBlockAt(LevelData level, Cell cell)
        {
            for (int i = 0; i < level.Blocks.Count; i++)
            {
                if (level.Blocks[i].Cells.Contains(cell))
                {
                    return i;
                }
            }

            return NotFound;
        }

        /// <summary>True when the shape fits at this origin. The cells of ignoredBlock count as free, for a block that is being moved.</summary>
        public static bool CanPlaceBlock(LevelData level, IReadOnlyList<Cell> shape, Cell origin, int ignoredBlock = NotFound)
        {
            for (int i = 0; i < shape.Count; i++)
            {
                Cell cell = origin + shape[i];
                if (cell.X < 0 || cell.X >= level.Width || cell.Y < 0 || cell.Y >= level.Height)
                {
                    return false;
                }

                int occupant = FindBlockAt(level, cell);
                if (occupant != NotFound && occupant != ignoredBlock)
                {
                    return false;
                }
            }

            return shape.Count > 0;
        }

        public static bool TryPlaceBlock(LevelData level, BlockColor color, IReadOnlyList<Cell> shape, Cell origin)
        {
            if (!CanPlaceBlock(level, shape, origin))
            {
                return false;
            }

            var block = new BlockData { Color = color };
            for (int i = 0; i < shape.Count; i++)
            {
                block.Cells.Add(origin + shape[i]);
            }

            level.Blocks.Add(block);
            return true;
        }

        public static bool TryMoveBlock(LevelData level, int blockIndex, Cell offset)
        {
            List<Cell> cells = level.Blocks[blockIndex].Cells;
            if (!CanPlaceBlock(level, cells, offset, blockIndex))
            {
                return false;
            }

            for (int i = 0; i < cells.Count; i++)
            {
                cells[i] += offset;
            }

            return true;
        }

        public static int FindDoorAt(LevelData level, BoardSide side, int position)
        {
            for (int i = 0; i < level.Doors.Count; i++)
            {
                DoorData door = level.Doors[i];
                if (door.Side == side && position >= door.Start && position < door.Start + door.Length)
                {
                    return i;
                }
            }

            return NotFound;
        }

        /// <summary>Puts a door over the cells between the two positions. Doors it would overlap on that side are removed.</summary>
        public static void PaintDoor(LevelData level, BlockColor color, BoardSide side, int from, int to)
        {
            int lastPosition = SideLength(level, side) - 1;
            if (lastPosition < 0)
            {
                return;
            }

            int start = Math.Clamp(Math.Min(from, to), 0, lastPosition);
            int end = Math.Clamp(Math.Max(from, to), 0, lastPosition);

            for (int position = start; position <= end; position++)
            {
                int overlapped = FindDoorAt(level, side, position);
                if (overlapped != NotFound)
                {
                    level.Doors.RemoveAt(overlapped);
                }
            }

            level.Doors.Add(new DoorData { Color = color, Side = side, Start = start, Length = end - start + 1 });
        }

        /// <summary>Changes the board size. Blocks that no longer fit are removed, and doors are shortened or removed.</summary>
        public static void Resize(LevelData level, int width, int height)
        {
            level.Width = width;
            level.Height = height;

            for (int i = level.Blocks.Count - 1; i >= 0; i--)
            {
                if (!CanPlaceBlock(level, level.Blocks[i].Cells, new Cell(0, 0), i))
                {
                    level.Blocks.RemoveAt(i);
                }
            }

            for (int i = level.Doors.Count - 1; i >= 0; i--)
            {
                DoorData door = level.Doors[i];
                int sideLength = SideLength(level, door.Side);
                if (door.Start >= sideLength)
                {
                    level.Doors.RemoveAt(i);
                }
                else
                {
                    door.Length = Math.Min(door.Length, sideLength - door.Start);
                }
            }
        }
    }
}
