using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Editing;

namespace ColorBlockJam.LevelEditor
{
    internal sealed class DoorTool : IBoardTool
    {
        private readonly IEditingContext _context;
        private bool _isPainting;
        private BoardSide _side;
        private int _from;

        public DoorTool(IEditingContext context)
        {
            _context = context;
        }

        public string Label => "Door";
        public string Hint => "Drag along the strip beside the board to place a door of the chosen colour.";

        public void PointerDown(BoardHit hit)
        {
            if (hit.Kind != BoardHitKind.DoorSlot)
            {
                return;
            }

            _isPainting = true;
            _side = hit.Side;
            _from = hit.Position;
            _context.Canvas.ShowDoorPreview(_context.Color, _side, _from, _from);
        }

        public void PointerMove(BoardHit hit)
        {
            if (_isPainting)
            {
                _context.Canvas.ShowDoorPreview(_context.Color, _side, _from, PositionAlongSide(hit));
            }
            else if (hit.Kind == BoardHitKind.DoorSlot)
            {
                _context.Canvas.ShowDoorPreview(_context.Color, hit.Side, hit.Position, hit.Position);
            }
            else
            {
                _context.Canvas.ClearPreview();
            }
        }

        public void PointerUp(BoardHit hit)
        {
            if (!_isPainting)
            {
                return;
            }

            _isPainting = false;
            BoardSide side = _side;
            int from = _from;
            int to = PositionAlongSide(hit);
            _context.Canvas.ClearPreview();
            _context.Edit("Place Door", level => LevelEditing.PaintDoor(level, _context.Color, side, from, to));
        }

        public void Cancel()
        {
            _isPainting = false;
            _context.Canvas.ClearPreview();
        }

        private int PositionAlongSide(BoardHit hit)
        {
            return _side.RunsAlongX() ? hit.Cell.X : hit.Cell.Y;
        }
    }
}
