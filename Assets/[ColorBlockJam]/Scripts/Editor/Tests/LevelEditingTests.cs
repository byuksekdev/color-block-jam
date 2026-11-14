using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Editing;
using ColorBlockJam.Core.Validation;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class LevelEditingTests
    {
        private static readonly Cell[] Bar = { new Cell(0, 0), new Cell(1, 0) };

        private static LevelData LevelWithRedBar()
        {
            return TestLevel.Create(4, 4)
                .WithBlock(BlockColor.Red, new Cell(1, 1), new Cell(2, 1))
                .WithDoor(BlockColor.Red, BoardSide.Top, 1, 2);
        }

        [Test]
        public void FindBlockAt_CellOfABlock_GivesItsIndex()
        {
            LevelData level = LevelWithRedBar();

            Assert.AreEqual(0, LevelEditing.FindBlockAt(level, new Cell(2, 1)));
            Assert.AreEqual(LevelEditing.NotFound, LevelEditing.FindBlockAt(level, new Cell(3, 1)));
        }

        [Test]
        public void CanPlaceBlock_OnFreeCells_IsTrue()
        {
            Assert.IsTrue(LevelEditing.CanPlaceBlock(LevelWithRedBar(), Bar, new Cell(0, 3)));
        }

        [Test]
        public void CanPlaceBlock_OnAnotherBlock_IsFalse()
        {
            Assert.IsFalse(LevelEditing.CanPlaceBlock(LevelWithRedBar(), Bar, new Cell(0, 1)));
        }

        [Test]
        public void CanPlaceBlock_PartlyOutsideTheBoard_IsFalse()
        {
            Assert.IsFalse(LevelEditing.CanPlaceBlock(LevelWithRedBar(), Bar, new Cell(3, 3)));
        }

        [Test]
        public void CanPlaceBlock_OnTheIgnoredBlock_IsTrue()
        {
            Assert.IsTrue(LevelEditing.CanPlaceBlock(LevelWithRedBar(), Bar, new Cell(0, 1), 0));
        }

        [Test]
        public void TryPlaceBlock_OnFreeCells_AddsTheBlockAtThatPlace()
        {
            LevelData level = LevelWithRedBar();

            bool placed = LevelEditing.TryPlaceBlock(level, BlockColor.Blue, Bar, new Cell(2, 3));

            Assert.IsTrue(placed);
            Assert.AreEqual(BlockColor.Blue, level.Blocks[1].Color);
            CollectionAssert.AreEqual(new[] { new Cell(2, 3), new Cell(3, 3) }, level.Blocks[1].Cells);
        }

        [Test]
        public void TryPlaceBlock_OnAnotherBlock_AddsNothing()
        {
            LevelData level = LevelWithRedBar();

            bool placed = LevelEditing.TryPlaceBlock(level, BlockColor.Blue, Bar, new Cell(2, 1));

            Assert.IsFalse(placed);
            Assert.AreEqual(1, level.Blocks.Count);
        }

        [Test]
        public void TryMoveBlock_OverItsOwnOldCells_MovesIt()
        {
            LevelData level = LevelWithRedBar();

            bool moved = LevelEditing.TryMoveBlock(level, 0, new Cell(1, 0));

            Assert.IsTrue(moved);
            CollectionAssert.AreEqual(new[] { new Cell(2, 1), new Cell(3, 1) }, level.Blocks[0].Cells);
        }

        [Test]
        public void TryMoveBlock_OffTheBoard_LeavesItInPlace()
        {
            LevelData level = LevelWithRedBar();

            bool moved = LevelEditing.TryMoveBlock(level, 0, new Cell(2, 0));

            Assert.IsFalse(moved);
            CollectionAssert.AreEqual(new[] { new Cell(1, 1), new Cell(2, 1) }, level.Blocks[0].Cells);
        }

        [Test]
        public void FindDoorAt_InsideAndBesideADoor_FindsOnlyTheCoveredCells()
        {
            LevelData level = LevelWithRedBar();

            Assert.AreEqual(0, LevelEditing.FindDoorAt(level, BoardSide.Top, 2));
            Assert.AreEqual(LevelEditing.NotFound, LevelEditing.FindDoorAt(level, BoardSide.Top, 3));
            Assert.AreEqual(LevelEditing.NotFound, LevelEditing.FindDoorAt(level, BoardSide.Bottom, 2));
        }

        [TestCase(0, 2)]
        [TestCase(2, 0)]
        public void PaintDoor_BetweenTwoPositions_CoversBothEnds(int from, int to)
        {
            LevelData level = TestLevel.Create(4, 4);

            LevelEditing.PaintDoor(level, BlockColor.Blue, BoardSide.Left, from, to);

            Assert.AreEqual(1, level.Doors.Count);
            Assert.AreEqual(BlockColor.Blue, level.Doors[0].Color);
            Assert.AreEqual(BoardSide.Left, level.Doors[0].Side);
            Assert.AreEqual(0, level.Doors[0].Start);
            Assert.AreEqual(3, level.Doors[0].Length);
        }

        [Test]
        public void PaintDoor_PastTheEndOfTheSide_StopsAtTheLastCell()
        {
            LevelData level = TestLevel.Create(4, 6);

            LevelEditing.PaintDoor(level, BlockColor.Blue, BoardSide.Bottom, 2, 9);

            Assert.AreEqual(2, level.Doors[0].Start);
            Assert.AreEqual(2, level.Doors[0].Length);
        }

        [Test]
        public void PaintDoor_OverAnotherDoorOnThatSide_ReplacesIt()
        {
            LevelData level = LevelWithRedBar();

            LevelEditing.PaintDoor(level, BlockColor.Blue, BoardSide.Top, 2, 3);

            Assert.AreEqual(1, level.Doors.Count);
            Assert.AreEqual(BlockColor.Blue, level.Doors[0].Color);
        }

        [Test]
        public void PaintDoor_OnAnotherSide_KeepsTheOtherDoor()
        {
            LevelData level = LevelWithRedBar();

            LevelEditing.PaintDoor(level, BlockColor.Blue, BoardSide.Bottom, 1, 2);

            Assert.AreEqual(2, level.Doors.Count);
        }

        [Test]
        public void Resize_Smaller_RemovesWhatNoLongerFits()
        {
            LevelData level = LevelWithRedBar()
                .WithBlock(BlockColor.Blue, new Cell(0, 0))
                .WithDoor(BlockColor.Blue, BoardSide.Bottom, 0, 4)
                .WithDoor(BlockColor.Blue, BoardSide.Right, 3, 1);

            LevelEditing.Resize(level, 2, 2);

            Assert.AreEqual(1, level.Blocks.Count);
            Assert.AreEqual(BlockColor.Blue, level.Blocks[0].Color);
            Assert.AreEqual(2, level.Doors.Count);
            Assert.AreEqual(1, level.Doors[0].Length);
            Assert.AreEqual(2, level.Doors[1].Length);
        }

        [Test]
        public void Resize_Smaller_LeavesAWellFormedBoard()
        {
            LevelData level = LevelWithRedBar()
                .WithBlock(BlockColor.Red, new Cell(0, 0))
                .WithDoor(BlockColor.Red, BoardSide.Right, 3, 1);

            LevelEditing.Resize(level, 3, 2);

            CollectionAssert.IsEmpty(LevelValidator.Validate(level));
        }
    }
}
