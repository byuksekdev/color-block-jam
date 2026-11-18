using System;
using System.Globalization;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Game.Persistence;
using NUnit.Framework;

namespace ColorBlockJam.Tests.Game
{
    public class LevelSerializerTests
    {
        private CultureInfo _originalCulture;

        [SetUp]
        public void RememberCulture()
        {
            _originalCulture = CultureInfo.CurrentCulture;
        }

        [TearDown]
        public void RestoreCulture()
        {
            CultureInfo.CurrentCulture = _originalCulture;
        }

        private static LevelData SampleLevel()
        {
            LevelData level = TestLevel.Create(5, 6)
                .WithBlock(BlockColor.Blue, new Cell(1, 4), new Cell(2, 4), new Cell(1, 3))
                .WithBlock(BlockColor.Red, new Cell(4, 0))
                .WithDoor(BlockColor.Blue, BoardSide.Top, 0, 2)
                .WithDoor(BlockColor.Red, BoardSide.Right, 3, 2);

            level.Blocks[0].Axis = BlockAxis.Vertical;
            level.Blocks[1].IceCount = 1;
            level.Doors[0].IceCount = 1;
            return level;
        }

        private static void AssertSameLevel(LevelData expected, LevelData actual)
        {
            Assert.AreEqual(expected.Version, actual.Version);
            Assert.AreEqual(expected.Width, actual.Width);
            Assert.AreEqual(expected.Height, actual.Height);
            Assert.AreEqual(expected.TimeLimitSeconds, actual.TimeLimitSeconds);

            Assert.AreEqual(expected.Blocks.Count, actual.Blocks.Count);
            for (int i = 0; i < expected.Blocks.Count; i++)
            {
                Assert.AreEqual(expected.Blocks[i].Color, actual.Blocks[i].Color);
                CollectionAssert.AreEqual(expected.Blocks[i].Cells, actual.Blocks[i].Cells);
                Assert.AreEqual(expected.Blocks[i].Axis, actual.Blocks[i].Axis);
                Assert.AreEqual(expected.Blocks[i].IceCount, actual.Blocks[i].IceCount);
            }

            Assert.AreEqual(expected.Doors.Count, actual.Doors.Count);
            for (int i = 0; i < expected.Doors.Count; i++)
            {
                Assert.AreEqual(expected.Doors[i].Color, actual.Doors[i].Color);
                Assert.AreEqual(expected.Doors[i].Side, actual.Doors[i].Side);
                Assert.AreEqual(expected.Doors[i].Start, actual.Doors[i].Start);
                Assert.AreEqual(expected.Doors[i].Length, actual.Doors[i].Length);
                Assert.AreEqual(expected.Doors[i].IceCount, actual.Doors[i].IceCount);
            }
        }

        [Test]
        public void FromJson_TextWrittenByToJson_GivesAnEqualLevel()
        {
            LevelData level = SampleLevel();

            LevelData loaded = LevelSerializer.FromJson(LevelSerializer.ToJson(level));

            AssertSameLevel(level, loaded);
        }

        [Test]
        public void FromJson_TextWrittenUnderATurkishCulture_GivesAnEqualLevel()
        {
            LevelData level = SampleLevel();
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            string json = LevelSerializer.ToJson(level);
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            LevelData loaded = LevelSerializer.FromJson(json);

            AssertSameLevel(level, loaded);
        }

        [TestCase(0, 0)]
        [TestCase(1, 1)]
        public void FromJson_BoardWithoutBlocks_GivesAnEqualLevel(int width, int height)
        {
            LevelData level = TestLevel.Create(width, height);

            LevelData loaded = LevelSerializer.FromJson(LevelSerializer.ToJson(level));

            AssertSameLevel(level, loaded);
        }

        [Test]
        public void FromJson_MissingFields_UsesTheirDefaults()
        {
            LevelData loaded = LevelSerializer.FromJson("{\"Width\":3,\"Height\":4}");

            Assert.AreEqual(3, loaded.Width);
            Assert.AreEqual(4, loaded.Height);
            Assert.AreEqual(LevelData.CurrentVersion, loaded.Version);
            Assert.AreEqual(0, loaded.TimeLimitSeconds);
            Assert.AreEqual(0, loaded.Blocks.Count);
            Assert.AreEqual(0, loaded.Doors.Count);
        }

        [Test]
        public void FromJson_UnknownField_IsIgnored()
        {
            LevelData loaded = LevelSerializer.FromJson("{\"Width\":3,\"Gravity\":9.8}");

            Assert.AreEqual(3, loaded.Width);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("not a level")]
        [TestCase("null")]
        public void FromJson_TextThatHoldsNoLevel_Throws(string text)
        {
            Assert.Throws<FormatException>(() => LevelSerializer.FromJson(text));
        }
    }
}
