namespace ColorBlockJam.Core.Validation
{
    public readonly struct SolverResult
    {
        public SolverResult(Solvability solvability, string reason)
        {
            Solvability = solvability;
            Reason = reason;
        }

        public Solvability Solvability { get; }

        /// <summary>Why the level cannot be solved, in words for the designer. Empty for any other answer.</summary>
        public string Reason { get; }
    }
}
