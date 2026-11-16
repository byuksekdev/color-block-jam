using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>
    /// The small cubes a block turns into as it goes through a door. Cubes come out only where a cell
    /// of the block is passing the door at that moment, and only as fast as the block moves.
    /// The object stands in the middle of the block's run along the door, runs along X and faces
    /// outwards along +Z. It comes from a pool.
    /// </summary>
    public sealed class ExitEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particles;
        [SerializeField] private ParticleSystemRenderer _renderer;

        [Tooltip("How many cubes each cell of the block turns into.")]
        [SerializeField] private int _cubesPerCell = 6;

        [Tooltip("The middle of the box the cubes of one cell start in: X along the door, Y up, Z outwards.")]
        [SerializeField] private Vector3 _mouthCenter = new Vector3(0f, 0.9f, 0.5f);

        [Tooltip("The size of the box the cubes of one cell start in.")]
        [SerializeField] private Vector3 _mouthSize = new Vector3(1.6f, 1f, 0.4f);

        [Tooltip("The slowest a cube flies outwards, in units per second.")]
        [SerializeField] private float _minSpeed = 2.5f;

        [Tooltip("The fastest a cube flies outwards, in units per second.")]
        [SerializeField] private float _maxSpeed = 5f;

        [Tooltip("The most a cube drifts along the door and upwards, in units per second.")]
        [SerializeField] private float _drift = 1f;

        private readonly List<int> _rows = new List<int>();
        private readonly List<Vector3> _mouths = new List<Vector3>();
        private ParticleSystem.EmitParams _cube;
        private Quaternion _rotation;
        private float _length;
        private float _passed;
        private float _owedCubes;

        /// <summary>The last cube has gone and the effect can be taken away.</summary>
        public event Action<ExitEffect> Finished;

        /// <summary>Gets ready for a block that leaves through a door on this side. No cube comes out before the block moves.</summary>
        public void Play(Block block, BoardSide side, Material material)
        {
            bool runsAlongX = side.RunsAlongX();
            Cell step = side.OutwardStep();
            int cellsAcross = runsAlongX ? block.Width : block.Height;
            int cellsDeep = runsAlongX ? block.Height : block.Width;
            bool leavesForwards = (runsAlongX ? step.Y : step.X) > 0;
            Vector3 alongDoor = runsAlongX ? Vector3.right : Vector3.forward;

            _rotation = transform.rotation;
            _rows.Clear();
            _mouths.Clear();
            for (int i = 0; i < block.Shape.Count; i++)
            {
                Cell cell = block.Shape[i];
                int across = runsAlongX ? cell.X : cell.Y;
                int deep = runsAlongX ? cell.Y : cell.X;

                _rows.Add(leavesForwards ? cellsDeep - 1 - deep : deep);
                _mouths.Add(transform.position
                    + alongDoor * ((across + 0.5f - cellsAcross / 2f) * BoardSpace.CellSize)
                    + _rotation * _mouthCenter);
            }

            _length = cellsDeep * BoardSpace.CellSize;
            _passed = 0f;
            _owedCubes = 0f;
            _renderer.sharedMaterial = material;
            _particles.Play();
        }

        /// <summary>
        /// Throws out the cubes of what has gone through the door since the last call.
        /// The distance is how far the block has moved into the door, from its front standing at it.
        /// </summary>
        public void ShowPassed(float distance)
        {
            float passed = Mathf.Min(distance, _length);
            if (passed <= _passed)
            {
                return;
            }

            int row = (int)(_passed / BoardSpace.CellSize);
            _owedCubes += (passed - _passed) / BoardSpace.CellSize * _cubesPerCell;
            _passed = passed;

            int cubes = (int)_owedCubes;
            _owedCubes -= cubes;
            for (int i = 0; i < _rows.Count && cubes > 0; i++)
            {
                if (_rows[i] == row)
                {
                    ThrowOut(_mouths[i], cubes);
                }
            }

            if (_passed >= _length)
            {
                _particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        /// <summary>Removes the cubes at once, before the object goes back to the pool.</summary>
        public void Release()
        {
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void ThrowOut(Vector3 mouth, int cubes)
        {
            Vector3 reach = _mouthSize / 2f;
            for (int i = 0; i < cubes; i++)
            {
                var offset = new Vector3(
                    UnityEngine.Random.Range(-reach.x, reach.x),
                    UnityEngine.Random.Range(-reach.y, reach.y),
                    UnityEngine.Random.Range(-reach.z, reach.z));
                var velocity = new Vector3(
                    UnityEngine.Random.Range(-_drift, _drift),
                    UnityEngine.Random.Range(0f, _drift),
                    UnityEngine.Random.Range(_minSpeed, _maxSpeed));

                _cube.position = mouth + _rotation * offset;
                _cube.velocity = _rotation * velocity;
                _particles.Emit(_cube, 1);
            }
        }

        /// <summary>Unity calls this when the particle system has stopped and its stop action is Callback.</summary>
        private void OnParticleSystemStopped()
        {
            Finished?.Invoke(this);
        }
    }
}
