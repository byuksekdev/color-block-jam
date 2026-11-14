using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>A number changed one step at a time with a minus and a plus button.</summary>
    internal sealed class Stepper : VisualElement
    {
        private readonly int _min;
        private readonly int _max;
        private readonly Label _valueLabel;

        public Stepper(string title, int min, int max)
        {
            _min = min;
            _max = max;
            AddToClassList("stepper");

            var titleLabel = new Label(title);
            titleLabel.AddToClassList("stepper-title");
            Add(titleLabel);

            Add(new Button(() => Step(-1)) { text = "-" });
            _valueLabel = new Label();
            _valueLabel.AddToClassList("stepper-value");
            Add(_valueLabel);
            Add(new Button(() => Step(1)) { text = "+" });
        }

        public event Action<int> Changed;

        public int Value { get; private set; }

        public void SetValueWithoutNotify(int value)
        {
            Value = value;
            _valueLabel.text = value.ToString(CultureInfo.InvariantCulture);
        }

        private void Step(int amount)
        {
            int value = Mathf.Clamp(Value + amount, _min, _max);
            if (value == Value)
            {
                return;
            }

            SetValueWithoutNotify(value);
            Changed?.Invoke(value);
        }
    }
}
