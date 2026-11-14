using System;
using ColorBlockJam.Core.Players;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Game.UI
{
    /// <summary>
    /// The settings screen: one switch for each setting and a close button. Opened from a level it
    /// is also the pause screen, and then it offers the way back to the home screen.
    /// </summary>
    public sealed class SettingsPopup : Popup
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _homeButton;
        [SerializeField] private SettingToggle[] _toggles;

        public event Action ClosePressed;
        public event Action HomePressed;
        public event Action<GameSetting> TogglePressed;

        public void ShowSetting(GameSetting setting, bool isOn)
        {
            for (int i = 0; i < _toggles.Length; i++)
            {
                if (_toggles[i].Setting == setting)
                {
                    _toggles[i].Show(isOn);
                }
            }
        }

        public void ShowHomeButton(bool isShown)
        {
            _homeButton.gameObject.SetActive(isShown);
        }

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(RaiseClosePressed);
            _homeButton.onClick.AddListener(RaiseHomePressed);
            for (int i = 0; i < _toggles.Length; i++)
            {
                _toggles[i].Pressed += RaiseTogglePressed;
            }
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(RaiseClosePressed);
            _homeButton.onClick.RemoveListener(RaiseHomePressed);
            for (int i = 0; i < _toggles.Length; i++)
            {
                _toggles[i].Pressed -= RaiseTogglePressed;
            }
        }

        private void RaiseClosePressed()
        {
            ClosePressed?.Invoke();
        }

        private void RaiseHomePressed()
        {
            HomePressed?.Invoke();
        }

        private void RaiseTogglePressed(GameSetting setting)
        {
            TogglePressed?.Invoke(setting);
        }
    }
}
