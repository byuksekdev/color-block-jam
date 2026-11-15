using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Boards
{
    public sealed class Door
    {
        public Door(BlockColor color, BoardSide side, int start, int length, int iceCount)
        {
            Color = color;
            Side = side;
            Start = start;
            Length = length;
            IceCount = iceCount;
        }

        public BlockColor Color { get; }
        public BoardSide Side { get; }
        public int Start { get; }
        public int Length { get; }

        /// <summary>How many blocks had to leave before this door worked, when the level began. The board tells how many are still needed.</summary>
        public int IceCount { get; }

        /// <summary>True when the door covers this column (top and bottom doors) or row (left and right doors).</summary>
        public bool Spans(int index)
        {
            return index >= Start && index < Start + Length;
        }
    }
}
