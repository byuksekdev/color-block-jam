using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Boards
{
    public static class BlockAxisExtensions
    {
        public static bool AllowsHorizontal(this BlockAxis axis)
        {
            return axis != BlockAxis.Vertical;
        }

        public static bool AllowsVertical(this BlockAxis axis)
        {
            return axis != BlockAxis.Horizontal;
        }

        /// <summary>True when a block with this axis can move out through a door on this side of the board.</summary>
        public static bool AllowsLeavingThrough(this BlockAxis axis, BoardSide side)
        {
            return side.RunsAlongX() ? axis.AllowsVertical() : axis.AllowsHorizontal();
        }
    }
}
