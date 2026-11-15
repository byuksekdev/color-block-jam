namespace ColorBlockJam.Core.Validation
{
    public enum LevelProblemKind
    {
        UnsupportedVersion,
        BoardTooSmall,
        TimeLimitNotPositive,
        NoBlocks,
        BlockHasNoColor,
        BlockHasNoCells,
        BlockOutsideBoard,
        BlocksOverlap,
        BlockColorWithoutDoor,
        DoorHasNoColor,
        DoorOutsideSide,
        DoorsOverlap,
        IceCountNegative,
    }
}
