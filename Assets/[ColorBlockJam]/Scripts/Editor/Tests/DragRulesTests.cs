using ColorBlockJam.Core.Boards;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class DragRulesTests
    {
        [TestCase(0.2f, 0.2f)]
        [TestCase(-1f, 0f)]
        [TestCase(0.8f, 0.45f)]
        public void Constructor_BlockOverlap_IsKeptBelowHalfACell(float given, float expected)
        {
            var rules = new DragRules(given, 0.5f);

            Assert.AreEqual(expected, rules.BlockOverlap);
        }

        [TestCase(0.3f, 0.3f)]
        [TestCase(-1f, 0f)]
        [TestCase(2f, 0.5f)]
        public void Constructor_DoorReach_IsKeptWithinHalfACell(float given, float expected)
        {
            var rules = new DragRules(0f, given);

            Assert.AreEqual(expected, rules.DoorReach);
        }
    }
}
