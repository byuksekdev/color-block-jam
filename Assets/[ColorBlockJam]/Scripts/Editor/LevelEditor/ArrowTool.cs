using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Editing;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>Makes a block an arrow block, which moves only left and right or only up and down, or sets it free again.</summary>
    internal sealed class ArrowTool : IBoardTool
    {
        private const string SelectedClass = "selected";

        private static readonly BlockAxis[] Axes = { BlockAxis.Horizontal, BlockAxis.Vertical, BlockAxis.Any };
        private static readonly string[] AxisLabels = { "Left and right", "Up and down", "Any way" };

        private readonly IEditingContext _context;
        private readonly VisualElement _options = new VisualElement();
        private readonly List<Button> _axisButtons = new List<Button>();
        private BlockAxis _axis = BlockAxis.Horizontal;

        public ArrowTool(IEditingContext context)
        {
            _context = context;
            _options.AddToClassList("button-row");

            for (int i = 0; i < Axes.Length; i++)
            {
                BlockAxis axis = Axes[i];
                var button = new Button(() => ChooseAxis(axis)) { text = AxisLabels[i] };
                button.AddToClassList("tool-button");
                _axisButtons.Add(button);
                _options.Add(button);
            }

            ChooseAxis(_axis);
        }

        public string Label => "Arrow";
        public string Hint => "Click a block to let it move only the chosen way. Choose Any way to set it free again.";
        public VisualElement Options => _options;

        public void PointerDown(BoardHit hit)
        {
            if (hit.Kind != BoardHitKind.Cell)
            {
                return;
            }

            int block = LevelEditing.FindBlockAt(_context.Level, hit.Cell);
            if (block != LevelEditing.NotFound)
            {
                BlockAxis axis = _axis;
                _context.Edit("Set Block Arrow", level => LevelEditing.SetBlockAxis(level, block, axis));
            }
        }

        public void PointerMove(BoardHit hit)
        {
        }

        public void PointerUp(BoardHit hit)
        {
        }

        public void Cancel()
        {
        }

        private void ChooseAxis(BlockAxis axis)
        {
            _axis = axis;
            for (int i = 0; i < _axisButtons.Count; i++)
            {
                _axisButtons[i].EnableInClassList(SelectedClass, Axes[i] == axis);
            }
        }
    }
}
