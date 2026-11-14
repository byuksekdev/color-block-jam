using ColorBlockJam.Core.Players;

namespace ColorBlockJam.Tests
{
    /// <summary>Keeps a copy of what was saved, as a real storage does, so a test cannot pass by sharing one object.</summary>
    internal sealed class FakePlayerStorage : IPlayerStorage
    {
        private PlayerData _saved;

        public int SaveCount { get; private set; }

        public bool TryLoad(out PlayerData data)
        {
            data = _saved == null ? null : Copy(_saved);
            return data != null;
        }

        public void Save(PlayerData data)
        {
            _saved = Copy(data);
            SaveCount++;
        }

        private static PlayerData Copy(PlayerData data)
        {
            var copy = new PlayerData { Version = data.Version, Coins = data.Coins, LevelIndex = data.LevelIndex };
            copy.SettingsOff.AddRange(data.SettingsOff);
            return copy;
        }
    }
}
