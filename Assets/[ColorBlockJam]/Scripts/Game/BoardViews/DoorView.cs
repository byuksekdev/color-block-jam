using DG.Tweening;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>
    /// One door in the board frame. It sinks to let a block through and rises again.
    /// The object comes from a pool, so it takes its length and colour each time it is shown.
    /// </summary>
    public sealed class DoorView : MonoBehaviour
    {
        [Tooltip("The part that is stretched to the length of the door. It is one cell long at scale 1.")]
        [SerializeField] private Transform _body;
        [SerializeField] private MeshRenderer _bodyRenderer;

        [Tooltip("How far the door sinks to let a block pass.")]
        [SerializeField] private float _sinkDepth = 1.6f;

        [Tooltip("Seconds the door takes to sink, and again to rise.")]
        [SerializeField] private float _sinkSeconds = 0.12f;

        [Tooltip("Seconds the door stays down.")]
        [SerializeField] private float _openSeconds = 0.3f;

        private float _closedHeight;

        /// <summary>Call it after the door has been placed: the door closes at the height it has now.</summary>
        public void Show(int cells, Material material)
        {
            _body.localScale = new Vector3(cells, 1f, 1f);
            _bodyRenderer.sharedMaterial = material;
            _closedHeight = transform.position.y;
        }

        public void Open()
        {
            transform.DOKill();
            DOTween.Sequence()
                .Append(transform.DOMoveY(_closedHeight - _sinkDepth, _sinkSeconds))
                .AppendInterval(_openSeconds)
                .Append(transform.DOMoveY(_closedHeight, _sinkSeconds))
                .SetTarget(transform)
                .SetLink(gameObject);
        }

        /// <summary>Stops the door where it is, before the object goes back to the pool.</summary>
        public void Release()
        {
            transform.DOKill();
        }
    }
}
