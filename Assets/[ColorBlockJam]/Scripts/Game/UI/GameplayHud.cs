using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Game.UI
{
    /// <summary>What the player sees around the board: the level, the time left, the coins, restart and pause.</summary>
    public sealed class GameplayHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _timeLabel;
        [SerializeField] private TMP_Text _coinsLabel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _pauseButton;

        public event Action RestartPressed;
        public event Action PausePressed;

        public void ShowLevel(int levelNumber)
        {
            _levelLabel.text = levelNumber.ToString(CultureInfo.InvariantCulture);
        }

        public void ShowTime(int secondsLeft)
        {
            _timeLabel.text = ClockText.Format(secondsLeft);
        }

        public void ShowCoins(int coins)
        {
            _coinsLabel.text = coins.ToString(CultureInfo.InvariantCulture);
        }

        private void OnEnable()
        {
            _restartButton.onClick.AddListener(RaiseRestartPressed);
            _pauseButton.onClick.AddListener(RaisePausePressed);
        }

        private void OnDisable()
        {
            _restartButton.onClick.RemoveListener(RaiseRestartPressed);
            _pauseButton.onClick.RemoveListener(RaisePausePressed);
        }

        private void RaiseRestartPressed()
        {
            RestartPressed?.Invoke();
        }

        private void RaisePausePressed()
        {
            PausePressed?.Invoke();
        }
    }
}
