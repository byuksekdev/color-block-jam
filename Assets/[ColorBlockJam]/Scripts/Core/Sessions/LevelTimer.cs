using System;

namespace ColorBlockJam.Core.Sessions
{
    public sealed class LevelTimer
    {
        public LevelTimer(float durationSeconds)
        {
            Remaining = MathF.Max(0f, durationSeconds);
        }

        public float Remaining { get; private set; }
        public bool IsExpired => Remaining <= 0f;

        /// <summary>The whole seconds to show: a started second counts as a full one, so zero means expired.</summary>
        public int SecondsLeft => (int)MathF.Ceiling(Remaining);

        public void Advance(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
            {
                return;
            }

            Remaining = MathF.Max(0f, Remaining - deltaSeconds);
        }
    }
}
