using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>The shapes the board canvas is painted with.</summary>
    internal static class PainterExtensions
    {
        private const float BlockInset = 2f;
        private const float StudRadius = 0.2f;
        private const float StudLightness = 0.22f;
        private const float ArrowLength = 0.62f;
        private const float ArrowHead = 0.16f;
        private const float ArrowLineWidth = 0.08f;

        private static readonly Color ArrowColor = new Color(1f, 1f, 1f, 0.9f);
        private static readonly Cell[] NeighbourSteps = { new Cell(-1, 0), new Cell(1, 0), new Cell(0, -1), new Cell(0, 1) };

        public static void FillRect(this Painter2D painter, Rect rect, Color color)
        {
            painter.fillColor = color;
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMax));
            painter.LineTo(new Vector2(rect.xMin, rect.yMax));
            painter.ClosePath();
            painter.Fill();
        }

        /// <summary>A block as the editor shows it: its cells joined into one shape, with a stud on each.</summary>
        public static void FillBlock(this Painter2D painter, BoardLayout layout, List<Cell> cells, Color tint)
        {
            Color studColor = Color.Lerp(tint, Color.white, StudLightness);
            studColor.a = tint.a;

            for (int i = 0; i < cells.Count; i++)
            {
                Rect rect = layout.CellRect(cells[i]);
                float left = cells.Contains(cells[i] + NeighbourSteps[0]) ? 0f : BlockInset;
                float right = cells.Contains(cells[i] + NeighbourSteps[1]) ? 0f : BlockInset;
                float bottom = cells.Contains(cells[i] + NeighbourSteps[2]) ? 0f : BlockInset;
                float top = cells.Contains(cells[i] + NeighbourSteps[3]) ? 0f : BlockInset;

                painter.FillRect(
                    Rect.MinMaxRect(rect.xMin + left, rect.yMin + top, rect.xMax - right, rect.yMax - bottom),
                    tint);

                painter.fillColor = studColor;
                painter.BeginPath();
                painter.Arc(rect.center, layout.CellSize * StudRadius, 0f, 360f);
                painter.Fill();
            }
        }

        public static void StrokeRect(this Painter2D painter, Rect rect, Color color, float width)
        {
            painter.strokeColor = color;
            painter.lineWidth = width;
            painter.lineJoin = LineJoin.Miter;
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMax));
            painter.LineTo(new Vector2(rect.xMin, rect.yMax));
            painter.ClosePath();
            painter.Stroke();
        }

        /// <summary>A line around the outer edge of a block: along every side of a cell that no other cell of the block touches.</summary>
        public static void StrokeOutline(this Painter2D painter, BoardLayout layout, List<Cell> cells, Color color, float width)
        {
            painter.strokeColor = color;
            painter.lineWidth = width;
            painter.lineCap = LineCap.Round;
            painter.BeginPath();

            for (int i = 0; i < cells.Count; i++)
            {
                Rect rect = layout.CellRect(cells[i]);
                var topLeft = new Vector2(rect.xMin, rect.yMin);
                var topRight = new Vector2(rect.xMax, rect.yMin);
                var bottomLeft = new Vector2(rect.xMin, rect.yMax);
                var bottomRight = new Vector2(rect.xMax, rect.yMax);

                AddEdgeIfOpen(painter, cells, cells[i] + NeighbourSteps[0], topLeft, bottomLeft);
                AddEdgeIfOpen(painter, cells, cells[i] + NeighbourSteps[1], topRight, bottomRight);
                AddEdgeIfOpen(painter, cells, cells[i] + NeighbourSteps[2], bottomLeft, bottomRight);
                AddEdgeIfOpen(painter, cells, cells[i] + NeighbourSteps[3], topLeft, topRight);
            }

            painter.Stroke();
        }

        /// <summary>A line with a head at each end, along the way an arrow block can move, across the whole block.</summary>
        public static void StrokeArrow(this Painter2D painter, BoardLayout layout, BlockData block)
        {
            Rect bounds = layout.CellRect(block.Cells[0]);
            for (int i = 1; i < block.Cells.Count; i++)
            {
                Rect cell = layout.CellRect(block.Cells[i]);
                bounds = Rect.MinMaxRect(
                    Mathf.Min(bounds.xMin, cell.xMin),
                    Mathf.Min(bounds.yMin, cell.yMin),
                    Mathf.Max(bounds.xMax, cell.xMax),
                    Mathf.Max(bounds.yMax, cell.yMax));
            }

            bool isHorizontal = block.Axis == BlockAxis.Horizontal;
            Vector2 along = isHorizontal ? Vector2.right : Vector2.up;
            Vector2 across = isHorizontal ? Vector2.up : Vector2.right;
            Vector2 reach = along * ((isHorizontal ? bounds.width : bounds.height) * ArrowLength / 2f);
            float head = layout.CellSize * ArrowHead;

            painter.strokeColor = ArrowColor;
            painter.lineWidth = layout.CellSize * ArrowLineWidth;
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;
            painter.BeginPath();
            painter.MoveTo(bounds.center - reach);
            painter.LineTo(bounds.center + reach);

            for (int sign = -1; sign <= 1; sign += 2)
            {
                Vector2 tip = bounds.center + reach * sign;
                Vector2 back = tip - along * (head * sign);
                painter.MoveTo(back + across * head);
                painter.LineTo(tip);
                painter.LineTo(back - across * head);
            }

            painter.Stroke();
        }

        private static void AddEdgeIfOpen(Painter2D painter, List<Cell> cells, Cell neighbour, Vector2 from, Vector2 to)
        {
            if (!cells.Contains(neighbour))
            {
                painter.MoveTo(from);
                painter.LineTo(to);
            }
        }
    }
}
