using System;
using System.Collections.Generic;

namespace ColorBlockJam.Core.Players
{
    /// <summary>What is saved about the player between runs of the game.</summary>
    [Serializable]
    public sealed class PlayerData
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public int Coins;

        /// <summary>How many levels the player has completed, which is also the index of the level to play next.</summary>
        public int LevelIndex;

        /// <summary>Settings are on unless they are listed here, so a setting added later starts on.</summary>
        public List<GameSetting> SettingsOff = new List<GameSetting>();
    }
}
