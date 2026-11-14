using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Game.UI
{
    /// <summary>Shown when the level is won: the reward and the way to the next level.</summary>
    public sealed class LevelCompletePopup : Popup
    {
        private const string RewardPrefix = "+";

        [SerializeField] private Button _continueButton;
        [SerializeField] private GameObject _reward;
        [SerializeField] private TMP_Text _rewardLabel;

        public event Action ContinuePressed;

        /// <summary>A win that earned nothing, such as a level played again, shows no reward.</summary>
        public void ShowReward(int coins)
        {
            _reward.SetActive(coins > 0);
            _rewardLabel.text = RewardPrefix + coins.ToString(CultureInfo.InvariantCulture);
        }

        private void OnEnable()
        {
            _continueButton.onClick.AddListener(RaiseContinuePressed);
        }

        private void OnDisable()
        {
            _continueButton.onClick.RemoveListener(RaiseContinuePressed);
        }

        private void RaiseContinuePressed()
        {
            ContinuePressed?.Invoke();
        }
    }
}
