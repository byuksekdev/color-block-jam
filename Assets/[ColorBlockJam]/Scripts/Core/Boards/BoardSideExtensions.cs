using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Boards
{
    public static class BoardSideExtensions
    {
        /// <summary>True for the top and bottom sides, whose doors run along the X axis.</summary>
        public static bool RunsAlongX(this BoardSide side)
        {
            return side == BoardSide.Top || side == BoardSide.Bottom;
        }

        /// <summary>The one-cell step that leads from the board towards this side.</summary>
        public static Cell OutwardStep(this BoardSide side)
        {
            switch (side)
            {
                case BoardSide.Left:
                    return new Cell(-1, 0);
                case BoardSide.Right:
                    return new Cell(1, 0);
                case BoardSide.Bottom:
                    return new Cell(0, -1);
                default:
                    return new Cell(0, 1);
            }
        }
    }
}
