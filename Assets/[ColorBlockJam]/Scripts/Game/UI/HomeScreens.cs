using System;
using ColorBlockJam.Core.Players;
using UnityEngine;

namespace ColorBlockJam.Game.UI
{
    /// <summary>
    /// Shows the player's level and coins on the home screen and opens the settings over it.
    /// The screens themselves only show values and report presses.
    /// </summary>
    public sealed class HomeScreens : MonoBehaviour
    {
        [SerializeField] private HomeScreen _home;
        [SerializeField] private SettingsPopup _settingsPopup;

        private SettingsConnection _settings;

        public event Action PlayPressed;

        public void Bind(PlayerProfile player)
        {
            _settings = new SettingsConnection(_settingsPopup, player);

            _home.PlayPressed += RaisePlayPressed;
            _home.SettingsPressed += _settingsPopup.Show;
            _settingsPopup.ClosePressed += _settingsPopup.Hide;

            _home.ShowCoins(player.Coins);
            _home.ShowLevel(player.LevelIndex + 1);
            _settingsPopup.ShowHomeButton(false);
            _settingsPopup.Hide();
        }

        private void OnDestroy()
        {
            if (_settings != null)
            {
                _settings.Release();
            }
        }

        private void RaisePlayPressed()
        {
            PlayPressed?.Invoke();
        }
    }
}
