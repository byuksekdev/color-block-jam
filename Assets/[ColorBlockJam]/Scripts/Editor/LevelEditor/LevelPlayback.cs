using ColorBlockJam.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>
    /// Plays a level of the catalog in the game scene, whichever scene is open. The request is kept
    /// in the editor session, because entering Play Mode reloads the editor's code.
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
                SessionState.EraseInt(RequestKey);
                EditorSceneManager.playModeStartScene = null;
            }
        }

        private static void StartLevel(int levelIndex)
        {
            GameRoot game = Object.FindObjectOfType<GameRoot>();
            if (game == null)
            {
                Debug.LogWarning("The level cannot be played: the open scene has no " + nameof(GameRoot) + ".");
                return;
            }

            game.PlayLevel(levelIndex);
        }

        private static SceneAsset FindGameScene()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled)
                {
                    return AssetDatabase.LoadAssetAtPath<SceneAsset>(scenes[i].path);
                }
            }

            return null;
        }
    }
}
