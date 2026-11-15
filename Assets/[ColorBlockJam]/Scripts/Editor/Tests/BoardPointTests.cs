using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class BoardPointTests
    {
        [Test]
        public void MiddleOf_FullRectangle_IsTheMiddleOfItsBounds()
        {
            var cells = new[] { new Cell(2, 1), new Cell(3, 1), new Cell(2, 2), new Cell(3, 2) };

            BoardPoint middle = BoardPoint.MiddleOf(cells);

            Assert.AreEqual(3f, middle.X);
            Assert.AreEqual(2f, middle.Y);
        }

        [Test]
        public void MiddleOf_ShapeWithAGapInItsBounds_IsTheMiddleOfItsNearestCell()
        {
            var cells = new[] { new Cell(0, 0), new Cell(0, 1), new Cell(0, 2), new Cell(1, 0), new Cell(2, 0) };

            BoardPoint middle = BoardPoint.MiddleOf(cells);

            Assert.AreEqual(0.5f, middle.X);
            Assert.AreEqual(1.5f, middle.Y);
        }
    }
}
