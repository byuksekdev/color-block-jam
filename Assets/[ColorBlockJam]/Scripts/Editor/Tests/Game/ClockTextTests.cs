using ColorBlockJam.Game.UI;
using NUnit.Framework;

namespace ColorBlockJam.Tests.Game
{
    public class ClockTextTests
    {
        [TestCase(300, "05:00")]
        [TestCase(299, "04:59")]
        [TestCase(60, "01:00")]
        [TestCase(59, "00:59")]
        [TestCase(9, "00:09")]
        [TestCase(0, "00:00")]
        [TestCase(3600, "60:00")]
        public void Format_Seconds_ShowsMinutesAndSecondsWithTwoDigits(int seconds, string expected)
        {
            Assert.AreEqual(expected, ClockText.Format(seconds));
        }
    }
}
