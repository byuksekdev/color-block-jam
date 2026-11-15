using System.Collections.Generic;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Boards
{
    /// <summary>
    /// A block can leave through a door of its colour when it touches the door's side of the board,
    /// every cell of it lies inside the door's span, and no other block stands between it and the door.
    /// Ice on the block or on the door keeps it in, and an arrow block leaves only along its arrow.
    /// </summary>
    public static class ExitRule
    {
        public static bool TryFindExit(IReadOnlyBoard board, int blockId, out Door door)
        {
            Block block = board.Blocks[blockId];
            IReadOnlyList<Door> doors = board.Doors;

            for (int i = 0; i < doors.Count; i++)
            {
                if (doors[i].Color == block.Color && CanLeaveThrough(board, block, doors[i]))
                {
                    door = doors[i];
                    return true;
                }
            }

            door = null;
            return false;
        }

        private static bool CanLeaveThrough(IReadOnlyBoard board, Block block, Door door)
        {
            if (board.IceLeft(block) > 0 || board.IceLeft(door) > 0 || !block.Axis.AllowsLeavingThrough(door.Side))
            {
                return false;
            }

            Cell origin = board.GetOrigin(block.Id);
            Cell step = door.Side.OutwardStep();
            bool touchesSide = false;

            for (int i = 0; i < block.Shape.Count; i++)
            {
                Cell cell = origin + block.Shape[i];
                int positionAlongSide = door.Side.RunsAlongX() ? cell.X : cell.Y;
                if (!door.Spans(positionAlongSide))
                {
                    return false;
                }

                Cell next = cell + step;
                if (!board.Contains(next))
                {
                    touchesSide = true;
                }

                while (board.Contains(next))
                {
                    if (board.TryGetBlockAt(next, out int occupant) && occupant != block.Id)
                    {
                        return false;
                    }

                    next += step;
                }
            }

            return touchesSide;
        }
    }
}
