using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Editing;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class ShapeTransformTests
    {
        private static readonly Cell[] TallL = { new Cell(0, 0), new Cell(1, 0), new Cell(0, 1), new Cell(0, 2) };

        [Test]
        public void RotateClockwise_TallL_LiesDownWithItsFootOnTheLeft()
        {
            Cell[] rotated = ShapeTransform.RotateClockwise(TallL);

            CollectionAssert.AreEquivalent(
                new[] { new Cell(0, 1), new Cell(0, 0), new Cell(1, 1), new Cell(2, 1) },
                rotated);
        }

        [Test]
        public void RotateClockwise_FourTimes_GivesTheSameShape()
        {
            Cell[] shape = TallL;
            for (int i = 0; i < 4; i++)
            {
                shape = ShapeTransform.RotateClockwise(shape);
            }

            CollectionAssert.AreEquivalent(TallL, shape);
        }

        [Test]
        public void FlipHorizontally_TallL_PutsItsFootOnTheOtherSide()
        {
            Cell[] flipped = ShapeTransform.FlipHorizontally(TallL);

            CollectionAssert.AreEquivalent(
                new[] { new Cell(1, 0), new Cell(0, 0), new Cell(1, 1), new Cell(1, 2) },
                flipped);
        }

        [Test]
        public void FlipHorizontally_DoesNotChangeTheGivenCells()
        {
            ShapeTransform.FlipHorizontally(TallL);

            Assert.AreEqual(new Cell(1, 0), TallL[1]);
        }

        [Test]
        public void BlockShapes_EveryShape_StartsAtTheOriginWithoutRepeatedCells()
        {
            foreach (BlockShape shape in BlockShapes.All)
            {
                int minX = int.MaxValue;
                int minY = int.MaxValue;
                foreach (Cell cell in shape.Cells)
                {
                    minX = System.Math.Min(minX, cell.X);
                    minY = System.Math.Min(minY, cell.Y);
                }

                Assert.AreEqual(0, minX, shape.Name);
                Assert.AreEqual(0, minY, shape.Name);
                CollectionAssert.AllItemsAreUnique(shape.Cells, shape.Name);
            }
        }
    }
}
