using System.Globalization;

namespace ColorBlockJam.Game.UI
{
    /// <summary>Writes a number of seconds the way the timer shows it: minutes and seconds, two digits each.</summary>
    public static class ClockText
    {
        private const int SecondsPerMinute = 60;
        private const string TwoDigits = "00";

        public static string Format(int seconds)
        {
            int minutes = seconds / SecondsPerMinute;
            int rest = seconds % SecondsPerMinute;
            return minutes.ToString(TwoDigits, CultureInfo.InvariantCulture)
                + ":"
                + rest.ToString(TwoDigits, CultureInfo.InvariantCulture);
        }
    }
}
