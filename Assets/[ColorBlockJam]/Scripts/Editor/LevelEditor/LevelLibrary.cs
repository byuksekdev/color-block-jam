using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Validation;
using ColorBlockJam.Game.Persistence;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>
    /// The level files of the project, as the level editor sees them. The files that can be played
    /// come first, in play order, and only they are in the level catalog the game reads. A file that
    /// is not well formed yet is a draft: it follows them and enters the catalog when it is saved well formed.
    /// </summary>
    internal sealed class LevelLibrary
    {
        private const string LevelsFolder = "Assets/[ColorBlockJam]/Levels";
        private const string CatalogPath = "Assets/[ColorBlockJam]/Data/LevelCatalog.asset";
        private const string LevelsProperty = "_levels";
        private const string FileNameFormat = "Level_{0}.json";
        private const string FileNumberFormat = "000";

        private readonly SerializedObject _catalog;
        private readonly List<TextAsset> _files = new List<TextAsset>();
        private int _playableCount;

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

        /// <summary>The place of a file in the list. For a playable file it is also its place in the game.</summary>
        public int IndexOf(TextAsset file)
        {
            return _files.IndexOf(file);
        }

        /// <summary>False for a draft, which the game does not have.</summary>
        public bool IsPlayable(int index)
        {
            return index < _playableCount;
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
            WriteCatalog();
            return file;
        }

        public void Save(TextAsset file, LevelData level)
        {
            Write(AssetDatabase.GetAssetPath(file), level);
            WriteCatalog();
        }

        public void Delete(TextAsset file)
        {
            _files.Remove(file);
            WriteCatalog();
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(file));
        }

        public void Move(int from, int to)
        {
            TextAsset file = _files[from];
            _files.RemoveAt(from);
            _files.Insert(to, file);
            WriteCatalog();
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

        private static bool IsWellFormed(TextAsset file)
        {
            try
            {
                return LevelValidator.Validate(LevelSerializer.FromJson(file.text)).Count == 0;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>Reads the catalog in its order, then every other level file of the levels folder by name.</summary>
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

            if (AssetDatabase.IsValidFolder(LevelsFolder))
            {
                var others = new List<TextAsset>();
                foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(TextAsset), new[] { LevelsFolder }))
                {
                    var file = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guid));
                    if (file != null && !_files.Contains(file))
                    {
                        others.Add(file);
                    }
                }

                others.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
                _files.AddRange(others);
            }

            WriteCatalog();
        }

        /// <summary>
        /// Puts the playable files before the drafts, keeping the order within each group, and writes
        /// the playable ones to the catalog. Called after everything that can change either.
        /// </summary>
        private void WriteCatalog()
        {
            var drafts = new List<TextAsset>();
            for (int i = _files.Count - 1; i >= 0; i--)
            {
                if (!IsWellFormed(_files[i]))
                {
                    drafts.Insert(0, _files[i]);
                    _files.RemoveAt(i);
                }
            }

            _playableCount = _files.Count;

            SerializedProperty levels = _catalog.FindProperty(LevelsProperty);
            levels.arraySize = _playableCount;
            for (int i = 0; i < _playableCount; i++)
            {
                levels.GetArrayElementAtIndex(i).objectReferenceValue = _files[i];
            }

            _catalog.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(_catalog.targetObject);
            _files.AddRange(drafts);
        }
    }
}
