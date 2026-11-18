using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Game.UI
{
    /// <summary>The home screen: the level button, the levels ahead, the coins and the way into the settings.</summary>
    public sealed class HomeScreen : MonoBehaviour
    {
        private const string LevelPrefix = "Level ";

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _coinsLabel;
        [SerializeField] private TMP_Text _currentLevelLabel;

        [Tooltip("The levels that come after the current one, nearest first.")]
        [SerializeField] private TMP_Text[] _nextLevelLabels;

        public event Action PlayPressed;
        public event Action SettingsPressed;

        public void ShowLevel(int levelNumber)
        {
            _levelLabel.text = LevelPrefix + levelNumber.ToString(CultureInfo.InvariantCulture);
            _currentLevelLabel.text = levelNumber.ToString(CultureInfo.InvariantCulture);
            for (int i = 0; i < _nextLevelLabels.Length; i++)
            {
                _nextLevelLabels[i].text = (levelNumber + i + 1).ToString(CultureInfo.InvariantCulture);
            }
        }

        public void ShowCoins(int coins)
        {
            _coinsLabel.text = coins.ToString(CultureInfo.InvariantCulture);
        }

        private void OnEnable()
        {
            _playButton.onClick.AddListener(RaisePlayPressed);
            _settingsButton.onClick.AddListener(RaiseSettingsPressed);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(RaisePlayPressed);
            _settingsButton.onClick.RemoveListener(RaiseSettingsPressed);
        }

        private void RaisePlayPressed()
        {
            PlayPressed?.Invoke();
        }

        private void RaiseSettingsPressed()
        {
            SettingsPressed?.Invoke();
        }
    }
}
