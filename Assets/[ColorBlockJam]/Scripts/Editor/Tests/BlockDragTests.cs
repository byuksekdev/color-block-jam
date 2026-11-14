using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class BlockDragTests
    {
        private const int DraggedBlock = 0;

        private static BoardState BoardWithBlockers(params Cell[] blockers)
        {
            LevelData level = TestLevel.Create(5, 5).WithBlock(BlockColor.Blue, new Cell(0, 0));
            foreach (Cell blocker in blockers)
            {
                level.WithBlock(BlockColor.Red, blocker);
            }

            return new BoardState(level);
        }

        private static void MoveUntilItStops(BlockDrag drag, BoardPoint target)
        {
            while (drag.MoveTowards(target))
            {
            }
        }

        [Test]
        public void MoveTowards_FreePath_ArrivesAtTheTarget()
        {
            BoardState board = BoardWithBlockers();
            var drag = new BlockDrag(board, DraggedBlock);

            MoveUntilItStops(drag, new BoardPoint(3.25f, 0f));

            Assert.AreEqual(3.25f, drag.Position.X);
            Assert.AreEqual(new Cell(3, 0), board.GetOrigin(DraggedBlock));
        }

        [Test]
        public void MoveTowards_TargetBeyondTheBoard_StopsAtTheEdge()
        {
            BoardState board = BoardWithBlockers();
            var drag = new BlockDrag(board, DraggedBlock);

            MoveUntilItStops(drag, new BoardPoint(10f, 0f));

            Assert.AreEqual(4f, drag.Position.X);
            Assert.AreEqual(new Cell(4, 0), board.GetOrigin(DraggedBlock));
        }

        [Test]
        public void MoveTowards_TargetBehindAnotherBlock_StopsBesideIt()
        {
            BoardState board = BoardWithBlockers(new Cell(3, 0));
            var drag = new BlockDrag(board, DraggedBlock);

            MoveUntilItStops(drag, new BoardPoint(4f, 0f));

            Assert.AreEqual(2f, drag.Position.X);
            Assert.AreEqual(new Cell(2, 0), board.GetOrigin(DraggedBlock));
        }

        [Test]
        public void MoveTowards_LessThanHalfACell_KeepsTheCell()
        {
            BoardState board = BoardWithBlockers();
            var drag = new BlockDrag(board, DraggedBlock);

            bool enteredNewCell = drag.MoveTowards(new BoardPoint(0.4f, 0f));

            Assert.IsFalse(enteredNewCell);
            Assert.AreEqual(0.4f, drag.Position.X);
            Assert.AreEqual(new Cell(0, 0), board.GetOrigin(DraggedBlock));
        }

        [Test]
        public void MoveTowards_PastHalfACell_EntersTheNextCell()
        {
            BoardState board = BoardWithBlockers();
            var drag = new BlockDrag(board, DraggedBlock);

            bool enteredNewCell = drag.MoveTowards(new BoardPoint(0.6f, 0f));

            Assert.IsTrue(enteredNewCell);
            Assert.AreEqual(new Cell(1, 0), board.GetOrigin(DraggedBlock));
        }

        [Test]
        public void MoveTowards_BlockedOnOneAxis_SlidesOnTheOther()
        {
            BoardState board = BoardWithBlockers(new Cell(1, 0), new Cell(1, 1), new Cell(1, 2));
            var drag = new BlockDrag(board, DraggedBlock);

            MoveUntilItStops(drag, new BoardPoint(1f, 2f));

            Assert.AreEqual(0f, drag.Position.X);
            Assert.AreEqual(2f, drag.Position.Y);
        }

        [Test]
        public void MoveTowards_AroundACorner_ArrivesAtTheTarget()
        {
            BoardState board = BoardWithBlockers(new Cell(1, 0));
            var drag = new BlockDrag(board, DraggedBlock);

            MoveUntilItStops(drag, new BoardPoint(1f, 1f));

            Assert.AreEqual(1f, drag.Position.X);
            Assert.AreEqual(1f, drag.Position.Y);
            Assert.AreEqual(new Cell(1, 1), board.GetOrigin(DraggedBlock));
        }

        [Test]
        public void MoveTowards_BetweenTwoRows_NeedsBothRowsFree()
        {
            BoardState board = BoardWithBlockers(new Cell(2, 1));
            var drag = new BlockDrag(board, DraggedBlock);
            MoveUntilItStops(drag, new BoardPoint(0f, 0.4f));

            MoveUntilItStops(drag, new BoardPoint(4f, 0.4f));

            Assert.AreEqual(1f, drag.Position.X);
        }

        [TestCase(BoardSide.Left, -0.1f, 0f, true)]
        [TestCase(BoardSide.Left, 0f, 0f, false)]
        [TestCase(BoardSide.Right, 4.1f, 0f, true)]
        [TestCase(BoardSide.Right, 4f, 0f, false)]
        [TestCase(BoardSide.Bottom, 0f, -0.1f, true)]
        [TestCase(BoardSide.Bottom, 0f, 0f, false)]
        [TestCase(BoardSide.Top, 0f, 4.1f, true)]
        [TestCase(BoardSide.Top, 0f, 4f, false)]
        public void IsPushedAgainst_TargetBeyondThatEdge_IsTrue(BoardSide side, float targetX, float targetY, bool expected)
        {
            var drag = new BlockDrag(BoardWithBlockers(), DraggedBlock);

            Assert.AreEqual(expected, drag.IsPushedAgainst(side, new BoardPoint(targetX, targetY)));
        }
    }
}
