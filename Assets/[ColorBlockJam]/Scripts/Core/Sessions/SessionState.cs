namespace ColorBlockJam.Core.Sessions
{
    public enum SessionState
    {
        /// <summary>The level is shown and the timer waits for the first touch.</summary>
        Ready,
        Playing,
        Paused,
        Won,
        Failed,
    }
}
