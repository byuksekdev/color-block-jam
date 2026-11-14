using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>What a board tool may read and do: the open level, the designer's choices, and one way to change the level.</summary>
    internal interface IEditingContext
    {
        LevelData Level { get; }
        BlockColor Color { get; }
        IReadOnlyList<Cell> Shape { get; }
        BoardCanvas Canvas { get; }

        /// <summary>Changes the level as one step that the designer can undo.</summary>
        void Edit(string undoName, Action<LevelData> change);
    }
}
