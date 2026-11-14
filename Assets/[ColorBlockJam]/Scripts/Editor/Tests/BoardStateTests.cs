using System;
using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class BoardStateTests
    {
        private const int BlueBlock = 0;
        private const int RedBlock = 1;

        private static LevelData TwoBlockLevel()
        {
            return TestLevel.Create(4, 4)
                .WithBlock(BlockColor.Blue, new Cell(0, 0), new Cell(1, 0))
                .WithBlock(BlockColor.Red, new Cell(3, 0));
        }

        [Test]
        public void Constructor_Blocks_OccupyTheirCells()
        {
            var board = new BoardState(TwoBlockLevel());

            Assert.IsTrue(board.TryGetBlockAt(new Cell(1, 0), out int blockId));
            Assert.AreEqual(BlueBlock, blockId);
            Assert.IsFalse(board.TryGetBlockAt(new Cell(2, 0), out _));
            Assert.AreEqual(2, board.RemainingBlockCount);
        }

        [Test]
        public void Constructor_BlockShape_IsRelativeToItsLowerLeftCorner()
        {
            LevelData level = TestLevel.Create(4, 4)
                .WithBlock(BlockColor.Blue, new Cell(2, 1), new Cell(1, 1), new Cell(1, 2));

            var board = new BoardState(level);

            Assert.AreEqual(new Cell(1, 1), board.GetOrigin(0));
            Assert.AreEqual(2, board.Blocks[0].Width);
            Assert.AreEqual(2, board.Blocks[0].Height);
            CollectionAssert.AreEquivalent(
                new[] { new Cell(1, 0), new Cell(0, 0), new Cell(0, 1) },
                board.Blocks[0].Shape);
        }

        [Test]
        public void TryGetBlockAt_OutsideTheBoard_IsFalse()
        {
            var board = new BoardState(TwoBlockLevel());

            Assert.IsFalse(board.TryGetBlockAt(new Cell(-1, 0), out _));
        }

        [Test]
        public void CanPlace_OnFreeCells_IsTrue()
        {
            var board = new BoardState(TwoBlockLevel());

            Assert.IsTrue(board.CanPlace(BlueBlock, new Cell(0, 2)));
        }

        [Test]
        public void CanPlace_OnItsOwnCells_IsTrue()
        {
            var board = new BoardState(TwoBlockLevel());

            Assert.IsTrue(board.CanPlace(BlueBlock, new Cell(1, 0)));
        }

        [Test]
        public void CanPlace_OnAnotherBlock_IsFalse()
        {
            var board = new BoardState(TwoBlockLevel());

            Assert.IsFalse(board.CanPlace(BlueBlock, new Cell(2, 0)));
        }

        [Test]
        public void CanPlace_PartlyOutsideTheBoard_IsFalse()
        {
            var board = new BoardState(TwoBlockLevel());

            Assert.IsFalse(board.CanPlace(BlueBlock, new Cell(3, 2)));
        }

        [Test]
        public void MoveBlock_ToFreeCells_MovesItsCells()
        {
            var board = new BoardState(TwoBlockLevel());

            board.MoveBlock(BlueBlock, new Cell(0, 2));

            Assert.AreEqual(new Cell(0, 2), board.GetOrigin(BlueBlock));
            Assert.IsFalse(board.TryGetBlockAt(new Cell(0, 0), out _));
            Assert.IsTrue(board.TryGetBlockAt(new Cell(1, 2), out int blockId));
            Assert.AreEqual(BlueBlock, blockId);
        }

        [Test]
        public void MoveBlock_OntoAnotherBlock_Throws()
        {
            var board = new BoardState(TwoBlockLevel());

            Assert.Throws<InvalidOperationException>(() => board.MoveBlock(BlueBlock, new Cell(2, 0)));
        }

        [Test]
        public void MoveBlock_DoesNotChangeTheLevelData()
        {
            LevelData level = TwoBlockLevel();
            var board = new BoardState(level);

            board.MoveBlock(BlueBlock, new Cell(0, 2));

            Assert.AreEqual(new Cell(0, 0), level.Blocks[BlueBlock].Cells[0]);
        }

        [Test]
        public void RemoveBlock_FreesItsCellsOnce()
        {
            var board = new BoardState(TwoBlockLevel());

            board.RemoveBlock(RedBlock);
            board.RemoveBlock(RedBlock);

            Assert.IsFalse(board.IsOnBoard(RedBlock));
            Assert.IsFalse(board.TryGetBlockAt(new Cell(3, 0), out _));
            Assert.AreEqual(1, board.RemainingBlockCount);
        }
    }
}
