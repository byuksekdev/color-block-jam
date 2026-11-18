using System.Collections.Generic;
using System.Globalization;

namespace ColorBlockJam.Core.Editing
{
    /// <summary>What a smaller board takes away from a level: blocks that no longer fit, doors removed or shortened.</summary>
    public readonly struct ResizeLoss
    {
        public ResizeLoss(int removedBlocks, int removedDoors, int shortenedDoors)
        {
            RemovedBlocks = removedBlocks;
            RemovedDoors = removedDoors;
            ShortenedDoors = shortenedDoors;
        }

        public int RemovedBlocks { get; }
        public int RemovedDoors { get; }
        public int ShortenedDoors { get; }

        public bool IsAnythingLost => RemovedBlocks > 0 || RemovedDoors > 0 || ShortenedDoors > 0;

        /// <summary>The loss in words the designer reads before agreeing to it.</summary>
        public string Describe()
        {
            var lines = new List<string> { "The smaller board takes away what no longer fits:" };
            AddLine(lines, RemovedBlocks, "block", "removed");
            AddLine(lines, RemovedDoors, "door", "removed");
            AddLine(lines, ShortenedDoors, "door", "made shorter");
            lines.Add("Undo brings them back.");
            return string.Join("\n", lines);
        }

        private static void AddLine(List<string> lines, int count, string thing, string change)
        {
            if (count == 0)
            {
                return;
            }

            string plural = count == 1 ? string.Empty : "s";
            lines.Add("• " + count.ToString(CultureInfo.InvariantCulture) + " " + thing + plural + " " + change);
        }
    }
}
