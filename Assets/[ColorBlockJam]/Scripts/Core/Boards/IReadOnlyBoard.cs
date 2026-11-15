using System.Collections.Generic;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Boards
{
    public interface IReadOnlyBoard
    {
        int Width { get; }
        int Height { get; }

        /// <summary>Every block of the level, indexed by block id. A block that has left stays in the list.</summary>
        IReadOnlyList<Block> Blocks { get; }

        IReadOnlyList<Door> Doors { get; }
        int RemainingBlockCount { get; }

        bool Contains(Cell cell);
        bool IsOnBoard(int blockId);
        Cell GetOrigin(int blockId);
        bool TryGetBlockAt(Cell cell, out int blockId);

        /// <summary>How many more blocks have to leave before the ice on this block melts. Zero for a block that can move.</summary>
        int IceLeft(Block block);

        /// <summary>How many more blocks have to leave before the ice on this door melts. Zero for a door that works.</summary>
        int IceLeft(Door door);
    }
}
