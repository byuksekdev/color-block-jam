using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Editing;
using ColorBlockJam.Game.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>Draws the level and reports what the pointer touches. It changes nothing by itself.</summary>
    internal sealed class BoardCanvas : VisualElement
    {
        private const float CellGap = 1f;
        private const float BlockInset = 2f;
        private const float StudRadius = 0.2f;
        private const float StudLightness = 0.22f;
        private const float DoorThickness = 0.45f;
        private const float DoorSlotThickness = 1f;
        private const float PreviewAlpha = 0.65f;
        private const float FadedAlpha = 0.25f;

        private static readonly Color BoardColor = new Color(0.17f, 0.2f, 0.29f);
        private static readonly Color CellColor = new Color(0.36f, 0.41f, 0.53f);
        private static readonly Color DoorSlotColor = new Color(1f, 1f, 1f, 0.07f);
        private static readonly Color InvalidColor = new Color(0.9f, 0.15f, 0.15f);
        private static readonly Cell[] NeighbourSteps = { new Cell(-1, 0), new Cell(1, 0), new Cell(0, -1), new Cell(0, 1) };

        private readonly List<Cell> _previewCells = new List<Cell>();
        private LevelData _level;
        private BlockPalette _palette;
        private Color _previewColor;
        private DoorData _previewDoor;
        private int _fadedBlock = LevelEditing.NotFound;

        public BoardCanvas()
        {
            AddToClassList("board-canvas");
            focusable = true;
            generateVisualContent += Draw;
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        public event Action<BoardHit, int> PointerPressed;
        public event Action<BoardHit> PointerMoved;
        public event Action<BoardHit> PointerReleased;
        public event Action PointerLeft;

        public void Show(LevelData level, BlockPalette palette)
        {
            _level = level;
            _palette = palette;
            MarkDirtyRepaint();
        }

        /// <summary>Shows a block that is about to be placed. fadedBlock is drawn faintly, for a block that is being moved.</summary>
        public void ShowBlockPreview(
            IReadOnlyList<Cell> shape,
            Cell origin,
            BlockColor color,
            bool isValid,
            int fadedBlock = LevelEditing.NotFound)
        {
            ClearPreview();
            for (int i = 0; i < shape.Count; i++)
            {
                _previewCells.Add(origin + shape[i]);
            }

            _previewColor = isValid ? EditorTint.Of(_palette, color) : InvalidColor;
            _fadedBlock = fadedBlock;
        }

        public void ShowDoorPreview(BlockColor color, BoardSide side, int from, int to)
        {
            ClearPreview();
            int lastPosition = LevelEditing.SideLength(_level, side) - 1;
            int start = Mathf.Clamp(Mathf.Min(from, to), 0, lastPosition);
            int end = Mathf.Clamp(Mathf.Max(from, to), 0, lastPosition);
            _previewDoor = new DoorData { Color = color, Side = side, Start = start, Length = end - start + 1 };
        }

        public void ClearPreview()
        {
            _previewCells.Clear();
            _previewDoor = null;
            _fadedBlock = LevelEditing.NotFound;
            MarkDirtyRepaint();
        }

        private bool TryGetLayout(out BoardLayout layout)
        {
            layout = default;
            if (_level == null)
            {
                return false;
            }

            layout = new BoardLayout(contentRect, _level.Width, _level.Height);
            return layout.HasRoom;
        }

        private void OnPointerDown(PointerDownEvent pointerEvent)
        {
            if (TryGetLayout(out BoardLayout layout))
            {
                this.CapturePointer(pointerEvent.pointerId);
                PointerPressed?.Invoke(layout.HitTest(pointerEvent.localPosition), pointerEvent.button);
            }
        }

        private void OnPointerMove(PointerMoveEvent pointerEvent)
        {
            if (TryGetLayout(out BoardLayout layout))
            {
                PointerMoved?.Invoke(layout.HitTest(pointerEvent.localPosition));
            }
        }

        private void OnPointerUp(PointerUpEvent pointerEvent)
        {
            if (!this.HasPointerCapture(pointerEvent.pointerId))
            {
                return;
            }

            this.ReleasePointer(pointerEvent.pointerId);
            if (TryGetLayout(out BoardLayout layout))
            {
                PointerReleased?.Invoke(layout.HitTest(pointerEvent.localPosition));
            }
        }

        private void OnPointerLeave(PointerLeaveEvent pointerEvent)
        {
            if (!this.HasPointerCapture(pointerEvent.pointerId))
            {
                PointerLeft?.Invoke();
            }
        }

        private void Draw(MeshGenerationContext context)
        {
            if (!TryGetLayout(out BoardLayout layout))
            {
                return;
            }

            Painter2D painter = context.painter2D;
            DrawBoard(painter, layout);
            DrawDoors(painter, layout);
            DrawBlocks(painter, layout);
            DrawPreview(painter, layout);
        }

        private void DrawBoard(Painter2D painter, BoardLayout layout)
        {
            painter.FillRect(layout.BoardRect, BoardColor);

            for (int y = 0; y < _level.Height; y++)
            {
                for (int x = 0; x < _level.Width; x++)
                {
                    painter.FillRect(Shrink(layout.CellRect(new Cell(x, y)), CellGap), CellColor);
                }
            }

            foreach (BoardSide side in Enum.GetValues(typeof(BoardSide)))
            {
                int length = LevelEditing.SideLength(_level, side);
                for (int position = 0; position < length; position++)
                {
                    painter.FillRect(Shrink(layout.DoorRect(side, position, 1, DoorSlotThickness), CellGap), DoorSlotColor);
                }
            }
        }

        private void DrawDoors(Painter2D painter, BoardLayout layout)
        {
            for (int i = 0; i < _level.Doors.Count; i++)
            {
                DrawDoor(painter, layout, _level.Doors[i], 1f);
            }
        }

        private void DrawDoor(Painter2D painter, BoardLayout layout, DoorData door, float alpha)
        {
            Color tint = EditorTint.Of(_palette, door.Color);
            tint.a = alpha;
            painter.FillRect(layout.DoorRect(door.Side, door.Start, door.Length, DoorThickness), tint);
        }

        private void DrawBlocks(Painter2D painter, BoardLayout layout)
        {
            for (int i = 0; i < _level.Blocks.Count; i++)
            {
                BlockData block = _level.Blocks[i];
                Color tint = EditorTint.Of(_palette, block.Color);
                tint.a = i == _fadedBlock ? FadedAlpha : 1f;
                DrawBlock(painter, layout, block.Cells, tint);
            }
        }

        private void DrawPreview(Painter2D painter, BoardLayout layout)
        {
            if (_previewCells.Count > 0)
            {
                Color tint = _previewColor;
                tint.a = PreviewAlpha;
                DrawBlock(painter, layout, _previewCells, tint);
            }

            if (_previewDoor != null)
            {
                DrawDoor(painter, layout, _previewDoor, PreviewAlpha);
            }
        }

        private static void DrawBlock(Painter2D painter, BoardLayout layout, List<Cell> cells, Color tint)
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

        private static Rect Shrink(Rect rect, float amount)
        {
            return Rect.MinMaxRect(rect.xMin + amount, rect.yMin + amount, rect.xMax - amount, rect.yMax - amount);
        }
    }
}
