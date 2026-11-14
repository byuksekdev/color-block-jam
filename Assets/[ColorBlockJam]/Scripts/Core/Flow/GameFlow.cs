using System;
using ColorBlockJam.Core.Players;
using ColorBlockJam.Core.Sessions;

namespace ColorBlockJam.Core.Flow
{
    /// <summary>
    /// Moves the player between the home screen and the levels. Every start, restart and next level
    /// is a new session built from the level data, so nothing of an earlier attempt remains.
    /// </summary>
    public sealed class GameFlow
    {
        private readonly PlayerProfile _player;
        private readonly ILevelSource _levels;
        private LevelSession _session;

        public GameFlow(PlayerProfile player, ILevelSource levels)
        {
            _player = player;
            _levels = levels;
        }

        /// <summary>A new attempt began. The session is new each time.</summary>
        public event Action<LevelSession> LevelStarted;

        /// <summary>The level was won. The number is the coins the win earned, which is zero for a level played again.</summary>
        public event Action<int> LevelWon;

        /// <summary>The player went back to the home screen.</summary>
        public event Action LevelLeft;

        /// <summary>The index of the level being played. It keeps counting after the last level file.</summary>
        public int LevelIndex { get; private set; }

        public bool IsInLevel => _session != null;

        /// <summary>Starts the level the player has reached.</summary>
        public void StartLevel()
        {
            StartLevel(_player.LevelIndex);
        }

        /// <summary>Starts any level. After the last level file the levels repeat from the first.</summary>
        public void StartLevel(int levelIndex)
        {
            if (_levels.Count == 0)
            {
                throw new InvalidOperationException("There is no level to play: the level list is empty.");
            }

            ReleaseSession();
            LevelIndex = levelIndex;
            _session = new LevelSession(_levels.Load(levelIndex % _levels.Count));
            _session.StateChanged += OnStateChanged;
            LevelStarted?.Invoke(_session);
        }

        public void RestartLevel()
        {
            StartLevel(LevelIndex);
        }

        public void StartNextLevel()
        {
            StartLevel(LevelIndex + 1);
        }

        public void LeaveLevel()
        {
            if (_session == null)
            {
                return;
            }

            ReleaseSession();
            LevelLeft?.Invoke();
        }

        private void ReleaseSession()
        {
            if (_session == null)
            {
                return;
            }

            _session.StateChanged -= OnStateChanged;
            _session = null;
        }

        private void OnStateChanged(SessionState state)
        {
            if (state != SessionState.Won)
            {
                return;
            }

            int reward = 0;
            if (LevelIndex == _player.LevelIndex)
            {
                _player.CompleteLevel();
                reward = PlayerProfile.LevelReward;
            }

            LevelWon?.Invoke(reward);
        }
    }
}
