using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using UnityEngine;

namespace ColorBlockJam.Game.Visuals
{
    /// <summary>How each block colour looks. The game and the level editor both read it, so they show the same colours.</summary>
    public sealed class BlockPalette : ScriptableObject
    {
        [SerializeField] private List<BlockTint> _tints = new List<BlockTint>();

        public bool TryGetTint(BlockColor blockColor, out Color tint)
        {
            for (int i = 0; i < _tints.Count; i++)
            {
                if (_tints[i].BlockColor == blockColor)
                {
                    tint = _tints[i].Tint;
                    return true;
                }
            }

            tint = default;
            return false;
        }
    }
}
