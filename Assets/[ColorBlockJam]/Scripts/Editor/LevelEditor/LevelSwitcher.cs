using System;
using ColorBlockJam.Core.Data;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>
    /// Opens, creates, deletes, reorders, saves and plays levels for the editor window. It asks the
    /// designer before changes would be lost or a level is deleted.
    /// </summary>
    internal sealed class LevelSwitcher
    {
        private const int NewBoardSize = 5;
        private const int NewTimeLimitSeconds = 300;
        private const int SaveChoice = 0;
        private const int CancelChoice = 1;

        private readonly LevelLibrary _library;
        private readonly OpenLevel _open;
        private readonly UnityEngine.Object _undoOwner;

        /// <param name="undoOwner">The object that records the undo steps of the open level. They are dropped when another level opens.</param>
        public LevelSwitcher(LevelLibrary library, OpenLevel open, UnityEngine.Object undoOwner)
        {
            _library = library;
            _open = open;
            _undoOwner = undoOwner;
        }

        /// <summary>The open level, its file or the list of levels changed.</summary>
        public event Action Changed;

        /// <summary>Opens the first level when none is open yet.</summary>
        public void OpenFirst()
        {
            if (!_open.Exists && _library.Count > 0)
            {
                Load(_library.GetFile(0));
            }
        }

        public void Choose(int index)
        {
            if (index < 0 || index >= _library.Count || _library.GetFile(index) == _open.File)
            {
                return;
            }

            if (TryLeave())
            {
                Load(_library.GetFile(index));
            }

            Changed?.Invoke();
        }

        public void New()
        {
            if (!TryLeave())
            {
                return;
            }

            var level = new LevelData { Width = NewBoardSize, Height = NewBoardSize, TimeLimitSeconds = NewTimeLimitSeconds };
            Load(_library.Create(level));
            Changed?.Invoke();
        }

        public void Delete()
        {
            string question = "Delete " + _open.File.name + "? This cannot be undone.";
            if (!EditorUtility.DisplayDialog("Delete level", question, "Delete", "Cancel"))
            {
                return;
            }

            int index = _library.IndexOf(_open.File);
            _library.Delete(_open.File);
            _open.Close();

            if (_library.Count > 0)
            {
                Load(_library.GetFile(Mathf.Min(index, _library.Count - 1)));
            }

            Changed?.Invoke();
        }

        public void Move(int from, int to)
        {
            _library.Move(from, to);
            Changed?.Invoke();
        }

        public void Save()
        {
            _library.Save(_open.File, _open.Data);
            Changed?.Invoke();
        }

        /// <summary>Plays the open level in the game. Unsaved changes are saved first, because the game reads the file.</summary>
        public void Play()
        {
            if (_open.HasUnsavedChanges)
            {
                Save();
            }

            LevelPlayback.Play(_library.IndexOf(_open.File));
        }

        private void Load(TextAsset file)
        {
            LevelData level;
            try
            {
                level = _library.Load(file);
            }
            catch (FormatException exception)
            {
                EditorUtility.DisplayDialog("Level cannot be opened", file.name + ": " + exception.Message, "OK");
                return;
            }

            _open.Set(file, level);
            Undo.ClearUndo(_undoOwner);
        }

        private bool TryLeave()
        {
            if (!_open.HasUnsavedChanges)
            {
                return true;
            }

            string question = "Save the changes to " + _open.File.name + "?";
            int choice = EditorUtility.DisplayDialogComplex("Unsaved changes", question, "Save", "Cancel", "Discard");
            if (choice == SaveChoice)
            {
                Save();
            }

            return choice != CancelChoice;
        }
    }
}
