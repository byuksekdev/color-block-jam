using ColorBlockJam.Core.Data;
using ColorBlockJam.Game.Visuals;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    internal static class EditorTint
    {
        private static readonly Color Missing = new Color(0.5f, 0.5f, 0.5f);

        /// <summary>The colour of the palette, or grey when the palette is missing or has no entry.</summary>
        public static Color Of(BlockPalette palette, BlockColor blockColor)
        {
            return palette != null && palette.TryGetTint(blockColor, out Color tint) ? tint : Missing;
        }
    }
}
