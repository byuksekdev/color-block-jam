using UnityEngine;

namespace ColorBlockJam.Game.UI
{
    /// <summary>
    /// Keeps a panel out from under the notch and the home indicator. The panel must stretch over
    /// its whole canvas; this moves its top and bottom anchors to the safe part of the screen.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour
    {
        [SerializeField] private bool _fitsTop = true;
        [SerializeField] private bool _fitsBottom = true;

        private RectTransform _rect;
        private Rect _appliedArea;
        private int _appliedScreenHeight;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            Apply();
        }

        /// <summary>The canvas tells its children when the screen changes size, which is also when the safe area can change.</summary>
        private void OnRectTransformDimensionsChange()
        {
            if (_rect != null && (Screen.safeArea != _appliedArea || Screen.height != _appliedScreenHeight))
            {
                Apply();
            }
        }

        private void Apply()
        {
            _appliedArea = Screen.safeArea;
            _appliedScreenHeight = Screen.height;

            float bottom = _fitsBottom ? _appliedArea.yMin / _appliedScreenHeight : 0f;
            float top = _fitsTop ? _appliedArea.yMax / _appliedScreenHeight : 1f;

            _rect.anchorMin = new Vector2(0f, bottom);
            _rect.anchorMax = new Vector2(1f, top);
        }
    }
}
