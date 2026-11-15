using ColorBlockJam.Core.Flow;
using ColorBlockJam.Core.Players;
using ColorBlockJam.Game.Persistence;
using ColorBlockJam.Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorBlockJam.Game
{
    /// <summary>
    /// Puts the game scene together: it creates the logic objects, hands them to the views of the
    /// scene and starts the level the player has reached. Leaving a level opens the home scene.
    /// </summary>
    public sealed class GameRoot : MonoBehaviour
    {
        [SerializeField] private LevelCatalog _catalog;
        [SerializeField] private LevelRunner _levelRunner;
        [SerializeField] private GameScreens _screens;

        private GameFlow _flow;

        /// <summary>Starts this level instead of the one the player has reached. The level editor plays a level this way.</summary>
        public void PlayLevel(int levelIndex)
        {
            _flow.StartLevel(levelIndex);
        }

        private void Awake()
        {
            var player = new PlayerProfile(new PlayerPrefsStorage());
            _flow = new GameFlow(player, _catalog);
            _flow.LevelLeft += LoadHome;

            _levelRunner.Bind(_flow);
            _screens.Bind(_flow, player);
        }

        private void Start()
        {
            if (!_flow.IsInLevel)
            {
                _flow.StartLevel();
            }
        }

        private void OnDestroy()
        {
            _flow.LevelLeft -= LoadHome;
        }

        private static void LoadHome()
        {
            SceneManager.LoadScene(SceneNames.Home);
        }
    }
}
