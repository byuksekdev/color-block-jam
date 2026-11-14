using UnityEngine;

namespace ColorBlockJam.Scripts.Utilities.UI
{
    [ExecuteAlways]
    public class SafeArea : MonoBehaviour
    {
        //-------Public Variables-------//

        //------Serialized Fields-------//
        [SerializeField] private bool Top = true;
        [SerializeField] private bool Bottom = true;

        //------Private Variables-------//
        private const float TOP_OFFSET_WHEN_HAVE_NOTCH = -0.025f;
        private const float BOTTOM_OFFSET_WHEN_HAVE_NOTCH = 0.02f;
        private RectTransform _rectTransform;
        private Rect _safeArea;
        private Vector2 _minAnchor;
        private Vector2 _maxAnchor;


        #region UNITY_METHODS

        private void Awake()
        {
            ApplySafeArea();
        }

#if UNITY_EDITOR
        private void Update()
        {
            ApplySafeArea();
        }
#endif

        #endregion


        #region PUBLIC_METHODS

        #endregion


        #region PRIVATE_METHODS

        private void ApplySafeArea()
        {
            _rectTransform = GetComponent<RectTransform>();

            Vector2 screenSize = GetReliableScreenSize();
            _safeArea = GetReliableSafeArea(screenSize);

            _minAnchor = Bottom ? _safeArea.position : Vector2.zero;
            _minAnchor.x = 0f;
            _maxAnchor = Top ? _safeArea.position + _safeArea.size : Vector2.one;
            _maxAnchor.x = 1f;
            if (Bottom)
            {
                _minAnchor.y /= screenSize.y;
                if (_minAnchor.y > 0.03f)
                    _minAnchor.y -= BOTTOM_OFFSET_WHEN_HAVE_NOTCH;
            }

            if (Top)
            {
                _maxAnchor.y /= screenSize.y;
                if (_maxAnchor.y < 0.97f)
                    _maxAnchor.y -= TOP_OFFSET_WHEN_HAVE_NOTCH;
            }

            _rectTransform.anchorMin = _minAnchor;
            _rectTransform.anchorMax = _maxAnchor;
        }

        private static Vector2 GetReliableScreenSize()
        {
#if UNITY_EDITOR
            // In edit mode, Screen.width/height return the focused EditorWindow's size
            // (often Inspector/Scene), not the Game view. Ask Unity directly for the
            // Game view target size via UnityStats so inspector buttons behave correctly.
            string res = UnityEditor.UnityStats.screenRes;
            if (!string.IsNullOrEmpty(res))
            {
                string[] parts = res.Split('x');
                if (parts.Length == 2
                    && int.TryParse(parts[0], out int w)
                    && int.TryParse(parts[1], out int h)
                    && w > 0 && h > 0)
                {
                    return new Vector2(w, h);
                }
            }
#endif
            return new Vector2(Screen.width, Screen.height);
        }

        private static Rect GetReliableSafeArea(Vector2 screenSize)
        {
            return Screen.safeArea;
        }

        #endregion
    }
}