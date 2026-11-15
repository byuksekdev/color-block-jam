using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>
    /// One block on the board. It shows where the session says the block is; it decides nothing.
    /// The object comes from a pool, so it takes its mesh, colour and marks each time it is shown.
    /// </summary>
    public sealed class BlockView : MonoBehaviour
    {
        private static readonly Quaternion AlongX = Quaternion.identity;
        private static readonly Quaternion AlongZ = Quaternion.Euler(0f, 90f, 0f);

        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _renderer;

        [Tooltip("The white edge around the block. It takes the block's mesh and is shown while a finger holds the block.")]
        [SerializeField] private MeshFilter _outline;

        [Tooltip("The number of blocks that still have to leave before the ice melts. Shown while the block is under ice.")]
        [SerializeField] private TMP_Text _iceCount;

        [Tooltip("The arrow of a block that moves along one line only. It points along X and is turned for a block that moves up and down.")]
        [SerializeField] private Transform _arrow;
        [SerializeField] private MeshFilter _arrowMesh;

        [Tooltip("How far a block is lifted above the board while a finger holds it.")]
        [SerializeField] private float _heldLift = 0.35f;

        [Tooltip("Seconds a block takes to come to rest on its cell after the finger lets go.")]
        [SerializeField] private float _settleSeconds = 0.08f;

        [Tooltip("Seconds a block takes to line up with its door before it slides out.")]
        [SerializeField] private float _alignSeconds = 0.08f;

        [Tooltip("Seconds a block takes to slide out through a door.")]
        [SerializeField] private float _leaveSeconds = 0.3f;

        private readonly List<Vector3> _wholeShape = new List<Vector3>();
        private readonly List<Vector3> _cutShape = new List<Vector3>();
        private Mesh _mesh;
        private Material _material;
        private bool _hasArrow;
        private float _restHeight;
        private Vector3 _exitDirection;
        private float _doorLine;

        /// <summary>The block has gone through its door and can be taken away.</summary>
        public event Action<BlockView> Left;

        /// <summary>
        /// Takes over the mesh: the view destroys it when it is released. The marks of the block,
        /// its ice count and its arrow, lie over the middle point, which is given from the block's origin.
        /// </summary>
        public void Show(Mesh mesh, Material material, Vector3 position, Vector3 middle)
        {
            _mesh = mesh;
            _material = material;
            _restHeight = position.y;
            _meshFilter.sharedMesh = mesh;
            _outline.sharedMesh = mesh;
            _renderer.sharedMaterial = material;
            transform.position = position;
            ShowOutline(false);

            _iceCount.gameObject.SetActive(false);
            MoveOver(_iceCount.transform, middle);
            MoveOver(_arrow, middle);
            _hasArrow = false;
            _arrow.gameObject.SetActive(false);
        }

        /// <summary>Marks the block as one that moves along one line only, with an arrow of its length. Call it after Show.</summary>
        public void ShowArrow(Mesh arrow, bool pointsAlongZ)
        {
            _arrowMesh.sharedMesh = arrow;
            _arrow.localRotation = pointsAlongZ ? AlongZ : AlongX;
            _hasArrow = true;
            _arrow.gameObject.SetActive(true);
        }

        /// <summary>
        /// Shows the block under ice with the number of blocks that still have to leave, or in its own
        /// colour when the number is zero. The number takes the place of the arrow while the ice lasts.
        /// </summary>
        public void ShowIce(int count, Material ice)
        {
            bool isFrozen = count > 0;
            _renderer.sharedMaterial = isFrozen ? ice : _material;
            _arrow.gameObject.SetActive(_hasArrow && !isFrozen);
            _iceCount.gameObject.SetActive(isFrozen);
            if (isFrozen)
            {
                _iceCount.SetText("{0}", count);
            }
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
        /// Lines up on the doorstep, the place of the cell it leaves from, then moves out by the offset
        /// and reports that it has left. What passes the door line, where its front stands on the
        /// doorstep, is cut off, so the block goes into the door instead of through the frame.
        /// </summary>
        public void Leave(Vector3 doorstep, Vector3 offset)
        {
            transform.DOKill();
            ShowOutline(false);
            _arrow.gameObject.SetActive(false);
            doorstep.y = _restHeight;

            _mesh.GetVertices(_wholeShape);
            _exitDirection = offset.normalized;
            float front = float.MinValue;
            for (int i = 0; i < _wholeShape.Count; i++)
            {
                front = Mathf.Max(front, Vector3.Dot(_wholeShape[i], _exitDirection));
            }

            _doorLine = Vector3.Dot(doorstep, _exitDirection) + front;

            DOTween.Sequence()
                .Append(transform.DOMove(doorstep, _alignSeconds))
                .Append(transform.DOMove(doorstep + offset, _leaveSeconds).SetEase(Ease.InQuad))
                .SetTarget(transform)
                .SetLink(gameObject)
                .OnUpdate(CutAtDoorLine)
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

        /// <summary>Flattens every vertex that has passed the door line onto it. The mesh is the block's own, so nothing has to be put back.</summary>
        private void CutAtDoorLine()
        {
            float limit = _doorLine - Vector3.Dot(transform.position, _exitDirection);
            _cutShape.Clear();
            for (int i = 0; i < _wholeShape.Count; i++)
            {
                Vector3 vertex = _wholeShape[i];
                float past = Vector3.Dot(vertex, _exitDirection) - limit;
                _cutShape.Add(past > 0f ? vertex - _exitDirection * past : vertex);
            }

            _mesh.SetVertices(_cutShape);
        }

        private void ShowOutline(bool isShown)
        {
            if (_outline.gameObject.activeSelf != isShown)
            {
                _outline.gameObject.SetActive(isShown);
            }
        }

        private static void MoveOver(Transform mark, Vector3 middle)
        {
            Vector3 position = mark.localPosition;
            position.x = middle.x;
            position.z = middle.z;
            mark.localPosition = position;
        }

        private void RaiseLeft()
        {
            Left?.Invoke(this);
        }
    }
}
