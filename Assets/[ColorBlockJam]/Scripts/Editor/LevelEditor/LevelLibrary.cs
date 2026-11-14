using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Game.Persistence;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>The level files of the project and their play order, as the level editor sees them.</summary>
    internal sealed class LevelLibrary
    {
        private const string LevelsFolder = "Assets/[ColorBlockJam]/Levels";
        private const string CatalogPath = "Assets/[ColorBlockJam]/Data/LevelCatalog.asset";
        private const string LevelsProperty = "_levels";
        private const string FileNameFormat = "Level_{0}.json";
        private const string FileNumberFormat = "000";

        private readonly SerializedObject _catalog;
        private readonly List<TextAsset> _files = new List<TextAsset>();

        public LevelLibrary()
        {
            _catalog = new SerializedObject(LoadOrCreateCatalog());
            ReadFiles();
        }

        public int Count => _files.Count;

        public TextAsset GetFile(int index)
        {
            return _files[index];
        }

        public int IndexOf(TextAsset file)
        {
            return _files.IndexOf(file);
        }

        public LevelData Load(TextAsset file)
        {
            return LevelSerializer.FromJson(file.text);
        }

        public TextAsset Create(LevelData level)
        {
            string path = NextFreePath();
            Write(path, level);

            var file = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            _files.Add(file);
            WriteFiles();
            return file;
        }

        public void Save(TextAsset file, LevelData level)
        {
            Write(AssetDatabase.GetAssetPath(file), level);
        }

        public void Delete(TextAsset file)
        {
            _files.Remove(file);
            WriteFiles();
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(file));
        }

        public void Move(int from, int to)
        {
            TextAsset file = _files[from];
            _files.RemoveAt(from);
            _files.Insert(to, file);
            WriteFiles();
        }

        private static LevelCatalog LoadOrCreateCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog != null)
            {
                return catalog;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(CatalogPath));
            catalog = ScriptableObject.CreateInstance<LevelCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static string NextFreePath()
        {
            Directory.CreateDirectory(LevelsFolder);

            int number = 1;
            while (true)
            {
                string fileName = string.Format(
                    CultureInfo.InvariantCulture,
                    FileNameFormat,
                    number.ToString(FileNumberFormat, CultureInfo.InvariantCulture));
                string path = LevelsFolder + "/" + fileName;
                if (!File.Exists(path))
                {
                    return path;
                }

                number++;
            }
        }

        private static void Write(string path, LevelData level)
        {
            File.WriteAllText(path, LevelSerializer.ToJson(level));
            AssetDatabase.ImportAsset(path);
        }

        private void ReadFiles()
        {
            _catalog.Update();
            SerializedProperty levels = _catalog.FindProperty(LevelsProperty);

            _files.Clear();
            for (int i = 0; i < levels.arraySize; i++)
            {
                var file = levels.GetArrayElementAtIndex(i).objectReferenceValue as TextAsset;
                if (file != null)
                {
                    _files.Add(file);
                }
            }

            if (_files.Count != levels.arraySize)
            {
                WriteFiles();
            }
        }

        private void WriteFiles()
        {
            SerializedProperty levels = _catalog.FindProperty(LevelsProperty);
            levels.arraySize = _files.Count;
            for (int i = 0; i < _files.Count; i++)
            {
                levels.GetArrayElementAtIndex(i).objectReferenceValue = _files[i];
            }

            _catalog.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(_catalog.targetObject);
        }
    }
}
