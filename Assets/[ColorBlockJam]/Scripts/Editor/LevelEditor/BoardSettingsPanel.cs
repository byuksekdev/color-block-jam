using System;
using ColorBlockJam.Core.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>Board width, board height and the timer.</summary>
    internal sealed class BoardSettingsPanel : VisualElement
    {
        private const int SecondsPerMinute = 60;
        private const int MaxMinutes = 99;

        private readonly Stepper _width;
        private readonly Stepper _height;
        private readonly IntegerField _minutes;
        private readonly IntegerField _seconds;

        public BoardSettingsPanel(int minBoardSize, int maxBoardSize)
        {
            AddToClassList("panel");

            var title = new Label("Board");
            title.AddToClassList("panel-title");
            Add(title);

            _width = new Stepper("Width", minBoardSize, maxBoardSize);
            _height = new Stepper("Height", minBoardSize, maxBoardSize);
            _width.Changed += width => SizeChosen?.Invoke(width, _height.Value);
            _height.Changed += height => SizeChosen?.Invoke(_width.Value, height);
            Add(_width);
            Add(_height);

            var timer = new VisualElement();
            timer.AddToClassList("timer-row");
            var timerTitle = new Label("Timer");
            timerTitle.AddToClassList("stepper-title");
            timer.Add(timerTitle);

            _minutes = TimeField(timer, "min");
            _seconds = TimeField(timer, "s");
            Add(timer);
        }

        public event Action<int, int> SizeChosen;
        public event Action<int> TimeChosen;

        public void Refresh(LevelData level)
        {
            _width.SetValueWithoutNotify(level.Width);
            _height.SetValueWithoutNotify(level.Height);
            _minutes.SetValueWithoutNotify(level.TimeLimitSeconds / SecondsPerMinute);
            _seconds.SetValueWithoutNotify(level.TimeLimitSeconds % SecondsPerMinute);
        }

        private IntegerField TimeField(VisualElement row, string unit)
        {
            var field = new IntegerField { isDelayed = true };
            field.AddToClassList("time-field");
            field.RegisterValueChangedCallback(_ => OnTimeEdited());
            row.Add(field);
            row.Add(new Label(unit));
            return field;
        }

        private void OnTimeEdited()
        {
            int minutes = Mathf.Clamp(_minutes.value, 0, MaxMinutes);
            int seconds = Mathf.Clamp(_seconds.value, 0, SecondsPerMinute - 1);
            TimeChosen?.Invoke(minutes * SecondsPerMinute + seconds);
        }
    }
}
