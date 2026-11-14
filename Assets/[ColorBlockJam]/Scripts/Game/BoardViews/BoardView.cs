using System.Collections.Generic;
using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>
    /// Builds the board of a level from the supplied parts: ground, frame, doors and blocks.
    /// Showing another board removes everything of the board before it.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        /// <summary>How far a block's mesh reaches below its origin, so this lifts it onto the ground.</summary>
        public const float BlockRestHeight = 0.655f;

        /// <summary>The height of a block above the ground.</summary>
        public const float BlockTop = 1.62f;

        private const float FrameThickness = 1f;
        private const float FrameTop = 1.24f;
        private const float FrameTopInMesh = 0.611f;
        private const float WallMeshLength = 1f;
        private const float GroundBaseDepth = 0.02f;
        private const float QuarterTurn = 90f;
        private const float ExitExtraCells = 1f;

        private static readonly Quaternion FrameUpright = Quaternion.Euler(180f, 0f, 0f);
        private static readonly BoardSide[] Sides = { BoardSide.Top, BoardSide.Right, BoardSide.Bottom, BoardSide.Left };

        [SerializeField] private BoardKit _kit;

        private readonly List<BlockView> _blocks = new List<BlockView>();
        private readonly List<DoorView> _doors = new List<DoorView>();
        private IReadOnlyBoard _board;
        private Transform _content;
        private Mesh _groundMesh;

        public BoardSpace Space { get; private set; }

        public void Show(IReadOnlyBoard board)
        {
            Clear();

            _board = board;
            Space = new BoardSpace(transform.position, board.Width, board.Height);
            _content = new GameObject("Board").transform;
            _content.SetParent(transform, false);

            BuildGround();
            BuildFrame();
            BuildBlocks();
        }

        public void Clear()
        {
            if (_content != null)
            {
                Destroy(_content.gameObject);
            }

            Destroy(_groundMesh);
            _blocks.Clear();
            _doors.Clear();
        }

        public BlockView GetBlock(int blockId)
        {
            return _blocks[blockId];
        }

        /// <summary>Where a block rests when its origin is at this point of the board.</summary>
        public Vector3 BlockPosition(BoardPoint origin)
        {
            return Space.ToWorld(origin) + Vector3.up * BlockRestHeight;
        }

        /// <summary>Plays a block leaving through a door. The session has already removed the block.</summary>
        public void PlayExit(Block block, Door door)
        {
            Cell step = door.Side.OutwardStep();
            float cells = (door.Side.RunsAlongX() ? block.Height : block.Width) + ExitExtraCells;
            _blocks[block.Id].Leave(new Vector3(step.X, 0f, step.Y) * (cells * BoardSpace.CellSize));

            for (int i = 0; i < _board.Doors.Count; i++)
            {
                if (_board.Doors[i] == door)
                {
                    _doors[i].Open();
                }
            }
        }

        private void OnDestroy()
        {
            Destroy(_groundMesh);
        }

        private void BuildGround()
        {
            var tiles = new CombineInstance[_board.Width * _board.Height];
            for (int y = 0; y < _board.Height; y++)
            {
                for (int x = 0; x < _board.Width; x++)
                {
                    Vector3 position = Space.CellCenter(new Cell(x, y)) - Space.Center;
                    tiles[y * _board.Width + x] = new CombineInstance
                    {
                        mesh = _kit.GroundTile,
                        transform = Matrix4x4.Translate(position),
                    };
                }
            }

            _groundMesh = new Mesh { name = "Ground" };
            _groundMesh.CombineMeshes(tiles, true, true);
            CreatePart("Ground", _groundMesh, _kit.GridMaterial, Space.Center, Quaternion.identity, Vector3.one);

            Vector3 boardSize = Space.Max - Space.Min;
            float tileSize = _kit.GroundTile.bounds.size.x;
            CreatePart(
                "Ground Base",
                _kit.GroundTile,
                _kit.GroundMaterial,
                Space.Center + Vector3.down * GroundBaseDepth,
                Quaternion.identity,
                new Vector3(boardSize.x / tileSize, 1f, boardSize.z / tileSize));
        }

        private void BuildFrame()
        {
            var doorAt = new Door[Mathf.Max(_board.Width, _board.Height)];

            for (int i = 0; i < _board.Doors.Count; i++)
            {
                _doors.Add(null);
            }

            for (int s = 0; s < Sides.Length; s++)
            {
                BoardSide side = Sides[s];
                int length = side.RunsAlongX() ? _board.Width : _board.Height;
                FindDoors(side, length, doorAt);

                int wallStart = 0;
                for (int position = 0; position <= length; position++)
                {
                    Door door = position < length ? doorAt[position] : null;
                    bool wallEnds = position == length || door != null;
                    if (wallEnds && position > wallStart)
                    {
                        BuildWall(side, wallStart, position - wallStart);
                    }

                    if (door != null)
                    {
                        if (position == door.Start)
                        {
                            BuildDoor(door);
                        }

                        wallStart = position + 1;
                    }
                }

                BuildCorner(s);
            }
        }

        private void FindDoors(BoardSide side, int length, Door[] doorAt)
        {
            for (int position = 0; position < length; position++)
            {
                doorAt[position] = null;
                for (int i = 0; i < _board.Doors.Count; i++)
                {
                    if (_board.Doors[i].Side == side && _board.Doors[i].Spans(position))
                    {
                        doorAt[position] = _board.Doors[i];
                    }
                }
            }
        }

        private void BuildWall(BoardSide side, int start, int cells)
        {
            float length = cells * BoardSpace.CellSize;
            CreatePart(
                "Wall",
                _kit.Wall,
                _kit.WallMaterial,
                FramePosition(side, start, cells),
                SideRotation(side) * FrameUpright,
                new Vector3(length / WallMeshLength, 1f, 1f));
        }

        private void BuildDoor(Door door)
        {
            var root = new GameObject("Door");
            root.transform.SetParent(_content, false);
            root.transform.SetPositionAndRotation(FramePosition(door.Side, door.Start, door.Length), SideRotation(door.Side));

            Material material = BlockMaterial(door.Color);
            MeshRenderer body = CreatePart("Body", _kit.Door, material, root.transform.position, root.transform.rotation * FrameUpright, new Vector3(door.Length, 1f, 1f));
            body.transform.SetParent(root.transform, true);

            Vector3 arrowPosition = root.transform.position + Vector3.up * (FrameTopInMesh + FrameTop + 0.1f);
            Quaternion arrowRotation = root.transform.rotation * Quaternion.Euler(0f, 0f, 180f);
            MeshRenderer arrow = CreatePart("Arrow", _kit.DoorArrow, _kit.DoorArrowMaterial, arrowPosition, arrowRotation, Vector3.one);
            arrow.transform.SetParent(root.transform, true);

            var view = root.AddComponent<DoorView>();
            view.Initialize();

            for (int i = 0; i < _board.Doors.Count; i++)
            {
                if (_board.Doors[i] == door)
                {
                    _doors[i] = view;
                }
            }
        }

        private void BuildCorner(int quarterTurns)
        {
            Quaternion rotation = Quaternion.Euler(0f, quarterTurns * QuarterTurn, 0f);
            Vector3 half = (Space.Max - Space.Min) / 2f + new Vector3(FrameThickness, 0f, FrameThickness) / 2f;
            bool isTurnedSideways = quarterTurns % 2 == 1;
            Vector3 towardsCorner = isTurnedSideways ? new Vector3(half.z, 0f, half.x) : half;

            Vector3 position = Space.Center + rotation * towardsCorner;
            position.y = Space.Center.y + FrameTop - FrameTopInMesh;
            CreatePart("Corner", _kit.WallCorner, _kit.WallMaterial, position, rotation * FrameUpright, Vector3.one);
        }

        private void BuildBlocks()
        {
            for (int i = 0; i < _board.Blocks.Count; i++)
            {
                Block block = _board.Blocks[i];
                var view = new GameObject("Block " + block.Color).AddComponent<BlockView>();
                view.transform.SetParent(_content, false);
                view.Initialize(
                    BlockMeshBuilder.Build(block.Shape, _kit),
                    BlockMaterial(block.Color),
                    BlockPosition(BoardPoint.FromCell(_board.GetOrigin(block.Id))));
                _blocks.Add(view);
            }
        }

        /// <summary>The middle of a run of frame cells on one side, at the height that puts the frame top in place.</summary>
        private Vector3 FramePosition(BoardSide side, int start, int cells)
        {
            float along = (start + cells / 2f) * BoardSpace.CellSize;
            float outside = FrameThickness / 2f;
            Vector3 position;

            switch (side)
            {
                case BoardSide.Left:
                    position = new Vector3(Space.Min.x - outside, 0f, Space.Min.z + along);
                    break;
                case BoardSide.Right:
                    position = new Vector3(Space.Max.x + outside, 0f, Space.Min.z + along);
                    break;
                case BoardSide.Bottom:
                    position = new Vector3(Space.Min.x + along, 0f, Space.Min.z - outside);
                    break;
                default:
                    position = new Vector3(Space.Min.x + along, 0f, Space.Max.z + outside);
                    break;
            }

            position.y = Space.Center.y + FrameTop - FrameTopInMesh;
            return position;
        }

        /// <summary>Turns a frame part, which runs along X and faces +Z, towards its side of the board.</summary>
        private static Quaternion SideRotation(BoardSide side)
        {
            int quarterTurns = System.Array.IndexOf(Sides, side);
            return Quaternion.Euler(0f, quarterTurns * QuarterTurn, 0f);
        }

        private Material BlockMaterial(BlockColor color)
        {
            return _kit.Palette.TryGetMaterial(color, out Material material) ? material : _kit.WallMaterial;
        }

        private MeshRenderer CreatePart(string partName, Mesh mesh, Material material, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var part = new GameObject(partName, typeof(MeshFilter), typeof(MeshRenderer));
            part.transform.SetParent(_content, false);
            part.transform.SetPositionAndRotation(position, rotation);
            part.transform.localScale = scale;
            part.GetComponent<MeshFilter>().sharedMesh = mesh;

            var meshRenderer = part.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            return meshRenderer;
        }
    }
}
