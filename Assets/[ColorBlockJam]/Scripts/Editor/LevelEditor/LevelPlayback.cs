using System.IO;
using ColorBlockJam.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>
    /// Plays a level of the catalog in the game scene, whichever scene is open, without passing through
    /// the home scene. The request is kept in the editor session, because entering Play Mode reloads the editor's code.
    /// In Play Mode on another scene, such as the home scene, the game scene is loaded first.
    /// </summary>
    [InitializeOnLoad]
    internal static class LevelPlayback
    {
        private const string RequestKey = "ColorBlockJam.LevelEditor.LevelToPlay";
        private const int NoRequest = -1;

        static LevelPlayback()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void Play(int levelIndex)
        {
            if (EditorApplication.isPlaying)
            {
                StartLevel(levelIndex);
                return;
            }

            SceneAsset gameScene = FindGameScene();
            if (gameScene == null)
            {
                EditorUtility.DisplayDialog("Level cannot be played", "Add the game scene to the Build Settings first.", "OK");
                return;
            }

            SessionState.SetInt(RequestKey, levelIndex);
            EditorSceneManager.playModeStartScene = gameScene;
            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            int levelIndex = SessionState.GetInt(RequestKey, NoRequest);
            if (levelIndex == NoRequest)
            {
                return;
            }

            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                StartLevel(levelIndex);
            }
            else if (change == PlayModeStateChange.EnteredEditMode)
            {
                SceneManager.sceneLoaded -= OnGameSceneLoaded;
                SessionState.EraseInt(RequestKey);
                EditorSceneManager.playModeStartScene = null;
            }
        }

        private static void StartLevel(int levelIndex)
        {
            if (TryPreview(levelIndex))
            {
                return;
            }

            SessionState.SetInt(RequestKey, levelIndex);
            SceneManager.sceneLoaded += OnGameSceneLoaded;
            SceneManager.LoadScene(SceneNames.Game);
        }

        /// <summary>The game scene has woken up but not started yet, so the preview takes the place of the level it would start.</summary>
        private static void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnGameSceneLoaded;
            int levelIndex = SessionState.GetInt(RequestKey, NoRequest);
            if (levelIndex != NoRequest && !TryPreview(levelIndex))
            {
                Debug.LogWarning("The level cannot be played: the game scene has no " + nameof(GameRoot) + ".");
            }
        }

        private static bool TryPreview(int levelIndex)
        {
            GameRoot game = Object.FindObjectOfType<GameRoot>();
            if (game == null)
            {
                return false;
            }

            game.PreviewLevel(levelIndex);
            return true;
        }

        private static SceneAsset FindGameScene()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled && Path.GetFileNameWithoutExtension(scenes[i].path) == SceneNames.Game)
                {
                    return AssetDatabase.LoadAssetAtPath<SceneAsset>(scenes[i].path);
                }
            }

            return null;
        }
    }
}
