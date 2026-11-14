using System.IO;
using ColorBlockJam.Game.Visuals;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>Finds the project assets the level editor shows itself with.</summary>
    internal static class EditorAssets
    {
        private const string StyleSheetFile = "LevelEditor.uss";

        public static BlockPalette FindPalette()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(BlockPalette));
            return guids.Length > 0
                ? AssetDatabase.LoadAssetAtPath<BlockPalette>(AssetDatabase.GUIDToAssetPath(guids[0]))
                : null;
        }

        /// <summary>The style sheet lies next to the window's script, so the folder can be moved.</summary>
        public static StyleSheet LoadStyleSheet(ScriptableObject window)
        {
            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(window));
            string folder = Path.GetDirectoryName(scriptPath).Replace('\\', '/');
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(folder + "/" + StyleSheetFile);
        }
    }
}
