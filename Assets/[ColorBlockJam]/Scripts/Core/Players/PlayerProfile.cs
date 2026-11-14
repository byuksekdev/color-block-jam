using System;

namespace ColorBlockJam.Core.Players
{
    /// <summary>
    /// The player's coins, progress and settings. Every change is saved at once, so leaving the game
    /// at any moment loses nothing.
    /// </summary>
    public sealed class PlayerProfile
    {
        public const int StartingCoins = 400;
        public const int LevelReward = 40;

        private readonly IPlayerStorage _storage;
        private readonly PlayerData _data;

        public PlayerProfile(IPlayerStorage storage)
        {
            _storage = storage;
            if (!storage.TryLoad(out _data))
            {
                _data = new PlayerData { Coins = StartingCoins };
            }
        }

        public event Action<int> CoinsChanged;
        public event Action<int> LevelChanged;
        public event Action<GameSetting, bool> SettingChanged;

        public int Coins => _data.Coins;

        /// <summary>The index of the level the player plays next. It keeps counting after the last level file.</summary>
        public int LevelIndex => _data.LevelIndex;

        public bool IsOn(GameSetting setting)
        {
            return !_data.SettingsOff.Contains(setting);
        }

        /// <summary>Moves the player to the next level and grants the reward.</summary>
        public void CompleteLevel()
        {
            _data.LevelIndex++;
            _data.Coins += LevelReward;
            _storage.Save(_data);

            CoinsChanged?.Invoke(_data.Coins);
            LevelChanged?.Invoke(_data.LevelIndex);
        }

        public void Toggle(GameSetting setting)
        {
            bool isOn = !IsOn(setting);
            if (isOn)
            {
                _data.SettingsOff.Remove(setting);
            }
            else
            {
                _data.SettingsOff.Add(setting);
            }

            _storage.Save(_data);
            SettingChanged?.Invoke(setting, isOn);
        }
    }
}
