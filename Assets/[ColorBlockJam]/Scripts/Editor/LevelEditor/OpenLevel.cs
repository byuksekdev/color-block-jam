using System;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Game.Persistence;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>
    /// The level that is open in the editor, and the file it came from. The window keeps it as part
    /// of its own state, so it survives a script reload and takes part in undo.
    /// </summary>
    [Serializable]
    internal sealed class OpenLevel
    {
        [SerializeField] private LevelData _data = new LevelData();
        [SerializeField] private TextAsset _file;

        public LevelData Data => _data;
        public TextAsset File => _file;
        public bool Exists => _file != null;

        /// <summary>
        /// True when saving would change the level. The file is read and written again for the
        /// comparison, so a file from before a field was added does not count as changed.
        /// </summary>
        public bool HasUnsavedChanges =>
            Exists && LevelSerializer.ToJson(_data) != LevelSerializer.ToJson(LevelSerializer.FromJson(_file.text));

        public void Set(TextAsset file, LevelData data)
        {
            _file = file;
            _data = data;
        }

        public void Close()
        {
            _file = null;
        }
    }
}
