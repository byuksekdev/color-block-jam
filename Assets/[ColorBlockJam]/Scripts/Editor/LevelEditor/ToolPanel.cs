using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Editing;
using ColorBlockJam.Game.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>The designer's choices: which tool, which colour and which shape.</summary>
    internal sealed class ToolPanel : VisualElement
    {
        private const string SelectedClass = "selected";

        private readonly IReadOnlyList<IBoardTool> _tools;
        private readonly BlockPalette _palette;
        private readonly List<Button> _toolButtons = new List<Button>();
        private readonly List<BlockColor> _colors = new List<BlockColor>();
        private readonly List<Button> _colorButtons = new List<Button>();
        private readonly List<Button> _shapeButtons = new List<Button>();
        private readonly List<ShapePreview> _shapePreviews = new List<ShapePreview>();
        private readonly Label _hint = new Label();
        private readonly VisualElement _toolOptions = new VisualElement();
        private readonly ShapePreview _chosenShape = new ShapePreview();

        public ToolPanel(IReadOnlyList<IBoardTool> tools, BlockPalette palette)
        {
            _tools = tools;
            _palette = palette;
            AddToClassList("panel");

            AddToolButtons();
            AddColorButtons();
            AddShapeButtons();
        }

        public event Action<int> ToolChosen;
        public event Action<BlockColor> ColorChosen;
        public event Action<int> ShapeChosen;
        public event Action TurnRequested;
        public event Action MirrorRequested;

        public void Refresh(int toolIndex, BlockColor color, int shapeIndex, IReadOnlyList<Cell> shape)
        {
            Color tint = EditorTint.Of(_palette, color);

            for (int i = 0; i < _toolButtons.Count; i++)
            {
                _toolButtons[i].EnableInClassList(SelectedClass, i == toolIndex);
            }

            for (int i = 0; i < _colorButtons.Count; i++)
            {
                _colorButtons[i].EnableInClassList(SelectedClass, _colors[i] == color);
            }

            for (int i = 0; i < _shapeButtons.Count; i++)
            {
                _shapeButtons[i].EnableInClassList(SelectedClass, i == shapeIndex);
                _shapePreviews[i].Show(BlockShapes.All[i].Cells, tint);
            }

            _hint.text = _tools[toolIndex].Hint;
            ShowOptions(_tools[toolIndex].Options);
            _chosenShape.Show(shape, tint);
        }

        private void AddToolButtons()
        {
            VisualElement row = AddSection("Tool", "wrap-row");
            for (int i = 0; i < _tools.Count; i++)
            {
                int toolIndex = i;
                var button = new Button(() => ToolChosen?.Invoke(toolIndex)) { text = _tools[i].Label };
                button.AddToClassList("tool-button");
                _toolButtons.Add(button);
                row.Add(button);
            }

            _hint.AddToClassList("hint");
            Add(_hint);
            Add(_toolOptions);
        }

        private void ShowOptions(VisualElement options)
        {
            if (options != null && options.parent == _toolOptions)
            {
                return;
            }

            _toolOptions.Clear();
            if (options != null)
            {
                _toolOptions.Add(options);
            }
        }

        private void AddColorButtons()
        {
            VisualElement row = AddSection("Colour", "wrap-row");
            foreach (BlockColor color in Enum.GetValues(typeof(BlockColor)))
            {
                if (color == BlockColor.None)
                {
                    continue;
                }

                BlockColor chosen = color;
                var button = new Button(() => ColorChosen?.Invoke(chosen)) { tooltip = color.ToString() };
                button.AddToClassList("swatch");
                button.style.backgroundColor = EditorTint.Of(_palette, color);
                _colors.Add(color);
                _colorButtons.Add(button);
                row.Add(button);
            }
        }

        private void AddShapeButtons()
        {
            VisualElement row = AddSection("Shape", "wrap-row");
            for (int i = 0; i < BlockShapes.All.Count; i++)
            {
                int shapeIndex = i;
                var preview = new ShapePreview();
                var button = new Button(() => ShapeChosen?.Invoke(shapeIndex)) { tooltip = BlockShapes.All[i].Name };
                button.AddToClassList("shape-button");
                button.Add(preview);
                _shapePreviews.Add(preview);
                _shapeButtons.Add(button);
                row.Add(button);
            }

            var chosenRow = new VisualElement();
            chosenRow.AddToClassList("button-row");
            _chosenShape.AddToClassList("chosen-shape");
            chosenRow.Add(_chosenShape);
            chosenRow.Add(new Button(() => TurnRequested?.Invoke()) { text = "Turn (R)" });
            chosenRow.Add(new Button(() => MirrorRequested?.Invoke()) { text = "Mirror (F)" });
            Add(chosenRow);
        }

        private VisualElement AddSection(string title, string rowClass)
        {
            var label = new Label(title);
            label.AddToClassList("panel-title");
            Add(label);

            var row = new VisualElement();
            row.AddToClassList(rowClass);
            Add(row);
            return row;
        }
    }
}
