using System;
using ColorBlockJam.Core.Players;
using UnityEngine;

namespace ColorBlockJam.Game.Persistence
{
    /// <summary>Keeps the player's data as one JSON text in PlayerPrefs.</summary>
    public sealed class PlayerPrefsStorage : IPlayerStorage
    {
        private const string DefaultKey = "ColorBlockJam.Player";

        private readonly string _key;

        public PlayerPrefsStorage()
            : this(DefaultKey)
        {
        }

        public PlayerPrefsStorage(string key)
        {
            _key = key;
        }

        public bool TryLoad(out PlayerData data)
        {
            data = null;
            string json = PlayerPrefs.GetString(_key, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                data = JsonUtility.FromJson<PlayerData>(json);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return data != null && data.Version == PlayerData.CurrentVersion && data.Coins >= 0 && data.LevelIndex >= 0;
        }

        public void Save(PlayerData data)
        {
            PlayerPrefs.SetString(_key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }
}
