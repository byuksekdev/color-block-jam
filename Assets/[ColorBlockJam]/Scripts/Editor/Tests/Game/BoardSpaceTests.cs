using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Game.BoardViews;
using NUnit.Framework;
using UnityEngine;

namespace ColorBlockJam.Tests.Game
{
    public class BoardSpaceTests
    {
        private const float Tolerance = 0.0001f;

        private static readonly Vector3 Center = new Vector3(3f, 1f, -2f);

        [Test]
        public void Center_AnyBoard_LiesHalfwayBetweenMinAndMax()
        {
            var space = new BoardSpace(Center, 4, 5);

            Assert.That(Vector3.Distance((space.Min + space.Max) / 2f, Center), Is.LessThan(Tolerance));
            Assert.AreEqual(4 * BoardSpace.CellSize, space.Max.x - space.Min.x, Tolerance);
            Assert.AreEqual(5 * BoardSpace.CellSize, space.Max.z - space.Min.z, Tolerance);
        }

        [Test]
        public void ToWorld_FirstCell_IsTheLowerLeftCornerOfTheBoard()
        {
            var space = new BoardSpace(Center, 4, 5);

            Assert.That(Vector3.Distance(space.ToWorld(BoardPoint.FromCell(new Cell(0, 0))), space.Min), Is.LessThan(Tolerance));
        }

        [Test]
        public void CellCenter_LastCell_LiesHalfACellInsideMax()
        {
            var space = new BoardSpace(Center, 4, 5);
            Vector3 halfCell = new Vector3(BoardSpace.CellSize, 0f, BoardSpace.CellSize) / 2f;

            Assert.That(Vector3.Distance(space.CellCenter(new Cell(3, 4)), space.Max - halfCell), Is.LessThan(Tolerance));
        }

        [Test]
        public void ToBoardPoint_WorldPositionOfAPoint_GivesThePointBack()
        {
            var space = new BoardSpace(Center, 4, 5);
            var point = new BoardPoint(1.25f, 3.5f);

            BoardPoint back = space.ToBoardPoint(space.ToWorld(point));

            Assert.AreEqual(point.X, back.X, Tolerance);
            Assert.AreEqual(point.Y, back.Y, Tolerance);
        }
    }
}
