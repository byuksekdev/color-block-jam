using System;
using ColorBlockJam.Core.Players;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Game.UI
{
    /// <summary>One on/off switch of the settings screen. It shows the state it is given and reports a press.</summary>
    public sealed class SettingToggle : MonoBehaviour
    {
        [SerializeField] private GameSetting _setting;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _onState;
        [SerializeField] private GameObject _offState;

        public event Action<GameSetting> Pressed;

        public GameSetting Setting => _setting;

        public void Show(bool isOn)
        {
            _onState.SetActive(isOn);
            _offState.SetActive(!isOn);
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(RaisePressed);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(RaisePressed);
        }

        private void RaisePressed()
        {
            Pressed?.Invoke(_setting);
        }
    }
}
