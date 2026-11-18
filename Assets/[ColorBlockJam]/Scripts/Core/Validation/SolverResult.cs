namespace ColorBlockJam.Core.Validation
{
    public readonly struct SolverResult
    {
        public SolverResult(Solvability solvability, string reason, LevelPart part = default)
        {
            Solvability = solvability;
            Reason = reason;
            Part = part;
        }

        public Solvability Solvability { get; }

        /// <summary>Why the level cannot be solved, in words for the designer. Empty for any other answer.</summary>
        public string Reason { get; }

        /// <summary>The block the reason is about, when it is about one block.</summary>
        public LevelPart Part { get; }
    }
}
