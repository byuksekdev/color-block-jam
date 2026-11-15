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
        private readonly DragRules _dragRules;
        private BlockDrag _drag;
        private SessionState _stateBeforePause;
        private int _announcedSecondsLeft;

        public LevelSession(LevelData level, DragRules dragRules)
        {
            List<LevelProblem> problems = LevelValidator.Validate(level);
            if (problems.Count > 0)
            {
                throw new ArgumentException("The level cannot be played: " + problems[0].Message, nameof(level));
            }

            _board = new BoardState(level);
            _timer = new LevelTimer(level.TimeLimitSeconds);
            _dragRules = dragRules;
            _announcedSecondsLeft = _timer.SecondsLeft;
            State = SessionState.Ready;
        }

        public event Action<SessionState> StateChanged;
        public event Action<int> SecondsLeftChanged;
        /// <summary>A block left the board: the block, the cell its origin stood on, and the door it left through.</summary>
        public event Action<Block, Cell, Door> BlockExited;

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

            _drag = new BlockDrag(_board, blockId, _dragRules.BlockOverlap);
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

            BoardPoint start = _drag.Position;
            bool enteredCell;
            do
            {
                enteredCell = _drag.MoveTowards(target);
                if (IsTakenByDoor(start, target, enteredCell, out Door door))
                {
                    ExitDraggedBlock(door);
                    return false;
                }
            }
            while (enteredCell);

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

        /// <summary>
        /// A door takes the dragged block when the block may leave through it and is pressed towards it,
        /// or has just come within the door's reach. A block that only rests near its door stays.
        /// </summary>
        private bool IsTakenByDoor(BoardPoint start, BoardPoint target, bool enteredCell, out Door door)
        {
            if (!ExitRule.TryFindExit(_board, _drag.BlockId, out door))
            {
                return false;
            }

            if (_drag.IsPushedAgainst(door.Side, target))
            {
                return true;
            }

            float distance = _drag.DistanceFrom(door.Side, _drag.Position);
            bool cameCloser = enteredCell || distance < _drag.DistanceFrom(door.Side, start);
            return cameCloser && distance <= _dragRules.DoorReach;
        }

        private void ExitDraggedBlock(Door door)
        {
            Block block = _board.Blocks[_drag.BlockId];
            Cell origin = _board.GetOrigin(block.Id);
            EndDrag();
            _board.RemoveBlock(block.Id);
            BlockExited?.Invoke(block, origin, door);

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
