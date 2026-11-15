using System.Collections.Generic;
using System.Globalization;
using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>
    /// The ice counts written over the frozen blocks and doors of the board canvas. The canvas paints
    /// shapes; a number needs a label, so the labels lie over the canvas and let the pointer through.
    /// </summary>
    internal sealed class IceCountLabels : VisualElement
    {
        private const float FontSizeInCells = 0.45f;

        private readonly List<Label> _labels = new List<Label>();

        public IceCountLabels()
        {
            AddToClassList("ice-counts");
            pickingMode = PickingMode.Ignore;
        }

        public void Show(LevelData level, BoardLayout layout)
        {
            int used = 0;
            for (int i = 0; i < level.Blocks.Count; i++)
            {
                BlockData block = level.Blocks[i];
                if (block.IceCount > 0 && block.Cells.Count > 0)
                {
                    Place(used++, block.IceCount, layout.RectAround(BoardPoint.MiddleOf(block.Cells)), layout.CellSize);
                }
            }

            for (int i = 0; i < level.Doors.Count; i++)
            {
                DoorData door = level.Doors[i];
                if (door.IceCount > 0)
                {
                    Place(used++, door.IceCount, layout.DoorRect(door.Side, door.Start, door.Length, 1f), layout.CellSize);
                }
            }

            HideFrom(used);
        }

        public void HideAll()
        {
            HideFrom(0);
        }

        private void Place(int index, int count, Rect rect, float cellSize)
        {
            if (index == _labels.Count)
            {
                var created = new Label { pickingMode = PickingMode.Ignore };
                created.AddToClassList("ice-count");
                _labels.Add(created);
                Add(created);
            }

            Label label = _labels[index];
            label.text = count.ToString(CultureInfo.InvariantCulture);
            label.style.left = rect.xMin;
            label.style.top = rect.yMin;
            label.style.width = rect.width;
            label.style.height = rect.height;
            label.style.fontSize = cellSize * FontSizeInCells;
            label.style.display = DisplayStyle.Flex;
        }

        private void HideFrom(int index)
        {
            for (int i = index; i < _labels.Count; i++)
            {
                _labels[i].style.display = DisplayStyle.None;
            }
        }
    }
}
