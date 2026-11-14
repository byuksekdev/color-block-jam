using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Players;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public class PlayerProfileTests
    {
        private static readonly GameSetting[] Settings = (GameSetting[])Enum.GetValues(typeof(GameSetting));

        [Test]
        public void Constructor_NothingStored_StartsWith400CoinsOnTheFirstLevel()
        {
            var profile = new PlayerProfile(new FakePlayerStorage());

            Assert.AreEqual(400, profile.Coins);
            Assert.AreEqual(0, profile.LevelIndex);
        }

        [Test]
        public void Constructor_StoredPlayer_ContinuesFromTheStoredValues()
        {
            var storage = new FakePlayerStorage();
            storage.Save(new PlayerData { Coins = 520, LevelIndex = 3 });

            var profile = new PlayerProfile(storage);

            Assert.AreEqual(520, profile.Coins);
            Assert.AreEqual(3, profile.LevelIndex);
        }

        [Test]
        public void CompleteLevel_Once_Adds40CoinsAndMovesToTheNextLevel()
        {
            var profile = new PlayerProfile(new FakePlayerStorage());
            var coinCounts = new List<int>();
            var levels = new List<int>();
            profile.CoinsChanged += coinCounts.Add;
            profile.LevelChanged += levels.Add;

            profile.CompleteLevel();

            Assert.AreEqual(440, profile.Coins);
            Assert.AreEqual(1, profile.LevelIndex);
            CollectionAssert.AreEqual(new[] { 440 }, coinCounts);
            CollectionAssert.AreEqual(new[] { 1 }, levels);
        }

        [Test]
        public void CompleteLevel_ThenTheGameStartsAgain_KeepsCoinsAndLevel()
        {
            var storage = new FakePlayerStorage();
            new PlayerProfile(storage).CompleteLevel();

            var profile = new PlayerProfile(storage);

            Assert.AreEqual(440, profile.Coins);
            Assert.AreEqual(1, profile.LevelIndex);
        }

        [Test]
        public void IsOn_NewPlayer_EverySettingIsOn()
        {
            var profile = new PlayerProfile(new FakePlayerStorage());

            foreach (GameSetting setting in Settings)
            {
                Assert.IsTrue(profile.IsOn(setting), setting + " should start on.");
            }
        }

        [Test]
        public void Toggle_OneSetting_SwitchesOnlyThatSettingOff([ValueSource(nameof(Settings))] GameSetting toggled)
        {
            var profile = new PlayerProfile(new FakePlayerStorage());

            profile.Toggle(toggled);

            foreach (GameSetting setting in Settings)
            {
                Assert.AreEqual(setting != toggled, profile.IsOn(setting), setting.ToString());
            }
        }

        [Test]
        public void Toggle_Twice_SwitchesTheSettingOnAgain([ValueSource(nameof(Settings))] GameSetting setting)
        {
            var profile = new PlayerProfile(new FakePlayerStorage());
            var changes = new List<bool>();
            profile.SettingChanged += (changed, isOn) => changes.Add(isOn);

            profile.Toggle(setting);
            profile.Toggle(setting);

            Assert.IsTrue(profile.IsOn(setting));
            CollectionAssert.AreEqual(new[] { false, true }, changes);
        }

        [Test]
        public void Toggle_ThenTheGameStartsAgain_KeepsTheSetting([ValueSource(nameof(Settings))] GameSetting setting)
        {
            var storage = new FakePlayerStorage();
            new PlayerProfile(storage).Toggle(setting);

            var profile = new PlayerProfile(storage);

            Assert.IsFalse(profile.IsOn(setting));
        }
    }
}
