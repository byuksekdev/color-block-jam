using System;
using ColorBlockJam.Core.Data;
using UnityEngine;

namespace ColorBlockJam.Game.Visuals
{
    [Serializable]
    public sealed class BlockLook
    {
        [SerializeField] private BlockColor _blockColor;
        [SerializeField] private Material _material;

        public BlockColor BlockColor => _blockColor;
        public Material Material => _material;
    }
}
