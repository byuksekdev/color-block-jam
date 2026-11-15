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

        [Tooltip("The white edge around the block. It takes the block's mesh and is shown while a finger holds the block.")]
        [SerializeField] private MeshFilter _outline;

        [Tooltip("How far a block is lifted above the board while a finger holds it.")]
        [SerializeField] private float _heldLift = 0.35f;

        [Tooltip("Seconds a block takes to come to rest on its cell after the finger lets go.")]
        [SerializeField] private float _settleSeconds = 0.08f;

        [Tooltip("Seconds a block takes to line up with its door before it slides out.")]
        [SerializeField] private float _alignSeconds = 0.08f;

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
            _outline.sharedMesh = mesh;
            _renderer.sharedMaterial = material;
            transform.position = position;
            ShowOutline(false);
        }

        /// <summary>Follows the finger: the block is held slightly above the board.</summary>
        public void Follow(Vector3 position)
        {
            transform.DOKill();
            position.y = _restHeight + _heldLift;
            transform.position = position;
            ShowOutline(true);
        }

        /// <summary>Comes to rest on a cell after the finger let go.</summary>
        public void SettleAt(Vector3 position)
        {
            transform.DOKill();
            position.y = _restHeight;
            transform.DOMove(position, _settleSeconds).SetLink(gameObject);
            ShowOutline(false);
        }

        /// <summary>
        /// Lines up on the doorstep, the place of the cell it leaves from, then slides off the board
        /// through the door and reports that it has left.
        /// </summary>
        public void Leave(Vector3 doorstep, Vector3 offset)
        {
            transform.DOKill();
            ShowOutline(false);
            doorstep.y = _restHeight;
            DOTween.Sequence()
                .Append(transform.DOMove(doorstep, _alignSeconds))
                .Append(transform.DOMove(doorstep + offset, _leaveSeconds).SetEase(Ease.InQuad))
                .SetTarget(transform)
                .SetLink(gameObject)
                .OnComplete(RaiseLeft);
        }

        /// <summary>Stops what the block is doing and gives up its mesh, before the object goes back to the pool.</summary>
        public void Release()
        {
            transform.DOKill();
            ShowOutline(false);
            _meshFilter.sharedMesh = null;
            _outline.sharedMesh = null;
            Destroy(_mesh);
            _mesh = null;
        }

        private void OnDestroy()
        {
            Destroy(_mesh);
        }

        private void ShowOutline(bool isShown)
        {
            if (_outline.gameObject.activeSelf != isShown)
            {
                _outline.gameObject.SetActive(isShown);
            }
        }

        private void RaiseLeft()
        {
            Left?.Invoke(this);
        }
    }
}
