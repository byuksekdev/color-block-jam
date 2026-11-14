namespace ColorBlockJam.LevelEditor
{
    /// <summary>One way of editing the board with the pointer. A new tool is a new class and one entry in the window's tool list.</summary>
    internal interface IBoardTool
    {
        string Label { get; }
        string Hint { get; }

        void PointerDown(BoardHit hit);
        void PointerMove(BoardHit hit);
        void PointerUp(BoardHit hit);
        void Cancel();
    }
}
