using System;
using ColorBlockJam.Core.Players;

namespace ColorBlockJam.Game.UI
{
    /// <summary>
    /// Connects a settings screen to the player's settings: a press on a switch flips the setting,
    /// and the screen shows every change. Each scene has its own settings screen and its own connection.
    /// </summary>
    public sealed class SettingsConnection
    {
        private static readonly GameSetting[] Settings = (GameSetting[])Enum.GetValues(typeof(GameSetting));

        private readonly SettingsPopup _popup;
        private readonly PlayerProfile _player;

        public SettingsConnection(SettingsPopup popup, PlayerProfile player)
        {
            _popup = popup;
            _player = player;

            _popup.TogglePressed += _player.Toggle;
            _player.SettingChanged += _popup.ShowSetting;
            for (int i = 0; i < Settings.Length; i++)
            {
                _popup.ShowSetting(Settings[i], _player.IsOn(Settings[i]));
            }
        }

        public void Release()
        {
            _popup.TogglePressed -= _player.Toggle;
            _player.SettingChanged -= _popup.ShowSetting;
        }
    }
}
