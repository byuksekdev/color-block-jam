using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>One way of editing the board with the pointer. A new tool is a new class and one entry in the window's tool list.</summary>
    internal interface IBoardTool
    {
        string Label { get; }
        string Hint { get; }

        /// <summary>The tool's own choices, shown below the hint while the tool is chosen. Null for a tool without any.</summary>
        VisualElement Options { get; }

        void PointerDown(BoardHit hit);
        void PointerMove(BoardHit hit);
        void PointerUp(BoardHit hit);
        void Cancel();
    }
}
