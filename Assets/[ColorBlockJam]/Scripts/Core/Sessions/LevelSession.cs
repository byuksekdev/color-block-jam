using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Validation;

namespace ColorBlockJam.Core.Sessions
{
    /// <summary>
    /// One attempt at a level: the board, the timer and the state of play.
    /// A restart builds a new session from the same level data.
    /// </summary>
    public sealed class LevelSession
    {
        private readonly BoardState _board;
        private readonly LevelTimer _timer;
        private BlockDrag _drag;
        private SessionState _stateBeforePause;
        private int _announcedSecondsLeft;

        public LevelSession(LevelData level)
        {
            List<LevelProblem> problems = LevelValidator.Validate(level);
            if (problems.Count > 0)
            {
                throw new ArgumentException("The level cannot be played: " + problems[0].Message, nameof(level));
            }

            _board = new BoardState(level);
            _timer = new LevelTimer(level.TimeLimitSeconds);
            _announcedSecondsLeft = _timer.SecondsLeft;
            State = SessionState.Ready;
        }

        public event Action<SessionState> StateChanged;
        public event Action<int> SecondsLeftChanged;
        public event Action<Block, Door> BlockExited;

        public IReadOnlyBoard Board => _board;
        public SessionState State { get; private set; }
        public int SecondsLeft => _timer.SecondsLeft;

        /// <summary>Advances the timer. Call it after the frame's input, so that a win in the same frame stands.</summary>
        public void Tick(float deltaSeconds)
        {
            if (State != SessionState.Playing)
            {
                return;
            }

            _timer.Advance(deltaSeconds);
            if (_timer.SecondsLeft != _announcedSecondsLeft)
            {
                _announcedSecondsLeft = _timer.SecondsLeft;
                SecondsLeftChanged?.Invoke(_announcedSecondsLeft);
            }

            if (_timer.IsExpired)
            {
                EndDrag();
                SetState(SessionState.Failed);
            }
        }

        /// <summary>Starts dragging a block. The first drag of the session starts the timer.</summary>
        public bool TryBeginDrag(int blockId)
        {
            bool canPlay = State == SessionState.Ready || State == SessionState.Playing;
            if (!canPlay || _drag != null || !_board.IsOnBoard(blockId))
            {
                return false;
            }

            _drag = new BlockDrag(_board, blockId);
            SetState(SessionState.Playing);
            return true;
        }

        /// <summary>
        /// Moves the dragged block towards the target and reports where it is now.
        /// Returns false when no block is being dragged, or when the block left the board during this move.
        /// </summary>
        public bool TryMoveDrag(BoardPoint target, out BoardPoint position)
        {
            position = default;
            if (_drag == null)
            {
                return false;
            }

            while (_drag.MoveTowards(target))
            {
                if (ExitRule.TryFindExit(_board, _drag.BlockId, out Door reachedDoor))
                {
                    ExitDraggedBlock(reachedDoor);
                    return false;
                }
            }

            if (ExitRule.TryFindExit(_board, _drag.BlockId, out Door door) && _drag.IsPushedAgainst(door.Side, target))
            {
                ExitDraggedBlock(door);
                return false;
            }

            position = _drag.Position;
            return true;
        }

        /// <summary>Releases the dragged block. It rests on the cell the board reports for it.</summary>
        public void EndDrag()
        {
            _drag = null;
        }

        public void Pause()
        {
            if (State != SessionState.Ready && State != SessionState.Playing)
            {
                return;
            }

            _stateBeforePause = State;
            EndDrag();
            SetState(SessionState.Paused);
        }

        public void Resume()
        {
            if (State == SessionState.Paused)
            {
                SetState(_stateBeforePause);
            }
        }

        private void ExitDraggedBlock(Door door)
        {
            Block block = _board.Blocks[_drag.BlockId];
            EndDrag();
            _board.RemoveBlock(block.Id);
            BlockExited?.Invoke(block, door);

            if (_board.RemainingBlockCount == 0)
            {
                SetState(SessionState.Won);
            }
        }

        private void SetState(SessionState state)
        {
            if (State == state)
            {
                return;
            }

            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
