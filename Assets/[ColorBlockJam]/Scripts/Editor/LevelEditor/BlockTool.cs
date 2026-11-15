using ColorBlockJam.Core.Editing;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    internal sealed class BlockTool : IBoardTool
    {
        private readonly IEditingContext _context;

        public BlockTool(IEditingContext context)
        {
            _context = context;
        }

        public string Label => "Block";
        public string Hint => "Click a cell to place the chosen shape. R turns it, F mirrors it.";
        public VisualElement Options => null;

        public void PointerDown(BoardHit hit)
        {
            if (!LevelEditing.CanPlaceBlock(_context.Level, _context.Shape, hit.Cell))
            {
                return;
            }

            _context.Edit(
                "Place Block",
                level => LevelEditing.TryPlaceBlock(level, _context.Color, _context.Shape, hit.Cell));
        }

        public void PointerMove(BoardHit hit)
        {
            if (hit.Kind != BoardHitKind.Cell)
            {
                _context.Canvas.ClearPreview();
                return;
            }

            _context.Canvas.ShowBlockPreview(
                _context.Shape,
                hit.Cell,
                _context.Color,
                LevelEditing.CanPlaceBlock(_context.Level, _context.Shape, hit.Cell));
        }

        public void PointerUp(BoardHit hit)
        {
        }

        public void Cancel()
        {
            _context.Canvas.ClearPreview();
        }
    }
}
