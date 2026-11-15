using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Editing;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    internal sealed class MoveTool : IBoardTool
    {
        private readonly IEditingContext _context;
        private int _block = LevelEditing.NotFound;
        private Cell _grabbedCell;

        public MoveTool(IEditingContext context)
        {
            _context = context;
        }

        public string Label => "Move";
        public string Hint => "Drag a block to another place on the board.";
        public VisualElement Options => null;

        public void PointerDown(BoardHit hit)
        {
            if (hit.Kind != BoardHitKind.Cell)
            {
                return;
            }

            _block = LevelEditing.FindBlockAt(_context.Level, hit.Cell);
            _grabbedCell = hit.Cell;
        }

        public void PointerMove(BoardHit hit)
        {
            if (_block == LevelEditing.NotFound)
            {
                return;
            }

            BlockData block = _context.Level.Blocks[_block];
            Cell offset = hit.Cell - _grabbedCell;
            _context.Canvas.ShowBlockPreview(
                block.Cells,
                offset,
                block.Color,
                LevelEditing.CanPlaceBlock(_context.Level, block.Cells, offset, _block),
                _block);
        }

        public void PointerUp(BoardHit hit)
        {
            if (_block == LevelEditing.NotFound)
            {
                return;
            }

            int block = _block;
            Cell offset = hit.Cell - _grabbedCell;
            Cancel();

            if (offset != new Cell(0, 0))
            {
                _context.Edit("Move Block", level => LevelEditing.TryMoveBlock(level, block, offset));
            }
        }

        public void Cancel()
        {
            _block = LevelEditing.NotFound;
            _context.Canvas.ClearPreview();
        }
    }
}
