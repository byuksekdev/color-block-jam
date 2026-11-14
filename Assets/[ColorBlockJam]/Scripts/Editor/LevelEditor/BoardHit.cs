using ColorBlockJam.Core.Data;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>What lies under the pointer on the board canvas.</summary>
    internal readonly struct BoardHit
    {
        public BoardHit(BoardHitKind kind, Cell cell, BoardSide side, int position)
        {
            Kind = kind;
            Cell = cell;
            Side = side;
            Position = position;
        }

        public BoardHitKind Kind { get; }

        /// <summary>The grid cell under the pointer. It lies outside the board when Kind is not Cell.</summary>
        public Cell Cell { get; }

        /// <summary>The side and the position along it, when Kind is DoorSlot.</summary>
        public BoardSide Side { get; }

        public int Position { get; }
    }
}
