using ColorBlockJam.Core.Players;
using ColorBlockJam.Game.Persistence;
using ColorBlockJam.Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorBlockJam.Game
{
    /// <summary>
    /// Puts the home scene together: it loads the player's saved state, hands it to the screens
    /// and opens the game scene when the player presses the level button.
    /// </summary>
    public sealed class HomeRoot : MonoBehaviour
    {
        [SerializeField] private HomeScreens _screens;

        private void Awake()
        {
            _screens.Bind(new PlayerProfile(new PlayerPrefsStorage()));
            _screens.PlayPressed += LoadGame;
        }

        private void LoadGame()
        {
            _screens.PlayPressed -= LoadGame;
            SceneManager.LoadScene(SceneNames.Game);
        }
    }
}
