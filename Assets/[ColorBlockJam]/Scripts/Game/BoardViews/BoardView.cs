using System.Collections.Generic;
using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using Lean.Pool;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>
    /// Shows the board of a level: the ground, the frame with its doors, and the blocks.
    /// Showing another board first gives everything of the board before it back to the pools.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        /// <summary>How far a block's mesh reaches below its origin, so this lifts it onto the ground.</summary>
        public const float BlockRestHeight = 0.655f;

        /// <summary>The height of a block above the ground.</summary>
        public const float BlockTop = 1.62f;

        /// <summary>A leaving block slides this many cells further than its own size, so that it clears the frame.</summary>
        private const float ExitExtraCells = 1f;

        [SerializeField] private BoardKit _kit;
        [SerializeField] private GroundView _ground;
        [SerializeField] private BoardFrame _frame;
        [SerializeField] private LeanGameObjectPool _blockPool;

        private readonly List<BlockView> _blocks = new List<BlockView>();

        public BoardSpace Space { get; private set; }

        public void Show(IReadOnlyBoard board)
        {
            Clear();

            Space = new BoardSpace(transform.position, board.Width, board.Height);
            _ground.Show(Space);
            _frame.Show(board, Space);

            for (int i = 0; i < board.Blocks.Count; i++)
            {
                Block block = board.Blocks[i];
                Vector3 position = BlockPosition(BoardPoint.FromCell(board.GetOrigin(block.Id)));
                var view = _blockPool.Spawn(position, Quaternion.identity, transform).GetComponent<BlockView>();

                _kit.Palette.TryGetMaterial(block.Color, out Material material);
                view.Show(BlockMeshBuilder.Build(block.Shape, _kit), material, position);
                view.Left += TakeAway;
                _blocks.Add(view);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _blocks.Count; i++)
            {
                if (_blocks[i] != null)
                {
                    TakeAway(_blocks[i]);
                }
            }

            _blocks.Clear();
            _frame.Clear();
            _ground.Hide();
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
            _frame.Open(door);
        }

        private void TakeAway(BlockView view)
        {
            view.Left -= TakeAway;
            view.Release();
            _blockPool.Despawn(view.gameObject);
            _blocks[_blocks.IndexOf(view)] = null;
        }
    }
}
