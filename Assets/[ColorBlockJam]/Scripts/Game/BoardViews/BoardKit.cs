using ColorBlockJam.Game.Visuals;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>The supplied parts from which the mesh of a block is put together, and the colours a block can have.</summary>
    public sealed class BoardKit : ScriptableObject
    {
        [Header("Block parts, one quarter of a cell each")]
        [SerializeField] private Mesh _blockCenter;
        [SerializeField] private Mesh _blockEdge;
        [SerializeField] private Mesh _blockOuterCorner;
        [SerializeField] private Mesh _blockInnerCorner;

        [SerializeField] private BlockPalette _palette;

        public Mesh BlockCenter => _blockCenter;
        public Mesh BlockEdge => _blockEdge;
        public Mesh BlockOuterCorner => _blockOuterCorner;
        public Mesh BlockInnerCorner => _blockInnerCorner;
        public BlockPalette Palette => _palette;
    }
}
