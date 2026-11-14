using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Flow;

namespace ColorBlockJam.Tests
{
    /// <summary>A fixed number of copies of one small level, and a record of which were asked for.</summary>
    internal sealed class FakeLevelSource : ILevelSource
    {
        public const int RedBlock = 0;

        public FakeLevelSource(int count)
        {
            Count = count;
        }

        public int Count { get; }

        public List<int> LoadedIndices { get; } = new List<int>();

        public static Cell RedBlockStart => new Cell(1, 0);

        public LevelData Load(int index)
        {
            LoadedIndices.Add(index);
            return TestLevel.Create(3, 3)
                .WithBlock(BlockColor.Red, RedBlockStart)
                .WithDoor(BlockColor.Red, BoardSide.Top, 1, 1);
        }
    }
}
