using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Validation;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>
    /// What is wrong with the level, in the designer's words. A line about one block or door marks it
    /// on the board while the pointer is over the line. A well formed level offers the solvability check.
    /// </summary>
    internal sealed class ProblemsPanel : VisualElement
    {
        private const string PointingClass = "points-at-part";
        private const string PointingHint = "Point at a line to see where it is on the board.";
        private const string UnknownAnswer = "The level is too open to check whether it can be solved: play it to make sure.";

        private readonly VisualElement _rows = new VisualElement();
        private readonly Label _hint = new Label(PointingHint);
        private readonly Button _solveButton;

        public ProblemsPanel()
        {
            AddToClassList("panel");

            var title = new Label("Level check");
            title.AddToClassList("panel-title");
            Add(title);
            Add(_rows);

            _hint.AddToClassList("hint");
            Add(_hint);

            _solveButton = new Button(() => SolveRequested?.Invoke()) { text = "Check if it can be solved" };
            Add(_solveButton);
        }

        /// <summary>The pointer came onto a line about this part, or left it: then the part is the default, no part.</summary>
        public event Action<LevelPart> PartPointed;

        public event Action SolveRequested;

        public void Refresh(IReadOnlyList<LevelProblem> problems)
        {
            _rows.Clear();
            SetShown(_solveButton, problems.Count == 0);

            if (problems.Count == 0)
            {
                AddRow("No problem found.", "problem-none", default);
            }

            for (int i = 0; i < problems.Count; i++)
            {
                AddRow(problems[i].Message, "problem", problems[i].Part);
            }

            ShowHintIfAnyLinePoints();
        }

        /// <summary>Shows the answer of the level solver in place of the check button, until the next Refresh.</summary>
        public void ShowSolverResult(SolverResult result)
        {
            SetShown(_solveButton, false);

            switch (result.Solvability)
            {
                case Solvability.Solvable:
                    AddRow("It can be solved.", "problem-none", default);
                    break;
                case Solvability.Unsolvable:
                    AddRow("It cannot be solved. " + result.Reason, "problem", result.Part);
                    break;
                default:
                    AddRow(UnknownAnswer, "hint", default);
                    break;
            }

            ShowHintIfAnyLinePoints();
        }

        private void AddRow(string text, string styleClass, LevelPart part)
        {
            var row = new Label(text);
            row.AddToClassList(styleClass);
            _rows.Add(row);

            if (part.Kind == LevelPartKind.None)
            {
                return;
            }

            row.AddToClassList(PointingClass);
            row.RegisterCallback<PointerEnterEvent>(_ => PartPointed?.Invoke(part));
            row.RegisterCallback<PointerLeaveEvent>(_ => PartPointed?.Invoke(default));
        }

        private void ShowHintIfAnyLinePoints()
        {
            bool anyLinePoints = false;
            foreach (VisualElement row in _rows.Children())
            {
                anyLinePoints |= row.ClassListContains(PointingClass);
            }

            SetShown(_hint, anyLinePoints);
        }

        private static void SetShown(VisualElement element, bool isShown)
        {
            element.style.display = isShown ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
