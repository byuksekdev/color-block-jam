using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Game.UI
{
    /// <summary>
    /// Sets how a canvas scaler weighs the width of the screen against its height, by the shape of the
    /// screen: one weight for a screen narrower than the threshold, another for every other screen.
    /// Outside Play Mode it follows the Game view, so the layout can be checked for each screen shape.
    /// </summary>
    [ExecuteAlways]
    public sealed class CanvasAspectRatioScaler : MonoBehaviour
    {
        [SerializeField] private CanvasScaler _scaler;

        [Tooltip("Width and height of the screen shape that separates the two weights.")]
        [SerializeField] private Vector2 _aspectRatioThreshold = new Vector2(10f, 16f);

        [Tooltip("The scaler's match on a screen narrower than the threshold: 0 follows the width, 1 the height.")]
        [SerializeField, Range(0f, 1f)] private float _matchBelowThreshold = 0.35f;

        [Tooltip("The scaler's match on a screen as wide as the threshold or wider: 0 follows the width, 1 the height.")]
        [SerializeField, Range(0f, 1f)] private float _matchFromThreshold = 0.85f;

        private void Awake()
        {
            if (_scaler == null)
            {
                _scaler = GetComponent<CanvasScaler>();
            }

            ApplyMatch();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying || _scaler == null)
            {
                return;
            }

            ApplyMatch();
        }
#endif

        private void ApplyMatch()
        {
            if (_scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                return;
            }

            float aspectRatio = (float)Screen.width / Screen.height;
            float threshold = _aspectRatioThreshold.x / _aspectRatioThreshold.y;

            _scaler.matchWidthOrHeight = aspectRatio < threshold ? _matchBelowThreshold : _matchFromThreshold;
        }
    }
}
