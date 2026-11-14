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
        /// <summary>An inset smaller than this share of the screen height is rounding, not a notch.</summary>
        private const float SmallestInset = 0.03f;

        /// <summary>Phones report more room than the notch and the home indicator take; the panel takes this share of the screen height back.</summary>
        private const float TopReclaim = 0.025f;
        private const float BottomReclaim = 0.02f;

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

            if (bottom > SmallestInset)
            {
                bottom -= BottomReclaim;
            }

            if (top < 1f - SmallestInset)
            {
                top += TopReclaim;
            }

            _rect.anchorMin = new Vector2(0f, bottom);
            _rect.anchorMax = new Vector2(1f, top);
        }
    }
}
