using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Validation;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class LevelSolverTests
    {
        [Test]
        public void Solve_BlockWithAFreeWayToItsDoor_IsSolvable()
        {
            LevelData level = TestLevel.Create(3, 3)
                .WithBlock(BlockColor.Red, new Cell(1, 1))
                .WithDoor(BlockColor.Red, BoardSide.Top, 0, 1);

            Assert.AreEqual(Solvability.Solvable, LevelSolver.Solve(level).Solvability);
        }

        [Test]
        public void Solve_BlockWiderThanItsDoor_IsUnsolvableAndNamesTheBlock()
        {
            LevelData level = TestLevel.Create(3, 3)
                .WithBlock(BlockColor.Red, new Cell(0, 1), new Cell(1, 1))
                .WithDoor(BlockColor.Red, BoardSide.Top, 0, 1);

            SolverResult result = LevelSolver.Solve(level);

            Assert.AreEqual(Solvability.Unsolvable, result.Solvability);
            StringAssert.Contains("Block 1", result.Reason);
        }

        [Test]
        public void Solve_BlockerHasToStepAsideFirst_IsSolvable()
        {
            LevelData level = TestLevel.Create(3, 2)
                .WithBlock(BlockColor.Red, new Cell(0, 0))
                .WithBlock(BlockColor.Blue, new Cell(1, 0))
                .WithDoor(BlockColor.Red, BoardSide.Right, 0, 1)
                .WithDoor(BlockColor.Blue, BoardSide.Left, 1, 1);

            Assert.AreEqual(Solvability.Solvable, LevelSolver.Solve(level).Solvability);
        }

        [Test]
        public void Solve_BlocksThatLockEachOtherIn_IsUnsolvable()
        {
            LevelData level = TestLevel.Create(2, 1)
                .WithBlock(BlockColor.Red, new Cell(0, 0))
                .WithBlock(BlockColor.Blue, new Cell(1, 0))
                .WithDoor(BlockColor.Red, BoardSide.Right, 0, 1)
                .WithDoor(BlockColor.Blue, BoardSide.Left, 0, 1);

            Assert.AreEqual(Solvability.Unsolvable, LevelSolver.Solve(level).Solvability);
        }

        [Test]
        public void Solve_IceThatMeltsWhenTheOtherBlockLeaves_IsSolvable()
        {
            LevelData level = TwoBlocksWithTheirDoors();
            level.Blocks[0].IceCount = 1;
            level.Doors[0].IceCount = 1;

            Assert.AreEqual(Solvability.Solvable, LevelSolver.Solve(level).Solvability);
        }

        [Test]
        public void Solve_TwoBlocksThatWaitForEachOther_IsUnsolvable()
        {
            LevelData level = TwoBlocksWithTheirDoors();
            level.Blocks[0].IceCount = 1;
            level.Blocks[1].IceCount = 1;

            SolverResult result = LevelSolver.Solve(level);

            Assert.AreEqual(Solvability.Unsolvable, result.Solvability);
            StringAssert.Contains("ice", result.Reason);
        }

        [Test]
        public void Solve_OnlyDoorWaitsForMoreBlocksThanThereAre_IsUnsolvable()
        {
            LevelData level = TwoBlocksWithTheirDoors();
            level.Doors[1].IceCount = 2;

            Assert.AreEqual(Solvability.Unsolvable, LevelSolver.Solve(level).Solvability);
        }

        [Test]
        public void Solve_ArrowBlockWithItsDoorAcrossItsArrow_IsUnsolvable()
        {
            LevelData level = TestLevel.Create(3, 3)
                .WithBlock(BlockColor.Red, new Cell(1, 1))
                .WithDoor(BlockColor.Red, BoardSide.Top, 1, 1);
            level.Blocks[0].Axis = BlockAxis.Horizontal;

            Assert.AreEqual(Solvability.Unsolvable, LevelSolver.Solve(level).Solvability);
        }

        [Test]
        public void Solve_ArrowBlockWithItsDoorAlongItsArrow_IsSolvable()
        {
            LevelData level = TestLevel.Create(3, 3)
                .WithBlock(BlockColor.Red, new Cell(1, 1))
                .WithDoor(BlockColor.Red, BoardSide.Right, 1, 1);
            level.Blocks[0].Axis = BlockAxis.Horizontal;

            Assert.AreEqual(Solvability.Solvable, LevelSolver.Solve(level).Solvability);
        }

        [Test]
        public void Solve_MorePositionsThanTheLimit_IsUnknown()
        {
            LevelData level = TestLevel.Create(3, 2)
                .WithBlock(BlockColor.Red, new Cell(0, 0))
                .WithBlock(BlockColor.Blue, new Cell(1, 0))
                .WithDoor(BlockColor.Red, BoardSide.Right, 0, 1)
                .WithDoor(BlockColor.Blue, BoardSide.Left, 1, 1);

            Assert.AreEqual(Solvability.Unknown, LevelSolver.Solve(level, 1).Solvability);
        }

        private static LevelData TwoBlocksWithTheirDoors()
        {
            return TestLevel.Create(3, 3)
                .WithBlock(BlockColor.Red, new Cell(0, 0))
                .WithBlock(BlockColor.Blue, new Cell(2, 2))
                .WithDoor(BlockColor.Red, BoardSide.Left, 0, 1)
                .WithDoor(BlockColor.Blue, BoardSide.Right, 2, 1);
        }
    }
}
