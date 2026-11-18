namespace ColorBlockJam.Core.Validation
{
    /// <summary>The block or door a finding is about, by its place in the level's list. The default is no part: the finding is about the whole level.</summary>
    public readonly struct LevelPart
    {
        private LevelPart(LevelPartKind kind, int index)
        {
            Kind = kind;
            Index = index;
        }

        public LevelPartKind Kind { get; }
        public int Index { get; }

        public static LevelPart Block(int index)
        {
            return new LevelPart(LevelPartKind.Block, index);
        }

        public static LevelPart Door(int index)
        {
            return new LevelPart(LevelPartKind.Door, index);
        }
    }
}
