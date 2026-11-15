using UnityEngine;

namespace ColorBlockJam.Game.UI
{
    /// <summary>How the UI moves: the press of a button and the opening of a popup. One asset, so that every screen feels the same.</summary>
    public sealed class UiMotion : ScriptableObject
    {
        [Header("Button press")]
        [SerializeField] private float _pressedScale = 0.9f;
        [SerializeField] private float _pressSeconds = 0.08f;

        [Header("Popup opening")]
        [SerializeField] private float _popupStartScale = 0.85f;
        [SerializeField] private float _popupSeconds = 0.2f;

        public float PressedScale => _pressedScale;
        public float PressSeconds => _pressSeconds;
        public float PopupStartScale => _popupStartScale;
        public float PopupSeconds => _popupSeconds;
    }
}
