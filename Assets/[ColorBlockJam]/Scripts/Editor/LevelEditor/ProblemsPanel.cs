using System.Collections.Generic;
using ColorBlockJam.Core.Validation;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>What is wrong with the level, in the designer's words.</summary>
    internal sealed class ProblemsPanel : VisualElement
    {
        private readonly VisualElement _rows = new VisualElement();

        public ProblemsPanel()
        {
            AddToClassList("panel");

            var title = new Label("Level check");
            title.AddToClassList("panel-title");
            Add(title);
            Add(_rows);
        }

        public void Refresh(IReadOnlyList<LevelProblem> problems)
        {
            _rows.Clear();

            if (problems.Count == 0)
            {
                var allGood = new Label("No problem found.");
                allGood.AddToClassList("problem-none");
                _rows.Add(allGood);
                return;
            }

            for (int i = 0; i < problems.Count; i++)
            {
                var row = new Label(problems[i].Message);
                row.AddToClassList("problem");
                _rows.Add(row);
            }
        }
    }
}
