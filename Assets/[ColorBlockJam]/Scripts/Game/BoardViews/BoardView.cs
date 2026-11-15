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

        [SerializeField] private BoardKit _kit;
        [SerializeField] private GroundView _ground;
        [SerializeField] private BoardFrame _frame;
        [SerializeField] private LeanGameObjectPool _blockPool;

        private readonly List<BlockView> _blocks = new List<BlockView>();
        private IReadOnlyBoard _board;

        public BoardSpace Space { get; private set; }

        public void Show(IReadOnlyBoard board)
        {
            Clear();

            _board = board;
            Space = new BoardSpace(transform.position, board.Width, board.Height);
            _ground.Show(Space);
            _frame.Show(board, Space);

            for (int i = 0; i < board.Blocks.Count; i++)
            {
                Block block = board.Blocks[i];
                Vector3 position = BlockPosition(BoardPoint.FromCell(board.GetOrigin(block.Id)));
                var view = _blockPool.Spawn(position, Quaternion.identity, transform).GetComponent<BlockView>();

                BoardPoint middle = BoardPoint.MiddleOf(block.Shape);
                _kit.Palette.TryGetMaterial(block.Color, out Material material);
                view.Show(
                    BlockMeshBuilder.Build(block.Shape, _kit),
                    material,
                    position,
                    new Vector3(middle.X, 0f, middle.Y) * BoardSpace.CellSize);
                if (block.Axis != BlockAxis.Any)
                {
                    bool isVertical = block.Axis == BlockAxis.Vertical;
                    view.ShowArrow(_kit.ArrowFor(ArrowCells(block, isVertical)), isVertical);
                }

                view.Left += TakeAway;
                _blocks.Add(view);
            }

            ShowIce();
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

        /// <summary>
        /// Plays a block leaving through a door: the block lines up on the cell it left from and goes
        /// into the door by its own length. The session has already removed the block.
        /// </summary>
        public void PlayExit(Block block, Cell origin, Door door)
        {
            Cell step = door.Side.OutwardStep();
            int cells = door.Side.RunsAlongX() ? block.Height : block.Width;
            Vector3 doorstep = BlockPosition(BoardPoint.FromCell(origin));
            _blocks[block.Id].Leave(doorstep, new Vector3(step.X, 0f, step.Y) * (cells * BoardSpace.CellSize));
            _frame.LetOut(block, origin, door);
            ShowIce();
        }

        /// <summary>Shows how much ice is left on every block and door. Each block that leaves takes one off every count.</summary>
        private void ShowIce()
        {
            for (int i = 0; i < _board.Blocks.Count; i++)
            {
                Block block = _board.Blocks[i];
                if (block.IceCount > 0 && _board.IsOnBoard(block.Id))
                {
                    _blocks[i].ShowIce(_board.IceLeft(block), _kit.Palette.Ice);
                }
            }

            _frame.ShowIce();
        }

        /// <summary>An arrow runs the whole length of a rectangular block. On any other shape it stays on the one cell it is drawn over.</summary>
        private static int ArrowCells(Block block, bool isVertical)
        {
            bool isRectangle = block.Shape.Count == block.Width * block.Height;
            if (!isRectangle)
            {
                return 1;
            }

            return isVertical ? block.Height : block.Width;
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
