using System.Collections;
using ColorBlockJam.Core.Flow;
using ColorBlockJam.Core.Players;
using ColorBlockJam.Core.Sessions;
using UnityEngine;

namespace ColorBlockJam.Game.UI
{
    /// <summary>
    /// Shows the popup that fits what the level is doing and passes each button press on to the game.
    /// The screens themselves only show values and report presses.
    /// </summary>
    public sealed class GameScreens : MonoBehaviour
    {
        [SerializeField] private GameplayHud _hud;
        [SerializeField] private FailPopup _failPopup;
        [SerializeField] private LevelCompletePopup _completePopup;
        [SerializeField] private SettingsPopup _settingsPopup;

        [Tooltip("Seconds between the win and its popup, so that the last block is seen leaving through its door.")]
        [SerializeField] private float _completePopupDelaySeconds = 0.6f;

        private WaitForSeconds _completePopupDelay;
        private SettingsConnection _settings;
        private GameFlow _flow;
        private PlayerProfile _player;
        private LevelSession _session;

        public void Bind(GameFlow flow, PlayerProfile player)
        {
            _flow = flow;
            _player = player;
            _settings = new SettingsConnection(_settingsPopup, player);
            _completePopupDelay = new WaitForSeconds(_completePopupDelaySeconds);

            _flow.LevelStarted += OnLevelStarted;
            _flow.LevelWon += OnLevelWon;
            _flow.LevelLeft += OnLevelLeft;
            _player.CoinsChanged += _hud.ShowCoins;

            _settingsPopup.ClosePressed += ResumeLevel;
            _settingsPopup.HomePressed += _flow.LeaveLevel;
            _hud.RestartPressed += _flow.RestartLevel;
            _hud.PausePressed += PauseLevel;
            _failPopup.RestartPressed += _flow.RestartLevel;
            _failPopup.HomePressed += _flow.LeaveLevel;
            _completePopup.ContinuePressed += _flow.StartNextLevel;

            _hud.ShowCoins(_player.Coins);
            _settingsPopup.ShowHomeButton(true);
            HidePopups();
        }

        private void OnDestroy()
        {
            ReleaseSession();
            if (_flow == null)
            {
                return;
            }

            _settings.Release();
            _flow.LevelStarted -= OnLevelStarted;
            _flow.LevelWon -= OnLevelWon;
            _flow.LevelLeft -= OnLevelLeft;
            _player.CoinsChanged -= _hud.ShowCoins;
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
        }

        private void OnLevelLeft()
        {
            ReleaseSession();
            HidePopups();
        }

        /// <summary>In a level the settings screen is the pause screen: it is open exactly while the session is paused.</summary>
        private void OnSessionStateChanged(SessionState state)
        {
            if (state == SessionState.Paused)
            {
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

        private IEnumerator ShowCompletePopupAfterDelay()
        {
            yield return _completePopupDelay;
            _completePopup.Show();
        }

        private void HidePopups()
        {
            StopAllCoroutines();
            _failPopup.Hide();
            _completePopup.Hide();
            _settingsPopup.Hide();
        }

        private void PauseLevel()
        {
            _session.Pause();
        }

        private void ResumeLevel()
        {
            _session.Resume();
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
