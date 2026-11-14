using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Game.UI
{
    /// <summary>Shown when the time ran out: try again or go home.</summary>
    public sealed class FailPopup : Popup
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _homeButton;

        public event Action RestartPressed;
        public event Action HomePressed;

        private void OnEnable()
        {
            _restartButton.onClick.AddListener(RaiseRestartPressed);
            _homeButton.onClick.AddListener(RaiseHomePressed);
        }

        private void OnDisable()
        {
            _restartButton.onClick.RemoveListener(RaiseRestartPressed);
            _homeButton.onClick.RemoveListener(RaiseHomePressed);
        }

        private void RaiseRestartPressed()
        {
            RestartPressed?.Invoke();
        }

        private void RaiseHomePressed()
        {
            HomePressed?.Invoke();
        }
    }
}
