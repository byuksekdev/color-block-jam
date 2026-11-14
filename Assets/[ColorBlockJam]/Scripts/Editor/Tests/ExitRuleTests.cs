using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class ExitRuleTests
    {
        private const int RedBlock = 0;

        private static bool CanExit(LevelData level)
        {
            return ExitRule.TryFindExit(new BoardState(level), RedBlock, out _);
        }

        [Test]
        public void TryFindExit_BlockAtItsDoor_FindsTheDoor()
        {
            LevelData level = TestLevel.Create(4, 4)
                .WithBlock(BlockColor.Red, new Cell(1, 2), new Cell(1, 3))
                .WithDoor(BlockColor.Red, BoardSide.Top, 1, 1);

            bool found = ExitRule.TryFindExit(new BoardState(level), RedBlock, out Door door);

            Assert.IsTrue(found);
            Assert.AreEqual(BoardSide.Top, door.Side);
        }

        [Test]
        public void TryFindExit_DoorOfAnotherColour_FindsNothing()
        {
            LevelData level = TestLevel.Create(4, 4)
                .WithBlock(BlockColor.Red, new Cell(1, 2), new Cell(1, 3))
                .WithDoor(BlockColor.Blue, BoardSide.Top, 1, 1);

            Assert.IsFalse(CanExit(level));
        }

        [Test]
        public void TryFindExit_BlockNotTouchingTheSide_FindsNothing()
        {
            LevelData level = TestLevel.Create(4, 4)
                .WithBlock(BlockColor.Red, new Cell(1, 1), new Cell(1, 2))
                .WithDoor(BlockColor.Red, BoardSide.Top, 1, 1);

            Assert.IsFalse(CanExit(level));
        }

        [Test]
        public void TryFindExit_BlockWiderThanTheDoor_FindsNothing()
        {
            LevelData level = TestLevel.Create(4, 4)
                .WithBlock(BlockColor.Red, new Cell(1, 3), new Cell(2, 3))
                .WithDoor(BlockColor.Red, BoardSide.Top, 1, 1);

            Assert.IsFalse(CanExit(level));
        }

        [Test]
        public void TryFindExit_DoorWiderThanTheBlock_FindsTheDoor()
        {
            LevelData level = TestLevel.Create(4, 4)
                .WithBlock(BlockColor.Red, new Cell(1, 3), new Cell(2, 3))
                .WithDoor(BlockColor.Red, BoardSide.Top, 0, 4);

            Assert.IsTrue(CanExit(level));
        }

        [Test]
        public void TryFindExit_AnotherBlockInTheWay_FindsNothing()
        {
            LevelData level = TestLevel.Create(4, 4)
                .WithBlock(BlockColor.Red, new Cell(1, 3), new Cell(1, 2), new Cell(2, 2))
                .WithBlock(BlockColor.Blue, new Cell(2, 3))
                .WithDoor(BlockColor.Red, BoardSide.Top, 1, 2);

            Assert.IsFalse(CanExit(level));
        }

        [Test]
        public void TryFindExit_OnlyItsOwnCellsInTheWay_FindsTheDoor()
        {
            LevelData level = TestLevel.Create(4, 4)
                .WithBlock(BlockColor.Red, new Cell(1, 3), new Cell(1, 2), new Cell(2, 2))
                .WithDoor(BlockColor.Red, BoardSide.Top, 1, 2);

            Assert.IsTrue(CanExit(level));
        }

        [TestCase(BoardSide.Left, 0, 1)]
        [TestCase(BoardSide.Right, 3, 1)]
        [TestCase(BoardSide.Bottom, 1, 0)]
        [TestCase(BoardSide.Top, 1, 3)]
        public void TryFindExit_OnEachSide_UsesThatSideOfTheBoard(BoardSide side, int blockX, int blockY)
        {
            LevelData level = TestLevel.Create(4, 4)
                .WithBlock(BlockColor.Red, new Cell(blockX, blockY))
                .WithDoor(BlockColor.Red, side, 1, 1);

            Assert.IsTrue(CanExit(level));
        }

        [Test]
        public void TryFindExit_LeftDoor_MeasuresItsSpanInRows()
        {
            LevelData level = TestLevel.Create(4, 4)
                .WithBlock(BlockColor.Red, new Cell(0, 2))
                .WithDoor(BlockColor.Red, BoardSide.Left, 1, 1);

            Assert.IsFalse(CanExit(level));
        }
    }
}
