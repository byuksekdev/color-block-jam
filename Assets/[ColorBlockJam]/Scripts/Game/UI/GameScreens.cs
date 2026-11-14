using System;
using System.Collections;
using ColorBlockJam.Core.Flow;
using ColorBlockJam.Core.Players;
using ColorBlockJam.Core.Sessions;
using UnityEngine;

namespace ColorBlockJam.Game.UI
{
    /// <summary>
    /// Shows the screen that fits what the game is doing and passes each button press on to the game.
    /// The screens themselves only show values and report presses.
    /// </summary>
    public sealed class GameScreens : MonoBehaviour
    {
        private const float CompletePopupDelaySeconds = 0.6f;

        private static readonly GameSetting[] Settings = (GameSetting[])Enum.GetValues(typeof(GameSetting));

        [SerializeField] private HomeScreen _home;
        [SerializeField] private GameplayHud _hud;
        [SerializeField] private FailPopup _failPopup;
        [SerializeField] private LevelCompletePopup _completePopup;
        [SerializeField] private SettingsPopup _settingsPopup;

        private readonly WaitForSeconds _completePopupDelay = new WaitForSeconds(CompletePopupDelaySeconds);
        private GameFlow _flow;
        private PlayerProfile _player;
        private LevelSession _session;

        public void Bind(GameFlow flow, PlayerProfile player)
        {
            _flow = flow;
            _player = player;

            _flow.LevelStarted += OnLevelStarted;
            _flow.LevelWon += OnLevelWon;
            _flow.LevelLeft += ShowHome;
            _player.CoinsChanged += ShowCoins;
            _player.LevelChanged += ShowPlayerLevel;
            _player.SettingChanged += _settingsPopup.ShowSetting;

            _home.PlayPressed += _flow.StartLevel;
            _home.SettingsPressed += OpenSettingsAtHome;
            _settingsPopup.ClosePressed += CloseSettings;
            _settingsPopup.HomePressed += _flow.LeaveLevel;
            _settingsPopup.TogglePressed += _player.Toggle;
            _hud.RestartPressed += _flow.RestartLevel;
            _hud.PausePressed += PauseLevel;
            _failPopup.RestartPressed += _flow.RestartLevel;
            _failPopup.HomePressed += _flow.LeaveLevel;
            _completePopup.ContinuePressed += _flow.StartNextLevel;

            ShowCoins(_player.Coins);
            ShowPlayerLevel(_player.LevelIndex);
            for (int i = 0; i < Settings.Length; i++)
            {
                _settingsPopup.ShowSetting(Settings[i], _player.IsOn(Settings[i]));
            }

            ShowHome();
        }

        private void OnDestroy()
        {
            ReleaseSession();
            if (_flow == null)
            {
                return;
            }

            _flow.LevelStarted -= OnLevelStarted;
            _flow.LevelWon -= OnLevelWon;
            _flow.LevelLeft -= ShowHome;
            _player.CoinsChanged -= ShowCoins;
            _player.LevelChanged -= ShowPlayerLevel;
            _player.SettingChanged -= _settingsPopup.ShowSetting;
        }

        private void OnLevelStarted(LevelSession session)
        {
            ReleaseSession();
            _session = session;
            _session.StateChanged += OnSessionStateChanged;
            _session.SecondsLeftChanged += _hud.ShowTime;

            _hud.ShowLevel(_flow.LevelIndex + 1);
            _hud.ShowTime(_session.SecondsLeft);

            HidePopups();
            _home.gameObject.SetActive(false);
            _hud.gameObject.SetActive(true);
        }

        private void OnSessionStateChanged(SessionState state)
        {
            if (state == SessionState.Paused)
            {
                _settingsPopup.ShowHomeButton(true);
                _settingsPopup.Show();
            }
            else if (state == SessionState.Failed)
            {
                _failPopup.Show();
            }
            else if (state == SessionState.Ready || state == SessionState.Playing)
            {
                _settingsPopup.Hide();
            }
        }

        private void OnLevelWon(int reward)
        {
            _completePopup.ShowReward(reward);
            StartCoroutine(ShowCompletePopupAfterDelay());
        }

        /// <summary>Waits until the last block has left through its door.</summary>
        private IEnumerator ShowCompletePopupAfterDelay()
        {
            yield return _completePopupDelay;
            _completePopup.Show();
        }

        private void ShowHome()
        {
            ReleaseSession();
            HidePopups();
            _hud.gameObject.SetActive(false);
            _home.gameObject.SetActive(true);
        }

        private void HidePopups()
        {
            StopAllCoroutines();
            _failPopup.Hide();
            _completePopup.Hide();
            _settingsPopup.Hide();
        }

        private void OpenSettingsAtHome()
        {
            _settingsPopup.ShowHomeButton(false);
            _settingsPopup.Show();
        }

        /// <summary>In a level the settings screen is the pause screen, so closing it resumes the level.</summary>
        private void CloseSettings()
        {
            if (_session == null)
            {
                _settingsPopup.Hide();
            }
            else
            {
                _session.Resume();
            }
        }

        private void PauseLevel()
        {
            _session.Pause();
        }

        private void ShowCoins(int coins)
        {
            _home.ShowCoins(coins);
            _hud.ShowCoins(coins);
        }

        private void ShowPlayerLevel(int levelIndex)
        {
            _home.ShowLevel(levelIndex + 1);
        }

        private void ReleaseSession()
        {
            if (_session == null)
            {
                return;
            }

            _session.StateChanged -= OnSessionStateChanged;
            _session.SecondsLeftChanged -= _hud.ShowTime;
            _session = null;
        }
    }
}
