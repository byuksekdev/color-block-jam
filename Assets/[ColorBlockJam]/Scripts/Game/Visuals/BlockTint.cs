using System;
using ColorBlockJam.Core.Data;
using UnityEngine;

namespace ColorBlockJam.Game.Visuals
{
    [Serializable]
    public sealed class BlockTint
    {
        [SerializeField] private BlockColor _blockColor;
        [SerializeField] private Color _tint = Color.white;

        public BlockColor BlockColor => _blockColor;
        public Color Tint => _tint;
    }
}
