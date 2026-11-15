using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class BlockDragTests
    {
        private const int DraggedBlock = 0;
        private const float NoOverlap = 0f;
        private const float Overlap = 0.2f;
        private const float Tolerance = 0.001f;

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
            var drag = new BlockDrag(board, DraggedBlock, NoOverlap);

            MoveUntilItStops(drag, new BoardPoint(3.25f, 0f));

            Assert.AreEqual(3.25f, drag.Position.X);
            Assert.AreEqual(new Cell(3, 0), board.GetOrigin(DraggedBlock));
        }

        [Test]
        public void MoveTowards_TargetBeyondTheBoard_StopsAtTheEdge()
        {
            BoardState board = BoardWithBlockers();
            var drag = new BlockDrag(board, DraggedBlock, NoOverlap);

            MoveUntilItStops(drag, new BoardPoint(10f, 0f));

            Assert.AreEqual(4f, drag.Position.X);
            Assert.AreEqual(new Cell(4, 0), board.GetOrigin(DraggedBlock));
        }

        [Test]
        public void MoveTowards_TargetBehindAnotherBlock_StopsBesideIt()
        {
            BoardState board = BoardWithBlockers(new Cell(3, 0));
            var drag = new BlockDrag(board, DraggedBlock, NoOverlap);

            MoveUntilItStops(drag, new BoardPoint(4f, 0f));

            Assert.AreEqual(2f, drag.Position.X);
            Assert.AreEqual(new Cell(2, 0), board.GetOrigin(DraggedBlock));
        }

        [Test]
        public void MoveTowards_LessThanHalfACell_KeepsTheCell()
        {
            BoardState board = BoardWithBlockers();
            var drag = new BlockDrag(board, DraggedBlock, NoOverlap);

            bool enteredNewCell = drag.MoveTowards(new BoardPoint(0.4f, 0f));

            Assert.IsFalse(enteredNewCell);
            Assert.AreEqual(0.4f, drag.Position.X);
            Assert.AreEqual(new Cell(0, 0), board.GetOrigin(DraggedBlock));
        }

        [Test]
        public void MoveTowards_PastHalfACell_EntersTheNextCell()
        {
            BoardState board = BoardWithBlockers();
            var drag = new BlockDrag(board, DraggedBlock, NoOverlap);

            bool enteredNewCell = drag.MoveTowards(new BoardPoint(0.6f, 0f));

            Assert.IsTrue(enteredNewCell);
            Assert.AreEqual(new Cell(1, 0), board.GetOrigin(DraggedBlock));
        }

        [Test]
        public void MoveTowards_BlockedOnOneAxis_SlidesOnTheOther()
        {
            BoardState board = BoardWithBlockers(new Cell(1, 0), new Cell(1, 1), new Cell(1, 2));
            var drag = new BlockDrag(board, DraggedBlock, NoOverlap);

            MoveUntilItStops(drag, new BoardPoint(1f, 2f));

            Assert.AreEqual(0f, drag.Position.X);
            Assert.AreEqual(2f, drag.Position.Y);
        }

        [Test]
        public void MoveTowards_AroundACorner_ArrivesAtTheTarget()
        {
            BoardState board = BoardWithBlockers(new Cell(1, 0));
            var drag = new BlockDrag(board, DraggedBlock, NoOverlap);

            MoveUntilItStops(drag, new BoardPoint(1f, 1f));

            Assert.AreEqual(1f, drag.Position.X);
            Assert.AreEqual(1f, drag.Position.Y);
            Assert.AreEqual(new Cell(1, 1), board.GetOrigin(DraggedBlock));
        }

        [Test]
        public void MoveTowards_BetweenTwoRows_NeedsBothRowsFree()
        {
            BoardState board = BoardWithBlockers(new Cell(2, 1));
            var drag = new BlockDrag(board, DraggedBlock, NoOverlap);
            MoveUntilItStops(drag, new BoardPoint(0f, 0.4f));

            MoveUntilItStops(drag, new BoardPoint(4f, 0.4f));

            Assert.AreEqual(1f, drag.Position.X);
        }

        [Test]
        public void MoveTowards_AgainstAnotherBlockWithAnOverlap_SinksInThatFar()
        {
            BoardState board = BoardWithBlockers(new Cell(2, 0));
            var drag = new BlockDrag(board, DraggedBlock, Overlap);

            MoveUntilItStops(drag, new BoardPoint(4f, 0f));

            Assert.AreEqual(1f + Overlap, drag.Position.X, Tolerance);
            Assert.AreEqual(new Cell(1, 0), board.GetOrigin(DraggedBlock));
        }

        [Test]
        public void MoveTowards_AgainstTheBoardEdgeWithAnOverlap_StopsAtTheEdge()
        {
            var drag = new BlockDrag(BoardWithBlockers(), DraggedBlock, Overlap);

            MoveUntilItStops(drag, new BoardPoint(9f, 0f));

            Assert.AreEqual(4f, drag.Position.X);
        }

        [Test]
        public void MoveTowards_BetweenTwoRowsWithinTheOverlap_PassesTheBlocker()
        {
            BoardState board = BoardWithBlockers(new Cell(2, 1));
            var drag = new BlockDrag(board, DraggedBlock, Overlap);
            MoveUntilItStops(drag, new BoardPoint(0f, 0.15f));

            MoveUntilItStops(drag, new BoardPoint(4f, 0.15f));

            Assert.AreEqual(4f, drag.Position.X);
        }

        [Test]
        public void MoveTowards_BetweenTwoRowsBeyondTheOverlap_StopsAtTheBlocker()
        {
            BoardState board = BoardWithBlockers(new Cell(2, 1));
            var drag = new BlockDrag(board, DraggedBlock, Overlap);
            MoveUntilItStops(drag, new BoardPoint(0f, 0.4f));

            MoveUntilItStops(drag, new BoardPoint(4f, 0.4f));

            Assert.AreEqual(1f + Overlap, drag.Position.X, Tolerance);
        }

        [Test]
        public void MoveTowards_SunkIntoABlock_StillSlidesAlongIt()
        {
            BoardState board = BoardWithBlockers(new Cell(2, 0));
            var drag = new BlockDrag(board, DraggedBlock, Overlap);
            MoveUntilItStops(drag, new BoardPoint(4f, 0f));

            MoveUntilItStops(drag, new BoardPoint(4f, 2f));

            Assert.AreEqual(4f, drag.Position.X);
            Assert.AreEqual(2f, drag.Position.Y);
        }

        [TestCase(BoardSide.Left, 1.5f, 0f, 1.5f)]
        [TestCase(BoardSide.Right, 1.5f, 0f, 2.5f)]
        [TestCase(BoardSide.Bottom, 0f, 1.5f, 1.5f)]
        [TestCase(BoardSide.Top, 0f, 1.5f, 2.5f)]
        [TestCase(BoardSide.Top, 0f, 4.5f, -0.5f)]
        public void DistanceFrom_AnOriginOnTheBoard_IsTheWayLeftToThatSide(BoardSide side, float originX, float originY, float expected)
        {
            var drag = new BlockDrag(BoardWithBlockers(), DraggedBlock, NoOverlap);

            Assert.AreEqual(expected, drag.DistanceFrom(side, new BoardPoint(originX, originY)), Tolerance);
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
            var drag = new BlockDrag(BoardWithBlockers(), DraggedBlock, NoOverlap);

            Assert.AreEqual(expected, drag.IsPushedAgainst(side, new BoardPoint(targetX, targetY)));
        }

        [TestCase(BlockAxis.Horizontal, 2f, 0f)]
        [TestCase(BlockAxis.Vertical, 0f, 2f)]
        [TestCase(BlockAxis.Any, 2f, 2f)]
        public void MoveTowards_ArrowBlock_MovesOnlyAlongItsArrow(BlockAxis axis, float expectedX, float expectedY)
        {
            LevelData level = TestLevel.Create(5, 5).WithBlock(BlockColor.Blue, new Cell(0, 0));
            level.Blocks[0].Axis = axis;
            var drag = new BlockDrag(new BoardState(level), DraggedBlock, NoOverlap);

            MoveUntilItStops(drag, new BoardPoint(2f, 2f));

            Assert.AreEqual(expectedX, drag.Position.X);
            Assert.AreEqual(expectedY, drag.Position.Y);
        }
    }
}
