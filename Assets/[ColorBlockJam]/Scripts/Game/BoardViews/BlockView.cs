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

        [Tooltip("Everything of the block that leans while it is dragged. It stands on the middle of the block, so the block leans around its middle.")]
        [SerializeField] private Transform _body;
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

        [Tooltip("Degrees a held block leans for each unit per second it moves.")]
        [SerializeField] private float _leanPerSpeed = 1.2f;

        [Tooltip("The most a held block leans, in degrees.")]
        [SerializeField] private float _maxLean = 12f;

        [Tooltip("Seconds the lean of a held block takes to follow a change of its speed.")]
        [SerializeField] private float _leanSeconds = 0.1f;

        [Tooltip("Seconds a block takes to come to rest on its cell after the finger lets go.")]
        [SerializeField] private float _settleSeconds = 0.08f;

        [Tooltip("Seconds a block takes to stand upright again after the finger lets go.")]
        [SerializeField] private float _uprightSeconds = 0.25f;

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
        private Vector2 _lean;
        private ExitEffect _exitEffect;
        private Vector3 _exitDirection;
        private float _doorstep;
        private float _front;

        /// <summary>The block has gone through its door and can be taken away.</summary>
        public event Action<BlockView> Left;

        /// <summary>
        /// Takes over the mesh: the view destroys it when it is released. The marks of the block,
        /// its ice count and its arrow, lie over the middle point, which is given from the block's origin.
        /// The mesh starts at the block's origin.
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
            _body.localPosition = middle;
            _body.localRotation = Quaternion.identity;
            _meshFilter.transform.localPosition = -middle;
            _lean = Vector2.zero;
            ShowOutline(false);

            _iceCount.gameObject.SetActive(false);
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

        /// <summary>
        /// Follows the finger: the block is held slightly above the board and leans the way it moves.
        /// Call it every frame the block is held, also when it has not moved, so the lean can fade.
        /// </summary>
        public void Follow(Vector3 position)
        {
            StopMoving();
            position.y = _restHeight + _heldLift;
            LeanWith(position - transform.position);
            transform.position = position;
            ShowOutline(true);
        }

        /// <summary>Comes to rest on a cell after the finger let go.</summary>
        public void SettleAt(Vector3 position)
        {
            StopMoving();
            position.y = _restHeight;
            transform.DOMove(position, _settleSeconds).SetLink(gameObject);
            StandUpright(_uprightSeconds).SetEase(Ease.OutBack);
            ShowOutline(false);
        }

        /// <summary>
        /// Lines up on the doorstep, the place of the cell it leaves from, then moves out by the offset
        /// and reports that it has left. What passes the door line, where its front stands on the
        /// doorstep, is cut off, so the block goes into the door instead of through the frame.
        /// The cubes are told how far the block has gone through, every time it moves.
        /// </summary>
        public void Leave(Vector3 doorstep, Vector3 offset, ExitEffect cubes)
        {
            StopMoving();
            ShowOutline(false);
            _arrow.gameObject.SetActive(false);
            doorstep.y = _restHeight;

            _mesh.GetVertices(_wholeShape);
            _exitEffect = cubes;
            _exitDirection = offset.normalized;
            _doorstep = Vector3.Dot(doorstep, _exitDirection);
            _front = float.MinValue;
            for (int i = 0; i < _wholeShape.Count; i++)
            {
                _front = Mathf.Max(_front, Vector3.Dot(_wholeShape[i], _exitDirection));
            }

            StandUpright(_alignSeconds);
            DOTween.Sequence()
                .Append(transform.DOMove(doorstep, _alignSeconds))
                .Append(transform.DOMove(doorstep + offset, _leaveSeconds).SetEase(Ease.InQuad))
                .SetTarget(transform)
                .SetLink(gameObject)
                .OnUpdate(GoThroughDoor)
                .OnComplete(RaiseLeft);
        }

        /// <summary>Stops what the block is doing and gives up its mesh, before the object goes back to the pool.</summary>
        public void Release()
        {
            StopMoving();
            _exitEffect = null;
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

        /// <summary>
        /// Flattens every vertex that has passed the door line onto it, and has the cubes come out for
        /// what has passed. The mesh is the block's own, so nothing has to be put back.
        /// </summary>
        private void GoThroughDoor()
        {
            float passed = Vector3.Dot(transform.position, _exitDirection) - _doorstep;
            float limit = _front - passed;
            _cutShape.Clear();
            for (int i = 0; i < _wholeShape.Count; i++)
            {
                Vector3 vertex = _wholeShape[i];
                float past = Vector3.Dot(vertex, _exitDirection) - limit;
                _cutShape.Add(past > 0f ? vertex - _exitDirection * past : vertex);
            }

            _mesh.SetVertices(_cutShape);
            _exitEffect.ShowPassed(passed);
        }

        /// <summary>Leans towards the way the block has moved since the last frame: its leading edge dips.</summary>
        private void LeanWith(Vector3 moved)
        {
            float seconds = Time.deltaTime;
            if (seconds <= 0f)
            {
                return;
            }

            // The lean is limited after it is smoothed: a finger reports its moves less often than frames
            // are drawn, so the speed of a single frame says little.
            var wanted = new Vector2(moved.z, -moved.x) * (_leanPerSpeed / seconds);
            _lean = Vector2.ClampMagnitude(Vector2.Lerp(_lean, wanted, seconds / _leanSeconds), _maxLean);
            _body.localRotation = Quaternion.Euler(_lean.x, 0f, _lean.y);
        }

        private Tween StandUpright(float seconds)
        {
            _lean = Vector2.zero;
            return _body.DOLocalRotateQuaternion(Quaternion.identity, seconds).SetLink(gameObject);
        }

        private void StopMoving()
        {
            transform.DOKill();
            _body.DOKill();
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
