using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using UnityEngine;

namespace ColorBlockJam.Game.Visuals
{
    /// <summary>How each block colour looks. The game and the level editor both read it, so they show the same colours.</summary>
    public sealed class BlockPalette : ScriptableObject
    {
        [SerializeField] private List<BlockLook> _looks = new List<BlockLook>();

        public bool TryGetMaterial(BlockColor blockColor, out Material material)
        {
            for (int i = 0; i < _looks.Count; i++)
            {
                if (_looks[i].BlockColor == blockColor && _looks[i].Material != null)
                {
                    material = _looks[i].Material;
                    return true;
                }
            }

            material = null;
            return false;
        }

        /// <summary>The colour of the material, for places that draw a block flat, such as the level editor.</summary>
        public bool TryGetTint(BlockColor blockColor, out Color tint)
        {
            bool found = TryGetMaterial(blockColor, out Material material);
            tint = found ? material.color : default;
            return found;
        }
    }
}
