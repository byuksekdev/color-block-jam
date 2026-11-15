namespace ColorBlockJam.Core.Validation
{
    public enum Solvability
    {
        Solvable,
        Unsolvable,

        /// <summary>The level has too many positions to try within the limit, so the question stays open.</summary>
        Unknown,
    }
}
