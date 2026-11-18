namespace ColorBlockJam.Core.Validation
{
    public readonly struct LevelProblem
    {
        public LevelProblem(LevelProblemKind kind, string message, LevelPart part = default)
        {
            Kind = kind;
            Message = message;
            Part = part;
        }

        public LevelProblemKind Kind { get; }
        public string Message { get; }

        /// <summary>The block or door the problem is about, so the editor can point at it.</summary>
        public LevelPart Part { get; }
    }
}
