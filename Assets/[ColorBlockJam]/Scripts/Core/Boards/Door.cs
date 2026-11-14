using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Boards
{
    public sealed class Door
    {
        public Door(BlockColor color, BoardSide side, int start, int length)
        {
            Color = color;
            Side = side;
            Start = start;
            Length = length;
        }

        public BlockColor Color { get; }
        public BoardSide Side { get; }
        public int Start { get; }
        public int Length { get; }

        /// <summary>True when the door covers this column (top and bottom doors) or row (left and right doors).</summary>
        public bool Spans(int index)
        {
            return index >= Start && index < Start + Length;
        }
    }
}
