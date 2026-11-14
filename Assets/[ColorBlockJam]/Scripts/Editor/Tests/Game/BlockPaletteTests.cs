using System;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Game.Visuals;
using NUnit.Framework;
using UnityEditor;

namespace ColorBlockJam.Tests.Game
{
    public class BlockPaletteTests
    {
        private static BlockPalette LoadProjectPalette()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(BlockPalette));
            Assert.AreEqual(1, guids.Length, "The project needs exactly one block palette.");
            return AssetDatabase.LoadAssetAtPath<BlockPalette>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        [Test]
        public void ProjectPalette_EveryBlockColour_HasATint()
        {
            BlockPalette palette = LoadProjectPalette();

            foreach (BlockColor color in Enum.GetValues(typeof(BlockColor)))
            {
                if (color != BlockColor.None)
                {
                    Assert.IsTrue(palette.TryGetTint(color, out _), color + " has no tint in the palette.");
                }
            }
        }

        [Test]
        public void ProjectPalette_NoColour_HasNoTint()
        {
            Assert.IsFalse(LoadProjectPalette().TryGetTint(BlockColor.None, out _));
        }
    }
}
