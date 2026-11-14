using ColorBlockJam.Core.Flow;
using ColorBlockJam.Core.Players;
using ColorBlockJam.Game.Persistence;
using ColorBlockJam.Game.UI;
using UnityEngine;

namespace ColorBlockJam.Game
{
    /// <summary>
    /// The one place where the game is put together: it creates the logic objects and hands them
    /// to the views of the scene.
    /// </summary>
    public sealed class GameRoot : MonoBehaviour
    {
        private const int TargetFrameRate = 60;

        [SerializeField] private LevelCatalog _catalog;
        [SerializeField] private LevelRunner _levelRunner;
        [SerializeField] private GameScreens _screens;

        private GameFlow _flow;

        /// <summary>Starts a level directly, without the home screen. The level editor plays a level this way.</summary>
        public void PlayLevel(int levelIndex)
        {
            _flow.StartLevel(levelIndex);
        }

        private void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;

            var player = new PlayerProfile(new PlayerPrefsStorage());
            _flow = new GameFlow(player, _catalog);

            _levelRunner.Bind(_flow);
            _screens.Bind(_flow, player);
        }
    }
}
