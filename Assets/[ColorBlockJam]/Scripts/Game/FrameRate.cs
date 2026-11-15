using UnityEngine;

namespace ColorBlockJam.Game
{
    /// <summary>Sets the frame rate once when the game starts, whichever scene is first.</summary>
    internal static class FrameRate
    {
        private const int Target = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            Application.targetFrameRate = Target;
        }
    }
}
