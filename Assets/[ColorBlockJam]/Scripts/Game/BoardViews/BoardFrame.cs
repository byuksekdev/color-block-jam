using System.Collections.Generic;
using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Game.Visuals;
using Lean.Pool;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>
    /// The frame around the board: walls, corners and doors, taken from pools. A wall and a door
    /// are one cell long at scale 1 and are stretched along their side.
    /// </summary>
    public sealed class BoardFrame : MonoBehaviour
    {
        /// <summary>How far the frame reaches outwards from the edge of the board.</summary>
        public const float Thickness = 1f;

        private const float QuarterTurn = 90f;

        private static readonly BoardSide[] Sides = { BoardSide.Top, BoardSide.Right, BoardSide.Bottom, BoardSide.Left };

        [SerializeField] private BlockPalette _palette;
        [SerializeField] private LeanGameObjectPool _wallPool;
        [SerializeField] private LeanGameObjectPool _cornerPool;
        [SerializeField] private LeanGameObjectPool _doorPool;
        [SerializeField] private LeanGameObjectPool _exitEffectPool;

        private readonly List<GameObject> _walls = new List<GameObject>();
        private readonly List<GameObject> _corners = new List<GameObject>();
        private readonly List<DoorView> _doors = new List<DoorView>();
        private readonly List<ExitEffect> _exitEffects = new List<ExitEffect>();
        private IReadOnlyBoard _board;
        private BoardSpace _space;

        public void Show(IReadOnlyBoard board, BoardSpace space)
        {
            Clear();
            _board = board;
            _space = space;

            var doorAt = new Door[Mathf.Max(board.Width, board.Height)];
            for (int i = 0; i < board.Doors.Count; i++)
            {
                _doors.Add(null);
            }

            for (int s = 0; s < Sides.Length; s++)
            {
                BoardSide side = Sides[s];
                int length = side.RunsAlongX() ? board.Width : board.Height;
                FindDoors(side, length, doorAt);

                int wallStart = 0;
                for (int position = 0; position <= length; position++)
                {
                    Door door = position < length ? doorAt[position] : null;
                    bool wallEnds = position == length || door != null;
                    if (wallEnds && position > wallStart)
                    {
                        PlaceWall(side, wallStart, position - wallStart);
                    }

                    if (door != null)
                    {
                        if (position == door.Start)
                        {
                            PlaceDoor(door);
                        }

                        wallStart = position + 1;
                    }
                }

                PlaceCorner(s);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _walls.Count; i++)
            {
                _wallPool.Despawn(_walls[i]);
            }

            for (int i = 0; i < _corners.Count; i++)
            {
                _cornerPool.Despawn(_corners[i]);
            }

            for (int i = 0; i < _doors.Count; i++)
            {
                _doors[i].Release();
                _doorPool.Despawn(_doors[i].gameObject);
            }

            for (int i = 0; i < _exitEffects.Count; i++)
            {
                _exitEffects[i].Finished -= TakeAway;
                _exitEffects[i].Release();
                _exitEffectPool.Despawn(_exitEffects[i].gameObject);
            }

            _walls.Clear();
            _corners.Clear();
            _doors.Clear();
            _exitEffects.Clear();
        }

        /// <summary>Shows how much ice is left on every door.</summary>
        public void ShowIce()
        {
            for (int i = 0; i < _doors.Count; i++)
            {
                Door door = _board.Doors[i];
                if (door.IceCount > 0)
                {
                    _doors[i].ShowIce(_board.IceLeft(door), _palette.Ice);
                }
            }
        }

        /// <summary>
        /// Opens the door for a block that leaves from this cell. Returns the cubes of the block's colour
        /// that come out where it passes; they come out as the block tells them how far it has gone.
        /// </summary>
        public ExitEffect LetOut(Block block, Cell origin, Door door)
        {
            for (int i = 0; i < _board.Doors.Count; i++)
            {
                if (_board.Doors[i] == door)
                {
                    _doors[i].Open();
                }
            }

            bool runsAlongX = door.Side.RunsAlongX();
            int start = runsAlongX ? origin.X : origin.Y;
            int cells = runsAlongX ? block.Width : block.Height;
            GameObject clone = _exitEffectPool.Spawn(SidePosition(door.Side, start, cells), SideRotation(door.Side), transform);
            var effect = clone.GetComponent<ExitEffect>();

            _palette.TryGetMaterial(block.Color, out Material material);
            effect.Finished += TakeAway;
            effect.Play(block, door.Side, material);
            _exitEffects.Add(effect);
            return effect;
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

        private void PlaceWall(BoardSide side, int start, int cells)
        {
            GameObject wall = _wallPool.Spawn(SidePosition(side, start, cells), SideRotation(side), transform);
            wall.transform.localScale = new Vector3(cells, 1f, 1f);
            _walls.Add(wall);
        }

        private void PlaceDoor(Door door)
        {
            GameObject clone = _doorPool.Spawn(SidePosition(door.Side, door.Start, door.Length), SideRotation(door.Side), transform);
            var view = clone.GetComponent<DoorView>();
            _palette.TryGetMaterial(door.Color, out Material material);
            view.Show(door.Length, material);

            for (int i = 0; i < _board.Doors.Count; i++)
            {
                if (_board.Doors[i] == door)
                {
                    _doors[i] = view;
                }
            }
        }

        private void TakeAway(ExitEffect effect)
        {
            effect.Finished -= TakeAway;
            _exitEffects.Remove(effect);
            _exitEffectPool.Despawn(effect.gameObject);
        }

        private void PlaceCorner(int quarterTurns)
        {
            Quaternion rotation = Quaternion.Euler(0f, quarterTurns * QuarterTurn, 0f);
            Vector3 half = (_space.Max - _space.Min) / 2f + new Vector3(Thickness, 0f, Thickness) / 2f;
            bool isTurnedSideways = quarterTurns % 2 == 1;
            Vector3 towardsCorner = isTurnedSideways ? new Vector3(half.z, 0f, half.x) : half;

            _corners.Add(_cornerPool.Spawn(_space.Center + rotation * towardsCorner, rotation, transform));
        }

        /// <summary>The middle of a run of frame cells on one side, on the ground.</summary>
        private Vector3 SidePosition(BoardSide side, int start, int cells)
        {
            float along = (start + cells / 2f) * BoardSpace.CellSize;
            float outside = Thickness / 2f;

            switch (side)
            {
                case BoardSide.Left:
                    return new Vector3(_space.Min.x - outside, _space.Center.y, _space.Min.z + along);
                case BoardSide.Right:
                    return new Vector3(_space.Max.x + outside, _space.Center.y, _space.Min.z + along);
                case BoardSide.Bottom:
                    return new Vector3(_space.Min.x + along, _space.Center.y, _space.Min.z - outside);
                default:
                    return new Vector3(_space.Min.x + along, _space.Center.y, _space.Max.z + outside);
            }
        }

        /// <summary>Turns a frame part, which runs along X and faces +Z, towards its side of the board.</summary>
        private static Quaternion SideRotation(BoardSide side)
        {
            int quarterTurns = System.Array.IndexOf(Sides, side);
            return Quaternion.Euler(0f, quarterTurns * QuarterTurn, 0f);
        }
    }
}
