using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Flow;
using ColorBlockJam.Core.Players;
using ColorBlockJam.Core.Sessions;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class GameFlowTests
    {
        private const int LevelCount = 5;

        private static readonly BoardPoint RedDoor = new BoardPoint(1f, 2f);

        private FakePlayerStorage _storage;
        private FakeLevelSource _levels;
        private PlayerProfile _player;
        private GameFlow _flow;
        private LevelSession _session;
        private List<int> _rewards;

        [SetUp]
        public void SetUp()
        {
            _storage = new FakePlayerStorage();
            _levels = new FakeLevelSource(LevelCount);
            _player = new PlayerProfile(_storage);
            _flow = new GameFlow(_player, _levels, TestLevel.ExactDrag);
            _rewards = new List<int>();
            _session = null;
            _flow.LevelStarted += session => _session = session;
            _flow.LevelWon += _rewards.Add;
        }

        private static void Win(LevelSession session)
        {
            session.TryBeginDrag(FakeLevelSource.RedBlock);
            session.TryMoveDrag(RedDoor, out _);
            Assert.AreEqual(SessionState.Won, session.State, "The test level should be won by this move.");
        }

        [Test]
        public void IsInLevel_BeforeStartInALevelAndAfterLeaving_TellsWhetherALevelIsPlayed()
        {
            Assert.IsFalse(_flow.IsInLevel, "before the first level");

            _flow.StartLevel();
            Assert.IsTrue(_flow.IsInLevel, "in a level");

            _flow.LeaveLevel();
            Assert.IsFalse(_flow.IsInLevel, "after leaving");
        }

        [Test]
        public void StartLevel_NewPlayer_StartsTheFirstLevelAndWaitsForTheFirstTouch()
        {
            _flow.StartLevel();

            Assert.AreEqual(0, _flow.LevelIndex);
            Assert.AreEqual(SessionState.Ready, _session.State);
            CollectionAssert.AreEqual(new[] { 0 }, _levels.LoadedIndices);
        }

        [Test]
        public void StartLevel_EmptyLevelList_Throws()
        {
            var flow = new GameFlow(_player, new FakeLevelSource(0), TestLevel.ExactDrag);

            Assert.Throws<InvalidOperationException>(() => flow.StartLevel());
        }

        [Test]
        public void StartLevel_PastTheLastLevelFile_RepeatsFromTheFirstAndKeepsCounting()
        {
            _flow.StartLevel(LevelCount + 1);

            Assert.AreEqual(LevelCount + 1, _flow.LevelIndex);
            CollectionAssert.AreEqual(new[] { 1 }, _levels.LoadedIndices);
        }

        [Test]
        public void Win_LevelThePlayerHasReached_Grants40CoinsOnceAndMovesThePlayerOn()
        {
            _flow.StartLevel();

            Win(_session);

            Assert.AreEqual(440, _player.Coins);
            Assert.AreEqual(1, _player.LevelIndex);
            CollectionAssert.AreEqual(new[] { 40 }, _rewards);
            Assert.AreEqual(1, _storage.SaveCount);
        }

        [Test]
        public void Win_LevelPlayedAgain_GrantsNothing()
        {
            _flow.StartLevel();
            Win(_session);

            _flow.RestartLevel();
            Win(_session);

            Assert.AreEqual(440, _player.Coins);
            Assert.AreEqual(1, _player.LevelIndex);
            CollectionAssert.AreEqual(new[] { 40, 0 }, _rewards);
        }

        [Test]
        public void StartNextLevel_AfterAWin_StartsTheLevelThePlayerReached()
        {
            _flow.StartLevel();
            Win(_session);

            _flow.StartNextLevel();

            Assert.AreEqual(1, _flow.LevelIndex);
            Assert.AreEqual(_player.LevelIndex, _flow.LevelIndex);
            CollectionAssert.AreEqual(new[] { 0, 1 }, _levels.LoadedIndices);
        }

        [Test]
        public void StartNextLevel_LevelNotWon_StaysOnTheLevel()
        {
            _flow.StartLevel();
            LevelSession attempt = _session;

            _flow.StartNextLevel();

            Assert.AreSame(attempt, _session);
            Assert.AreEqual(0, _flow.LevelIndex);
        }

        [Test]
        public void StartNextLevel_PressedTwiceAfterAWin_MovesOneLevelOn()
        {
            _flow.StartLevel();
            Win(_session);

            _flow.StartNextLevel();
            _flow.StartNextLevel();

            Assert.AreEqual(1, _flow.LevelIndex);
        }

        [Test]
        public void RestartLevel_OnTheHomeScreen_StartsNothing()
        {
            _flow.RestartLevel();

            Assert.IsNull(_session);
            CollectionAssert.IsEmpty(_levels.LoadedIndices);
        }

        [Test]
        public void RestartLevel_AfterAMoveAndSomeTime_GivesTheStateOfAFreshStart()
        {
            _flow.StartLevel();
            LevelSession firstAttempt = _session;
            firstAttempt.TryBeginDrag(FakeLevelSource.RedBlock);
            firstAttempt.TryMoveDrag(new BoardPoint(0f, 0f), out _);
            firstAttempt.EndDrag();
            firstAttempt.Tick(4f);

            _flow.RestartLevel();

            Assert.AreNotSame(firstAttempt, _session);
            Assert.AreEqual(SessionState.Ready, _session.State);
            Assert.AreEqual(TestLevel.TimeLimitSeconds, _session.SecondsLeft);
            Assert.AreEqual(FakeLevelSource.RedBlockStart, _session.Board.GetOrigin(FakeLevelSource.RedBlock));
            Assert.AreEqual(1, _session.Board.RemainingBlockCount);
            Assert.AreEqual(0, _flow.LevelIndex);
        }

        [Test]
        public void RestartLevel_AfterTheTimeRanOut_StartsTheSameLevelAgain()
        {
            _flow.StartLevel();
            _session.TryBeginDrag(FakeLevelSource.RedBlock);
            _session.Tick(TestLevel.TimeLimitSeconds);
            Assert.AreEqual(SessionState.Failed, _session.State);

            _flow.RestartLevel();

            Assert.AreEqual(SessionState.Ready, _session.State);
            Assert.AreEqual(400, _player.Coins);
            Assert.AreEqual(0, _player.LevelIndex);
        }

        [Test]
        public void LeaveLevel_InTheMiddleOfALevelAndBackAgain_KeepsTheCoins()
        {
            _flow.StartLevel();
            Win(_session);
            _flow.StartNextLevel();

            _flow.LeaveLevel();
            _flow.StartLevel();

            Assert.AreEqual(440, _player.Coins);
            Assert.AreEqual(440, new PlayerProfile(_storage).Coins);
            Assert.AreEqual(1, _flow.LevelIndex);
        }

        [Test]
        public void LeaveLevel_InALevel_TellsTheViewsAndForgetsTheAttempt()
        {
            int leaveCount = 0;
            _flow.LevelLeft += () => leaveCount++;
            _flow.StartLevel();
            LevelSession leftAttempt = _session;

            _flow.LeaveLevel();
            Win(leftAttempt);

            Assert.AreEqual(1, leaveCount);
            Assert.AreEqual(400, _player.Coins);
            CollectionAssert.IsEmpty(_rewards);
        }

        [Test]
        public void LeaveLevel_OnTheHomeScreen_DoesNothing()
        {
            int leaveCount = 0;
            _flow.LevelLeft += () => leaveCount++;

            _flow.LeaveLevel();

            Assert.AreEqual(0, leaveCount);
        }

        [Test]
        public void RestartLevel_ThenTheOldAttemptIsWon_GrantsNothing()
        {
            _flow.StartLevel();
            LevelSession oldAttempt = _session;

            _flow.RestartLevel();
            Win(oldAttempt);

            Assert.AreEqual(400, _player.Coins);
            CollectionAssert.IsEmpty(_rewards);
        }
    }
}
