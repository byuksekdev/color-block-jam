using DG.Tweening;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>One door in the board frame. It sinks to let a block through and rises again.</summary>
    public sealed class DoorView : MonoBehaviour
    {
        private const float SinkDepth = 1.6f;
        private const float SinkSeconds = 0.12f;
        private const float OpenSeconds = 0.3f;

        private float _closedHeight;

        public void Initialize()
        {
            _closedHeight = transform.position.y;
        }

        public void Open()
        {
            transform.DOKill();
            DOTween.Sequence()
                .Append(transform.DOMoveY(_closedHeight - SinkDepth, SinkSeconds))
                .AppendInterval(OpenSeconds)
                .Append(transform.DOMoveY(_closedHeight, SinkSeconds))
                .SetTarget(transform)
                .SetLink(gameObject);
        }
    }
}
