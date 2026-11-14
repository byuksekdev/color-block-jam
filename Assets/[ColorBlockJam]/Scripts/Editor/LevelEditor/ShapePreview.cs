using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>A small picture of a block shape.</summary>
    internal sealed class ShapePreview : VisualElement
    {
        private const float Padding = 4f;
        private const float CellGap = 1f;
        private const int SmallestSpan = 3;

        private IReadOnlyList<Cell> _cells;
        private Color _color;

        public ShapePreview()
        {
            AddToClassList("shape-preview");
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        public void Show(IReadOnlyList<Cell> cells, Color color)
        {
            _cells = cells;
            _color = color;
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext context)
        {
            if (_cells == null || _cells.Count == 0)
            {
                return;
            }

            int width = 0;
            int height = 0;
            for (int i = 0; i < _cells.Count; i++)
            {
                width = Mathf.Max(width, _cells[i].X + 1);
                height = Mathf.Max(height, _cells[i].Y + 1);
            }

            Rect area = contentRect;
            int span = Mathf.Max(SmallestSpan, Mathf.Max(width, height));
            float cellSize = Mathf.Floor((Mathf.Min(area.width, area.height) - 2f * Padding) / span);
            var origin = new Vector2(
                Mathf.Round(area.center.x - cellSize * width / 2f),
                Mathf.Round(area.center.y - cellSize * height / 2f));

            Painter2D painter = context.painter2D;
            for (int i = 0; i < _cells.Count; i++)
            {
                var rect = new Rect(
                    origin.x + _cells[i].X * cellSize + CellGap,
                    origin.y + (height - 1 - _cells[i].Y) * cellSize + CellGap,
                    cellSize - 2f * CellGap,
                    cellSize - 2f * CellGap);
                painter.FillRect(rect, _color);
            }
        }
    }
}
