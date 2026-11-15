using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Sessions;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class LevelSessionTests
    {
        private const int RedBlock = 0;
        private const int BlueBlock = 1;

        private static readonly BoardPoint RedDoor = new BoardPoint(1f, 2f);

        private static LevelData OneBlockLevel()
        {
            return TestLevel.Create(3, 3)
                .WithBlock(BlockColor.Red, new Cell(1, 1))
                .WithDoor(BlockColor.Red, BoardSide.Top, 1, 1);
        }

        private static LevelData TwoBlockLevel()
        {
            return OneBlockLevel()
                .WithBlock(BlockColor.Blue, new Cell(0, 0))
                .WithDoor(BlockColor.Blue, BoardSide.Bottom, 0, 1);
        }

        private static List<SessionState> RecordStates(LevelSession session)
        {
            var states = new List<SessionState>();
            session.StateChanged += states.Add;
            return states;
        }

        [Test]
        public void Constructor_ValidLevel_WaitsWithTheFullTime()
        {
            var session = new LevelSession(OneBlockLevel(), TestLevel.ExactDrag);

            Assert.AreEqual(SessionState.Ready, session.State);
            Assert.AreEqual(TestLevel.TimeLimitSeconds, session.SecondsLeft);
        }

        [Test]
        public void Constructor_LevelWithProblems_Throws()
        {
            LevelData level = TestLevel.Create(3, 3);

            Assert.Throws<ArgumentException>(() => new LevelSession(level, TestLevel.ExactDrag));
        }

        [Test]
        public void Tick_BeforeTheFirstTouch_DoesNotCountDown()
        {
            var session = new LevelSession(OneBlockLevel(), TestLevel.ExactDrag);

            session.Tick(5f);

            Assert.AreEqual(SessionState.Ready, session.State);
            Assert.AreEqual(TestLevel.TimeLimitSeconds, session.SecondsLeft);
        }

        [Test]
        public void TryBeginDrag_FirstTouch_StartsTheTimer()
        {
            var session = new LevelSession(OneBlockLevel(), TestLevel.ExactDrag);

            bool began = session.TryBeginDrag(RedBlock);
            session.Tick(3f);

            Assert.IsTrue(began);
            Assert.AreEqual(SessionState.Playing, session.State);
            Assert.AreEqual(7, session.SecondsLeft);
        }

        [Test]
        public void TryBeginDrag_WhileAnotherBlockIsDragged_IsRefused()
        {
            var session = new LevelSession(TwoBlockLevel(), TestLevel.ExactDrag);
            session.TryBeginDrag(RedBlock);

            Assert.IsFalse(session.TryBeginDrag(BlueBlock));
        }

        [Test]
        public void Tick_PastTheTimeLeft_FailsOnce()
        {
            var session = new LevelSession(OneBlockLevel(), TestLevel.ExactDrag);
            session.TryBeginDrag(RedBlock);
            List<SessionState> states = RecordStates(session);

            session.Tick(20f);
            session.Tick(1f);

            Assert.AreEqual(SessionState.Failed, session.State);
            Assert.AreEqual(0, session.SecondsLeft);
            CollectionAssert.AreEqual(new[] { SessionState.Failed }, states);
        }

        [Test]
        public void Tick_PastTheTimeLeft_EndsTheDrag()
        {
            var session = new LevelSession(OneBlockLevel(), TestLevel.ExactDrag);
            session.TryBeginDrag(RedBlock);

            session.Tick(20f);

            Assert.IsFalse(session.TryMoveDrag(RedDoor, out _));
            Assert.IsTrue(session.Board.IsOnBoard(RedBlock));
            Assert.IsFalse(session.TryBeginDrag(RedBlock));
        }

        [Test]
        public void Tick_WhilePaused_DoesNotCountDown()
        {
            var session = new LevelSession(OneBlockLevel(), TestLevel.ExactDrag);
            session.TryBeginDrag(RedBlock);

            session.Pause();
            session.Tick(5f);

            Assert.AreEqual(SessionState.Paused, session.State);
            Assert.AreEqual(TestLevel.TimeLimitSeconds, session.SecondsLeft);
        }

        [Test]
        public void Resume_AfterPauseWhilePlaying_CountsDownAgain()
        {
            var session = new LevelSession(OneBlockLevel(), TestLevel.ExactDrag);
            session.TryBeginDrag(RedBlock);
            session.Pause();

            session.Resume();
            session.Tick(1f);

            Assert.AreEqual(SessionState.Playing, session.State);
            Assert.AreEqual(9, session.SecondsLeft);
        }

        [Test]
        public void Resume_AfterPauseBeforeTheFirstTouch_WaitsAgain()
        {
            var session = new LevelSession(OneBlockLevel(), TestLevel.ExactDrag);
            session.Pause();

            session.Resume();
            session.Tick(1f);

            Assert.AreEqual(SessionState.Ready, session.State);
            Assert.AreEqual(TestLevel.TimeLimitSeconds, session.SecondsLeft);
        }

        [Test]
        public void Pause_DuringADrag_EndsTheDrag()
        {
            var session = new LevelSession(OneBlockLevel(), TestLevel.ExactDrag);
            session.TryBeginDrag(RedBlock);

            session.Pause();
            session.Resume();

            Assert.IsFalse(session.TryMoveDrag(RedDoor, out _));
            Assert.IsTrue(session.Board.IsOnBoard(RedBlock));
        }

        [Test]
        public void TryMoveDrag_OnAFreePath_ReportsThePosition()
        {
            var session = new LevelSession(OneBlockLevel(), TestLevel.ExactDrag);
            session.TryBeginDrag(RedBlock);

            bool isDragging = session.TryMoveDrag(new BoardPoint(0.25f, 1f), out BoardPoint position);

            Assert.IsTrue(isDragging);
            Assert.AreEqual(0.25f, position.X);
            Assert.AreEqual(1f, position.Y);
        }

        [Test]
        public void TryMoveDrag_BlockReachesItsDoor_LeavesTheBoard()
        {
            var session = new LevelSession(TwoBlockLevel(), TestLevel.ExactDrag);
            Block exitedBlock = null;
            Cell exitOrigin = default;
            Door usedDoor = null;
            int exitCount = 0;
            session.BlockExited += (block, origin, door) =>
            {
                exitedBlock = block;
                exitOrigin = origin;
                usedDoor = door;
                exitCount++;
            };
            session.TryBeginDrag(RedBlock);

            bool isDragging = session.TryMoveDrag(RedDoor, out _);

            Assert.IsFalse(isDragging);
            Assert.AreEqual(1, exitCount);
            Assert.AreEqual(RedBlock, exitedBlock.Id);
            Assert.AreEqual(new Cell(1, 2), exitOrigin);
            Assert.AreEqual(BoardSide.Top, usedDoor.Side);
            Assert.IsFalse(session.Board.IsOnBoard(RedBlock));
            Assert.AreEqual(SessionState.Playing, session.State);
        }

        [Test]
        public void TryMoveDrag_BlockAlreadyAtItsDoor_LeavesOnlyWhenPushed()
        {
            var session = new LevelSession(TwoBlockLevel(), TestLevel.ExactDrag);
            session.TryBeginDrag(BlueBlock);

            bool staysWhenHeld = session.TryMoveDrag(new BoardPoint(0f, 0f), out _);
            bool staysWhenPushed = session.TryMoveDrag(new BoardPoint(0f, -0.5f), out _);

            Assert.IsTrue(staysWhenHeld);
            Assert.IsFalse(staysWhenPushed);
            Assert.IsFalse(session.Board.IsOnBoard(BlueBlock));
        }

        [Test]
        public void TryMoveDrag_BlockOutsideTheReachOfItsDoor_StaysUntilItComesWithinReach()
        {
            var session = new LevelSession(OneBlockLevel(), new DragRules(0f, 0.2f));
            session.TryBeginDrag(RedBlock);

            bool staysOutsideReach = session.TryMoveDrag(new BoardPoint(1f, 1.6f), out _);
            bool staysWithinReach = session.TryMoveDrag(new BoardPoint(1f, 1.85f), out _);

            Assert.IsTrue(staysOutsideReach);
            Assert.IsFalse(staysWithinReach);
            Assert.IsFalse(session.Board.IsOnBoard(RedBlock));
        }

        [Test]
        public void TryMoveDrag_BlockAtItsDoorMovedAway_Stays()
        {
            var session = new LevelSession(TwoBlockLevel(), TestLevel.ExactDrag);
            session.TryBeginDrag(BlueBlock);

            bool stays = session.TryMoveDrag(new BoardPoint(0f, 0.3f), out _);

            Assert.IsTrue(stays);
            Assert.IsTrue(session.Board.IsOnBoard(BlueBlock));
        }

        [Test]
        public void TryMoveDrag_LastBlockLeaves_WinsOnce()
        {
            var session = new LevelSession(OneBlockLevel(), TestLevel.ExactDrag);
            session.TryBeginDrag(RedBlock);
            List<SessionState> states = RecordStates(session);

            session.TryMoveDrag(RedDoor, out _);
            session.TryMoveDrag(RedDoor, out _);

            Assert.AreEqual(SessionState.Won, session.State);
            CollectionAssert.AreEqual(new[] { SessionState.Won }, states);
            Assert.IsFalse(session.TryBeginDrag(RedBlock));
        }

        [Test]
        public void Tick_AfterTheLastBlockLeftInTheSameFrame_KeepsTheWin()
        {
            var session = new LevelSession(OneBlockLevel(), TestLevel.ExactDrag);
            session.TryBeginDrag(RedBlock);
            session.Tick(9.5f);
            List<SessionState> states = RecordStates(session);

            session.TryMoveDrag(RedDoor, out _);
            session.Tick(5f);

            Assert.AreEqual(SessionState.Won, session.State);
            CollectionAssert.DoesNotContain(states, SessionState.Failed);
        }

        [Test]
        public void SecondsLeftChanged_IsRaisedOncePerShownSecond()
        {
            var session = new LevelSession(OneBlockLevel(), TestLevel.ExactDrag);
            var shownSeconds = new List<int>();
            session.SecondsLeftChanged += shownSeconds.Add;
            session.TryBeginDrag(RedBlock);

            session.Tick(0.4f);
            session.Tick(0.4f);
            session.Tick(0.4f);

            CollectionAssert.AreEqual(new[] { 9 }, shownSeconds);
        }

        [Test]
        public void NewSession_AfterAnAttempt_StartsFromTheLevelData()
        {
            LevelData level = TwoBlockLevel();
            var firstAttempt = new LevelSession(level, TestLevel.ExactDrag);
            firstAttempt.TryBeginDrag(RedBlock);
            firstAttempt.TryMoveDrag(RedDoor, out _);
            firstAttempt.Tick(4f);

            var restart = new LevelSession(level, TestLevel.ExactDrag);

            Assert.AreEqual(SessionState.Ready, restart.State);
            Assert.AreEqual(TestLevel.TimeLimitSeconds, restart.SecondsLeft);
            Assert.AreEqual(2, restart.Board.RemainingBlockCount);
            Assert.IsTrue(restart.Board.IsOnBoard(RedBlock));
            Assert.AreEqual(new Cell(1, 1), restart.Board.GetOrigin(RedBlock));
            Assert.AreEqual(new Cell(0, 0), restart.Board.GetOrigin(BlueBlock));
        }
    }
}
