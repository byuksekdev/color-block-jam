using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Sessions;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>
    /// Turns one finger, or the mouse, into a drag of one block. It asks the session for every move
    /// and shows what the session answers.
    /// </summary>
    public sealed class BoardInput : MonoBehaviour
    {
        private const int NoBlock = -1;
        private const int NoFinger = -1;
        private const int LeftMouseButton = 0;

        [SerializeField] private Camera _camera;

        private LevelSession _session;
        private BoardView _board;
        private Plane _touchPlane;
        private int _heldBlock = NoBlock;
        private int _finger = NoFinger;
        private float _grabOffsetX;
        private float _grabOffsetY;

        public void Bind(LevelSession session, BoardView board)
        {
            _session = session;
            _board = board;
            _heldBlock = NoBlock;
            _finger = NoFinger;
            _touchPlane = new Plane(Vector3.up, board.Space.Center + Vector3.up * BoardView.BlockTop);
        }

        /// <summary>Reads this frame's input. Call it before the session's Tick.</summary>
        public void Process()
        {
            if (Input.touchCount > 0)
            {
                ProcessTouches();
            }
            else
            {
                ProcessMouse();
            }
        }

        private void ProcessTouches()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (_heldBlock == NoBlock)
                {
                    if (touch.phase == TouchPhase.Began && TryGrab(touch.position))
                    {
                        _finger = touch.fingerId;
                    }
                }
                else if (touch.fingerId == _finger)
                {
                    bool lifted = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
                    DragOrRelease(touch.position, lifted);
                }
            }
        }

        private void ProcessMouse()
        {
            if (_heldBlock == NoBlock)
            {
                if (Input.GetMouseButtonDown(LeftMouseButton))
                {
                    TryGrab(Input.mousePosition);
                }
            }
            else
            {
                DragOrRelease(Input.mousePosition, !Input.GetMouseButton(LeftMouseButton));
            }
        }

        private bool TryGrab(Vector2 screenPosition)
        {
            if (!TryGetBoardPoint(screenPosition, out BoardPoint point))
            {
                return false;
            }

            var cell = new Cell(Mathf.FloorToInt(point.X), Mathf.FloorToInt(point.Y));
            if (!_session.Board.TryGetBlockAt(cell, out int blockId) || !_session.TryBeginDrag(blockId))
            {
                return false;
            }

            Cell origin = _session.Board.GetOrigin(blockId);
            _heldBlock = blockId;
            _grabOffsetX = point.X - origin.X;
            _grabOffsetY = point.Y - origin.Y;
            return true;
        }

        private void DragOrRelease(Vector2 screenPosition, bool lifted)
        {
            BoardPoint position = default;
            bool isDragging = TryGetBoardPoint(screenPosition, out BoardPoint point)
                && _session.TryMoveDrag(new BoardPoint(point.X - _grabOffsetX, point.Y - _grabOffsetY), out position);

            if (isDragging && !lifted)
            {
                _board.GetBlock(_heldBlock).Follow(_board.BlockPosition(position));
                return;
            }

            _session.EndDrag();
            if (_session.Board.IsOnBoard(_heldBlock))
            {
                Cell origin = _session.Board.GetOrigin(_heldBlock);
                _board.GetBlock(_heldBlock).SettleAt(_board.BlockPosition(BoardPoint.FromCell(origin)));
            }

            _heldBlock = NoBlock;
            _finger = NoFinger;
        }

        private bool TryGetBoardPoint(Vector2 screenPosition, out BoardPoint point)
        {
            Ray ray = _camera.ScreenPointToRay(screenPosition);
            if (!_touchPlane.Raycast(ray, out float distance))
            {
                point = default;
                return false;
            }

            point = _board.Space.ToBoardPoint(ray.GetPoint(distance));
            return true;
        }
    }
}
