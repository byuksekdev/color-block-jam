using System;

namespace ColorBlockJam.Core.Data
{
    /// <summary>A door on one side of the board. Start is the first cell it covers along that side.</summary>
    [Serializable]
    public sealed class DoorData
    {
        public BlockColor Color;
        public BoardSide Side;
        public int Start;
        public int Length;

        /// <summary>How many blocks have to leave the level before this door lets a block out. 0 for a door without ice.</summary>
        public int IceCount;
    }
}
