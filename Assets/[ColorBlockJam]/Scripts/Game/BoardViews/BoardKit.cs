using ColorBlockJam.Game.Visuals;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>The supplied meshes and the materials from which a board is built.</summary>
    public sealed class BoardKit : ScriptableObject
    {
        [Header("Block parts, one quarter of a cell each")]
        [SerializeField] private Mesh _blockCenter;
        [SerializeField] private Mesh _blockEdge;
        [SerializeField] private Mesh _blockOuterCorner;
        [SerializeField] private Mesh _blockInnerCorner;

        [Header("Board")]
        [SerializeField] private Mesh _groundTile;
        [SerializeField] private Mesh _wall;
        [SerializeField] private Mesh _wallCorner;
        [SerializeField] private Mesh _door;
        [SerializeField] private Mesh _doorArrow;

        [Header("Materials")]
        [SerializeField] private Material _gridMaterial;
        [SerializeField] private Material _groundMaterial;
        [SerializeField] private Material _wallMaterial;
        [SerializeField] private Material _doorArrowMaterial;
        [SerializeField] private BlockPalette _palette;

        public Mesh BlockCenter => _blockCenter;
        public Mesh BlockEdge => _blockEdge;
        public Mesh BlockOuterCorner => _blockOuterCorner;
        public Mesh BlockInnerCorner => _blockInnerCorner;
        public Mesh GroundTile => _groundTile;
        public Mesh Wall => _wall;
        public Mesh WallCorner => _wallCorner;
        public Mesh Door => _door;
        public Mesh DoorArrow => _doorArrow;
        public Material GridMaterial => _gridMaterial;
        public Material GroundMaterial => _groundMaterial;
        public Material WallMaterial => _wallMaterial;
        public Material DoorArrowMaterial => _doorArrowMaterial;
        public BlockPalette Palette => _palette;
    }
}
