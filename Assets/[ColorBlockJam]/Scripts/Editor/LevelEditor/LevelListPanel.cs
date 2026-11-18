using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>The levels in play order, with the drafts after them. Dragging a row changes the order.</summary>
    internal sealed class LevelListPanel : VisualElement
    {
        private const float RowHeight = 24f;
        private const float ListHeight = 150f;

        private readonly List<string> _names = new List<string>();
        private readonly ListView _list;
        private readonly Button _duplicateButton;
        private readonly Button _deleteButton;

        public LevelListPanel()
        {
            AddToClassList("panel");

            var title = new Label("Levels");
            title.AddToClassList("panel-title");
            Add(title);

            _list = new ListView(_names, RowHeight, () => new Label(), BindRow)
            {
                selectionType = SelectionType.Single,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
            };
            _list.style.height = ListHeight;
            _list.selectionChanged += _ => LevelChosen?.Invoke(_list.selectedIndex);
            _list.itemIndexChanged += (from, to) => LevelMoved?.Invoke(from, to);
            Add(_list);

            var buttons = new VisualElement();
            buttons.AddToClassList("button-row");
            buttons.Add(new Button(() => NewRequested?.Invoke()) { text = "New Level" });
            _duplicateButton = new Button(() => DuplicateRequested?.Invoke()) { text = "Duplicate" };
            buttons.Add(_duplicateButton);
            _deleteButton = new Button(() => DeleteRequested?.Invoke()) { text = "Delete" };
            buttons.Add(_deleteButton);
            Add(buttons);
        }

        public event Action<int> LevelChosen;
        public event Action<int, int> LevelMoved;
        public event Action NewRequested;
        public event Action DuplicateRequested;
        public event Action DeleteRequested;

        public void Refresh(IReadOnlyList<string> names, int selectedIndex)
        {
            _names.Clear();
            _names.AddRange(names);
            _list.Rebuild();

            bool hasSelection = selectedIndex >= 0;
            _list.SetSelectionWithoutNotify(hasSelection ? new[] { selectedIndex } : Array.Empty<int>());
            _duplicateButton.SetEnabled(hasSelection);
            _deleteButton.SetEnabled(hasSelection);
        }

        private void BindRow(VisualElement row, int index)
        {
            string number = (index + 1).ToString(CultureInfo.InvariantCulture);
            ((Label)row).text = number + ".  " + _names[index];
        }
    }
}
