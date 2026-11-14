namespace ColorBlockJam.Core.Validation
{
    public readonly struct LevelProblem
    {
        public LevelProblem(LevelProblemKind kind, string message)
        {
            Kind = kind;
            Message = message;
        }

        public LevelProblemKind Kind { get; }
        public string Message { get; }
    }
}
