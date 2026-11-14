using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>Places the camera so that a board of any size fits the part of the screen that is left for it.</summary>
    [RequireComponent(typeof(Camera))]
    public sealed class BoardCamera : MonoBehaviour
    {
        private const float FrameThickness = 1f;
        private const float MinDistance = 4f;
        private const float MaxDistance = 200f;
        private const int SearchSteps = 16;
        private const int BoundsCorners = 8;

        [SerializeField] private float _pitch = 72f;

        [Tooltip("The part of the screen the board may fill, as fractions of the screen: x, y, width, height.")]
        [SerializeField] private Rect _boardArea = new Rect(0.04f, 0.2f, 0.92f, 0.58f);

        private readonly Vector3[] _corners = new Vector3[BoundsCorners];
        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        public void Frame(BoardSpace space)
        {
            Vector3 margin = new Vector3(FrameThickness, 0f, FrameThickness);
            Vector3 min = space.Min - margin;
            Vector3 max = space.Max + margin + Vector3.up * BoardView.BlockTop;
            for (int i = 0; i < BoundsCorners; i++)
            {
                _corners[i] = new Vector3((i & 1) == 0 ? min.x : max.x, (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z);
            }

            transform.rotation = Quaternion.Euler(_pitch, 0f, 0f);

            float near = MinDistance;
            float far = MaxDistance;
            for (int step = 0; step < SearchSteps; step++)
            {
                float distance = (near + far) / 2f;
                if (Fits(space.Center, distance))
                {
                    far = distance;
                }
                else
                {
                    near = distance;
                }
            }

            Fits(space.Center, far);
        }

        /// <summary>Puts the camera at this distance, centres the board in its area and says whether the board fits.</summary>
        private bool Fits(Vector3 target, float distance)
        {
            transform.position = target - transform.forward * distance;

            Rect seen = ViewportBounds();
            Vector3 offset = _camera.ViewportToWorldPoint(new Vector3(seen.center.x, seen.center.y, distance))
                - _camera.ViewportToWorldPoint(new Vector3(_boardArea.center.x, _boardArea.center.y, distance));
            transform.position += offset;

            seen = ViewportBounds();
            return seen.width <= _boardArea.width && seen.height <= _boardArea.height;
        }

        private Rect ViewportBounds()
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < BoundsCorners; i++)
            {
                Vector2 point = _camera.WorldToViewportPoint(_corners[i]);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
