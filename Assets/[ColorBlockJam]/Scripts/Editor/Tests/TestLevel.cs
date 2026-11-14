using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Tests
{
    internal static class TestLevel
    {
        public const int TimeLimitSeconds = 10;

        public static LevelData Create(int width, int height)
        {
            return new LevelData { Width = width, Height = height, TimeLimitSeconds = TimeLimitSeconds };
        }

        public static LevelData WithBlock(this LevelData level, BlockColor color, params Cell[] cells)
        {
            var block = new BlockData { Color = color };
            block.Cells.AddRange(cells);
            level.Blocks.Add(block);
            return level;
        }

        public static LevelData WithDoor(this LevelData level, BlockColor color, BoardSide side, int start, int length)
        {
            level.Doors.Add(new DoorData { Color = color, Side = side, Start = start, Length = length });
            return level;
        }
    }
}
