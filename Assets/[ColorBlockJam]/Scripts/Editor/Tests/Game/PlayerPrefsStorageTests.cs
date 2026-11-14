using ColorBlockJam.Core.Players;
using ColorBlockJam.Game.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace ColorBlockJam.Tests.Game
{
    public class PlayerPrefsStorageTests
    {
        private const string TestKey = "ColorBlockJam.Tests.Player";

        [SetUp]
        [TearDown]
        public void RemoveTestData()
        {
            PlayerPrefs.DeleteKey(TestKey);
        }

        [Test]
        public void TryLoad_NothingSaved_FindsNoPlayer()
        {
            var storage = new PlayerPrefsStorage(TestKey);

            Assert.IsFalse(storage.TryLoad(out _));
        }

        [Test]
        public void Save_ThenLoad_GivesTheSameValues()
        {
            var saved = new PlayerData { Coins = 480, LevelIndex = 2 };
            saved.SettingsOff.Add(GameSetting.Music);
            new PlayerPrefsStorage(TestKey).Save(saved);

            bool found = new PlayerPrefsStorage(TestKey).TryLoad(out PlayerData loaded);

            Assert.IsTrue(found);
            Assert.AreEqual(480, loaded.Coins);
            Assert.AreEqual(2, loaded.LevelIndex);
            CollectionAssert.AreEqual(new[] { GameSetting.Music }, loaded.SettingsOff);
        }

        [Test]
        public void TryLoad_TextThatIsNotJson_FindsNoPlayer()
        {
            PlayerPrefs.SetString(TestKey, "not a player");

            Assert.IsFalse(new PlayerPrefsStorage(TestKey).TryLoad(out _));
        }

        [Test]
        public void TryLoad_DataOfAnotherVersion_FindsNoPlayer()
        {
            new PlayerPrefsStorage(TestKey).Save(new PlayerData { Version = PlayerData.CurrentVersion + 1, Coins = 480 });

            Assert.IsFalse(new PlayerPrefsStorage(TestKey).TryLoad(out _));
        }

        [Test]
        public void TryLoad_NegativeCoins_FindsNoPlayer()
        {
            new PlayerPrefsStorage(TestKey).Save(new PlayerData { Coins = -1 });

            Assert.IsFalse(new PlayerPrefsStorage(TestKey).TryLoad(out _));
        }

        [Test]
        public void TryLoad_TextWithoutSettings_KeepsEverySettingOn()
        {
            PlayerPrefs.SetString(TestKey, "{\"Version\":1,\"Coins\":440,\"LevelIndex\":1}");

            bool found = new PlayerPrefsStorage(TestKey).TryLoad(out PlayerData loaded);

            Assert.IsTrue(found);
            CollectionAssert.IsEmpty(loaded.SettingsOff);
        }
    }
}
