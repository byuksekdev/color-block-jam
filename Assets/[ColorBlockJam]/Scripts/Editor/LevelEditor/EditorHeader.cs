using System;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>The bar above the board: the name of the open level, Save and Play.</summary>
    internal sealed class EditorHeader : VisualElement
    {
        private const string PlayTooltip = "Saves the level and plays it in the game.";
        private const string CannotPlayTooltip = "Fix the problems under Level check to play this level.";

        private readonly Label _title = new Label();
        private readonly Button _saveButton;
        private readonly Button _playButton;

        public EditorHeader()
        {
            AddToClassList("header");
            _title.AddToClassList("header-title");

            _saveButton = new Button(() => SaveRequested?.Invoke()) { text = "Save" };
            _playButton = new Button(() => PlayRequested?.Invoke()) { text = "Play" };
            _playButton.AddToClassList("play-button");

            Add(_title);
            Add(_saveButton);
            Add(_playButton);
        }

        public event Action SaveRequested;

        public event Action PlayRequested;

        public void Refresh(string title, bool canSave, bool canPlay)
        {
            _title.text = title;
            _saveButton.SetEnabled(canSave);
            _playButton.SetEnabled(canPlay);
            _playButton.tooltip = canPlay ? PlayTooltip : CannotPlayTooltip;
        }
    }
}
