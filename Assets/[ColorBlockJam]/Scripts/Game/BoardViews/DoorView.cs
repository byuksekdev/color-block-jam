using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>
    /// One door in the board frame. It dips while a block goes into it.
    /// The object comes from a pool, so it takes its length, colour and ice each time it is shown.
    /// </summary>
    public sealed class DoorView : MonoBehaviour
    {
        /// <summary>The ice count lies flat on the door and reads the same way on every side of the board.</summary>
        private static readonly Quaternion FlatOnTop = Quaternion.Euler(90f, 0f, 0f);

        [Tooltip("The part that is stretched to the length of the door. It is one cell long at scale 1.")]
        [SerializeField] private Transform _body;
        [SerializeField] private MeshRenderer _bodyRenderer;

        [Tooltip("The mark that shows which way blocks leave. Hidden while the door is under ice.")]
        [SerializeField] private GameObject _arrow;

        [Tooltip("The number of blocks that still have to leave before the ice melts. Shown while the door is under ice.")]
        [SerializeField] private TMP_Text _iceCount;

        [Tooltip("How far the door dips while a block goes into it.")]
        [SerializeField] private float _sinkDepth = 0.25f;

        [Tooltip("Seconds the door takes to dip, and again to rise.")]
        [SerializeField] private float _sinkSeconds = 0.12f;

        [Tooltip("Seconds the door stays down.")]
        [SerializeField] private float _openSeconds = 0.3f;

        private Material _material;
        private float _closedHeight;

        /// <summary>Call it after the door has been placed: the door closes at the height it has now.</summary>
        public void Show(int cells, Material material)
        {
            _material = material;
            _body.localScale = new Vector3(cells, 1f, 1f);
            _bodyRenderer.sharedMaterial = material;
            _closedHeight = transform.position.y;
            _arrow.SetActive(true);
            _iceCount.gameObject.SetActive(false);
            _iceCount.transform.rotation = FlatOnTop;
        }

        /// <summary>Shows the door under ice with the number of blocks that still have to leave, or in its own colour when the number is zero.</summary>
        public void ShowIce(int count, Material ice)
        {
            bool isFrozen = count > 0;
            _bodyRenderer.sharedMaterial = isFrozen ? ice : _material;
            _arrow.SetActive(!isFrozen);
            _iceCount.gameObject.SetActive(isFrozen);
            if (isFrozen)
            {
                _iceCount.SetText("{0}", count);
            }
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
