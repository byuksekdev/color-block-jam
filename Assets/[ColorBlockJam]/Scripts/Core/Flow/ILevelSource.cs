using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Flow
{
    /// <summary>The levels of the game in play order.</summary>
    public interface ILevelSource
    {
        int Count { get; }

        LevelData Load(int index);
    }
}
