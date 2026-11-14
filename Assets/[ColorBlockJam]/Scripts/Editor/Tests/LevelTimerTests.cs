using ColorBlockJam.Core.Sessions;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class LevelTimerTests
    {
        [Test]
        public void Advance_ExactlyTheTimeLeft_Expires()
        {
            var timer = new LevelTimer(10f);

            timer.Advance(10f);

            Assert.AreEqual(0f, timer.Remaining);
            Assert.AreEqual(0, timer.SecondsLeft);
            Assert.IsTrue(timer.IsExpired);
        }

        [Test]
        public void Advance_MoreThanTheTimeLeft_StopsAtZero()
        {
            var timer = new LevelTimer(1f);

            timer.Advance(5f);

            Assert.AreEqual(0f, timer.Remaining);
        }

        [Test]
        public void Advance_LessThanTheTimeLeft_IsNotExpired()
        {
            var timer = new LevelTimer(10f);

            timer.Advance(9.5f);

            Assert.IsFalse(timer.IsExpired);
        }

        [Test]
        public void Advance_NegativeTime_ChangesNothing()
        {
            var timer = new LevelTimer(10f);

            timer.Advance(-3f);

            Assert.AreEqual(10f, timer.Remaining);
        }

        [TestCase(0f)]
        [TestCase(-3f)]
        public void Constructor_DurationNotAboveZero_IsExpired(float duration)
        {
            var timer = new LevelTimer(duration);

            Assert.AreEqual(0f, timer.Remaining);
            Assert.IsTrue(timer.IsExpired);
        }

        [Test]
        public void SecondsLeft_InsideASecond_CountsTheStartedSecond()
        {
            var timer = new LevelTimer(300f);

            timer.Advance(0.25f);
            Assert.AreEqual(300, timer.SecondsLeft);

            timer.Advance(0.75f);
            Assert.AreEqual(299, timer.SecondsLeft);
        }
    }
}
