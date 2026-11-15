using System;
using DG.Tweening;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>
    /// One block on the board. It shows where the session says the block is; it decides nothing.
    /// The object comes from a pool, so it takes its mesh and colour each time it is shown.
    /// </summary>
    public sealed class BlockView : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _renderer;

        [Tooltip("How far a block is lifted above the board while a finger holds it.")]
        [SerializeField] private float _heldLift = 0.35f;

        [Tooltip("Seconds a block takes to come to rest on its cell after the finger lets go.")]
        [SerializeField] private float _settleSeconds = 0.08f;

        [Tooltip("Seconds a block takes to slide out through a door.")]
        [SerializeField] private float _leaveSeconds = 0.3f;

        private Mesh _mesh;
        private float _restHeight;

        /// <summary>The block has slid off the board and can be taken away.</summary>
        public event Action<BlockView> Left;

        /// <summary>Takes over the mesh: the view destroys it when it is released.</summary>
        public void Show(Mesh mesh, Material material, Vector3 position)
        {
            _mesh = mesh;
            _restHeight = position.y;
            _meshFilter.sharedMesh = mesh;
            _renderer.sharedMaterial = material;
            transform.position = position;
        }

        /// <summary>Follows the finger: the block is held slightly above the board.</summary>
        public void Follow(Vector3 position)
        {
            transform.DOKill();
            position.y = _restHeight + _heldLift;
            transform.position = position;
        }

        /// <summary>Comes to rest on a cell after the finger let go.</summary>
        public void SettleAt(Vector3 position)
        {
            transform.DOKill();
            position.y = _restHeight;
            transform.DOMove(position, _settleSeconds).SetLink(gameObject);
        }

        /// <summary>Slides off the board through a door and then reports that it has left.</summary>
        public void Leave(Vector3 offset)
        {
            transform.DOKill();
            Vector3 start = transform.position;
            start.y = _restHeight;
            transform.position = start;
            transform.DOMove(start + offset, _leaveSeconds)
                .SetEase(Ease.InQuad)
                .SetLink(gameObject)
                .OnComplete(RaiseLeft);
        }

        /// <summary>Stops what the block is doing and gives up its mesh, before the object goes back to the pool.</summary>
        public void Release()
        {
            transform.DOKill();
            _meshFilter.sharedMesh = null;
            Destroy(_mesh);
            _mesh = null;
        }

        private void OnDestroy()
        {
            Destroy(_mesh);
        }

        private void RaiseLeft()
        {
            Left?.Invoke(this);
        }
    }
}
