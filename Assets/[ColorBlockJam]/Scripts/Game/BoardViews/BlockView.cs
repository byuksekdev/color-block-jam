using DG.Tweening;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>One block on the board. It shows where the session says the block is; it decides nothing.</summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class BlockView : MonoBehaviour
    {
        private const float HeldLift = 0.35f;
        private const float SettleSeconds = 0.08f;
        private const float LeaveSeconds = 0.3f;

        private Mesh _mesh;
        private float _restHeight;

        public void Initialize(Mesh mesh, Material material, Vector3 position)
        {
            _mesh = mesh;
            _restHeight = position.y;
            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshRenderer>().sharedMaterial = material;
            transform.position = position;
        }

        /// <summary>Follows the finger: the block is held slightly above the board.</summary>
        public void Follow(Vector3 position)
        {
            transform.DOKill();
            position.y = _restHeight + HeldLift;
            transform.position = position;
        }

        /// <summary>Comes to rest on a cell after the finger let go.</summary>
        public void SettleAt(Vector3 position)
        {
            transform.DOKill();
            position.y = _restHeight;
            transform.DOMove(position, SettleSeconds).SetLink(gameObject);
        }

        /// <summary>Slides off the board through a door and removes itself.</summary>
        public void Leave(Vector3 offset)
        {
            transform.DOKill();
            Vector3 start = transform.position;
            start.y = _restHeight;
            transform.position = start;
            transform.DOMove(start + offset, LeaveSeconds)
                .SetEase(Ease.InQuad)
                .SetLink(gameObject)
                .OnComplete(DestroySelf);
        }

        private void DestroySelf()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Destroy(_mesh);
        }
    }
}
