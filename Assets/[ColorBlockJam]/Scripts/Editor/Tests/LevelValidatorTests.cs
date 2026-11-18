using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Validation;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class LevelValidatorTests
    {
        private static LevelData ValidLevel()
        {
            return TestLevel.Create(3, 3)
                .WithBlock(BlockColor.Red, new Cell(1, 1))
                .WithDoor(BlockColor.Red, BoardSide.Top, 1, 1);
        }

        private static List<LevelProblemKind> ProblemKinds(LevelData level)
        {
            var kinds = new List<LevelProblemKind>();
            foreach (LevelProblem problem in LevelValidator.Validate(level))
            {
                kinds.Add(problem.Kind);
            }

            return kinds;
        }

        [Test]
        public void Validate_WellFormedLevel_FindsNoProblem()
        {
            CollectionAssert.IsEmpty(ProblemKinds(ValidLevel()));
        }

        [Test]
        public void Validate_NoLevel_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => LevelValidator.Validate(null));
        }

        [Test]
        public void Validate_OneByOneBoard_FindsNoProblem()
        {
            LevelData level = TestLevel.Create(1, 1)
                .WithBlock(BlockColor.Red, new Cell(0, 0))
                .WithDoor(BlockColor.Red, BoardSide.Top, 0, 1);

            CollectionAssert.IsEmpty(ProblemKinds(level));
        }

        [TestCase(0, 3)]
        [TestCase(3, 0)]
        public void Validate_BoardWithoutCells_ReportsTheBoard(int width, int height)
        {
            LevelData level = ValidLevel();
            level.Width = width;
            level.Height = height;

            CollectionAssert.Contains(ProblemKinds(level), LevelProblemKind.BoardTooSmall);
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void Validate_TimeLimitNotAboveZero_ReportsTheTimer(int seconds)
        {
            LevelData level = ValidLevel();
            level.TimeLimitSeconds = seconds;

            CollectionAssert.AreEqual(new[] { LevelProblemKind.TimeLimitNotPositive }, ProblemKinds(level));
        }

        [Test]
        public void Validate_UnknownFormatVersion_ReportsTheVersion()
        {
            LevelData level = ValidLevel();
            level.Version = LevelData.CurrentVersion + 1;

            CollectionAssert.AreEqual(new[] { LevelProblemKind.UnsupportedVersion }, ProblemKinds(level));
        }

        [Test]
        public void Validate_NoBlocks_ReportsTheEmptyLevel()
        {
            LevelData level = TestLevel.Create(3, 3);

            CollectionAssert.AreEqual(new[] { LevelProblemKind.NoBlocks }, ProblemKinds(level));
        }

        [Test]
        public void Validate_BlockWithoutColour_ReportsTheBlock()
        {
            LevelData level = ValidLevel().WithBlock(BlockColor.None, new Cell(0, 0));

            CollectionAssert.AreEqual(new[] { LevelProblemKind.BlockHasNoColor }, ProblemKinds(level));
        }

        [Test]
        public void Validate_BlockWithoutCells_ReportsTheBlock()
        {
            LevelData level = ValidLevel().WithBlock(BlockColor.Red);

            CollectionAssert.AreEqual(new[] { LevelProblemKind.BlockHasNoCells }, ProblemKinds(level));
        }

        [Test]
        public void Validate_BlockPartlyOutsideTheBoard_ReportsTheBlock()
        {
            LevelData level = ValidLevel().WithBlock(BlockColor.Red, new Cell(2, 0), new Cell(3, 0));

            CollectionAssert.AreEqual(new[] { LevelProblemKind.BlockOutsideBoard }, ProblemKinds(level));
        }

        [Test]
        public void Validate_TwoBlocksOnOneCell_ReportsTheOverlap()
        {
            LevelData level = ValidLevel().WithBlock(BlockColor.Red, new Cell(1, 1), new Cell(1, 0));

            CollectionAssert.AreEqual(new[] { LevelProblemKind.BlocksOverlap }, ProblemKinds(level));
        }

        [Test]
        public void Validate_BlockColourWithoutDoor_ReportsTheColour()
        {
            LevelData level = ValidLevel().WithBlock(BlockColor.Blue, new Cell(0, 0));

            CollectionAssert.AreEqual(new[] { LevelProblemKind.BlockColorWithoutDoor }, ProblemKinds(level));
        }

        [Test]
        public void Validate_DoorWithoutColour_ReportsTheDoor()
        {
            LevelData level = ValidLevel().WithDoor(BlockColor.None, BoardSide.Left, 0, 1);

            CollectionAssert.AreEqual(new[] { LevelProblemKind.DoorHasNoColor }, ProblemKinds(level));
        }

        [TestCase(2, 2)]
        [TestCase(-1, 1)]
        [TestCase(0, 0)]
        public void Validate_DoorThatDoesNotFitOnItsSide_ReportsTheDoor(int start, int length)
        {
            LevelData level = ValidLevel().WithDoor(BlockColor.Red, BoardSide.Bottom, start, length);

            CollectionAssert.AreEqual(new[] { LevelProblemKind.DoorOutsideSide }, ProblemKinds(level));
        }

        [TestCase(BoardSide.Left, true)]
        [TestCase(BoardSide.Right, true)]
        [TestCase(BoardSide.Bottom, false)]
        [TestCase(BoardSide.Top, false)]
        public void Validate_DoorOnATallBoard_MeasuresLeftAndRightSidesInRows(BoardSide side, bool fits)
        {
            LevelData level = TestLevel.Create(3, 5)
                .WithBlock(BlockColor.Red, new Cell(1, 1))
                .WithDoor(BlockColor.Red, side, 3, 2);

            Assert.AreEqual(!fits, ProblemKinds(level).Contains(LevelProblemKind.DoorOutsideSide));
        }

        [Test]
        public void Validate_TwoDoorsOnTheSameCells_ReportsTheOverlap()
        {
            LevelData level = ValidLevel().WithDoor(BlockColor.Red, BoardSide.Top, 0, 2);

            CollectionAssert.AreEqual(new[] { LevelProblemKind.DoorsOverlap }, ProblemKinds(level));
        }

        [Test]
        public void Validate_SameSpanOnAnotherSide_FindsNoProblem()
        {
            LevelData level = ValidLevel().WithDoor(BlockColor.Red, BoardSide.Bottom, 1, 1);

            CollectionAssert.IsEmpty(ProblemKinds(level));
        }

        [Test]
        public void Validate_IceCountBelowZero_IsReportedForBlockAndDoor()
        {
            LevelData level = TestLevel.Create(3, 3)
                .WithBlock(BlockColor.Red, new Cell(0, 0))
                .WithDoor(BlockColor.Red, BoardSide.Top, 0, 1);
            level.Blocks[0].IceCount = -1;
            level.Doors[0].IceCount = -1;

            List<LevelProblem> problems = LevelValidator.Validate(level);

            Assert.AreEqual(2, problems.Count);
            Assert.AreEqual(LevelProblemKind.IceCountNegative, problems[0].Kind);
            Assert.AreEqual(LevelProblemKind.IceCountNegative, problems[1].Kind);
        }

        [Test]
        public void Validate_ValuesThatAreNoColourDirectionOrSide_AreReportedEach()
        {
            const int unknown = 999;
            LevelData level = TestLevel.Create(3, 3)
                .WithBlock((BlockColor)unknown, new Cell(0, 0))
                .WithDoor((BlockColor)unknown, (BoardSide)unknown, 0, 1);
            level.Blocks[0].Axis = (BlockAxis)unknown;

            List<LevelProblem> problems = LevelValidator.Validate(level);

            Assert.AreEqual(4, problems.Count);
            Assert.AreEqual(LevelProblemKind.BlockColorUnknown, problems[0].Kind);
            Assert.AreEqual(LevelProblemKind.BlockAxisUnknown, problems[1].Kind);
            Assert.AreEqual(LevelProblemKind.DoorColorUnknown, problems[2].Kind);
            Assert.AreEqual(LevelProblemKind.DoorSideUnknown, problems[3].Kind);
        }

        [Test]
        public void Validate_ProblemOfOneBlock_PointsAtThatBlock()
        {
            LevelData level = ValidLevel().WithBlock(BlockColor.Red, new Cell(1, 1));

            List<LevelProblem> problems = LevelValidator.Validate(level);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(LevelPart.Block(1), problems[0].Part);
        }

        [Test]
        public void Validate_BlockColourWithoutDoor_PointsAtTheBlock()
        {
            LevelData level = ValidLevel().WithBlock(BlockColor.Blue, new Cell(0, 0));

            List<LevelProblem> problems = LevelValidator.Validate(level);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(LevelPart.Block(1), problems[0].Part);
        }

        [Test]
        public void Validate_ProblemOfOneDoor_PointsAtThatDoor()
        {
            LevelData level = ValidLevel().WithDoor(BlockColor.Red, BoardSide.Top, 0, 2);

            List<LevelProblem> problems = LevelValidator.Validate(level);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(LevelPart.Door(1), problems[0].Part);
        }

        [Test]
        public void Validate_ProblemOfTheWholeLevel_PointsAtNoPart()
        {
            LevelData level = ValidLevel();
            level.TimeLimitSeconds = 0;

            List<LevelProblem> problems = LevelValidator.Validate(level);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(LevelPartKind.None, problems[0].Part.Kind);
        }
    }
}
