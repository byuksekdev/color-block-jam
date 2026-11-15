using ColorBlockJam.Game.Visuals;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>The supplied parts from which the mesh of a block is put together, its arrows, and the colours a block can have.</summary>
    public sealed class BoardKit : ScriptableObject
    {
        [Header("Block parts, one quarter of a cell each")]
        [SerializeField] private Mesh _blockCenter;
        [SerializeField] private Mesh _blockEdge;
        [SerializeField] private Mesh _blockOuterCorner;
        [SerializeField] private Mesh _blockInnerCorner;

        [Tooltip("The arrow on a block that moves along one line only: one mesh for each length in cells, shortest first. A longer block takes the last one.")]
        [SerializeField] private Mesh[] _arrows;

        [SerializeField] private BlockPalette _palette;

        public Mesh BlockCenter => _blockCenter;
        public Mesh BlockEdge => _blockEdge;
        public Mesh BlockOuterCorner => _blockOuterCorner;
        public Mesh BlockInnerCorner => _blockInnerCorner;
        public BlockPalette Palette => _palette;

        /// <summary>The arrow for a block that is this many cells long in the direction it moves.</summary>
        public Mesh ArrowFor(int cells)
        {
            return _arrows[Mathf.Clamp(cells, 1, _arrows.Length) - 1];
        }
    }
}
