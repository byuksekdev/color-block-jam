using System;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Validation;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>
    /// Opens, creates, deletes, reorders, saves and plays levels for the editor window. It asks the
    /// designer before changes would be lost, a level is deleted, or a level that cannot be solved is saved.
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

        /// <summary>Saves the open level. Returns false when the designer kept a level that cannot be solved from being saved.</summary>
        public bool Save()
        {
            if (!ConfirmUnsolvable())
            {
                return false;
            }

            _library.Save(_open.File, _open.Data);
            Changed?.Invoke();
            return true;
        }

        /// <summary>Plays the open level in the game. Unsaved changes are saved first, because the game reads the file.</summary>
        public void Play()
        {
            if (_open.HasUnsavedChanges && !Save())
            {
                return;
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
                return Save();
            }

            return choice != CancelChoice;
        }

        /// <summary>
        /// Warns when the level cannot be solved and lets the designer save it anyway. A level that is
        /// not well formed is not tried: the problems list already says what is wrong with it.
        /// </summary>
        private bool ConfirmUnsolvable()
        {
            if (LevelValidator.Validate(_open.Data).Count > 0)
            {
                return true;
            }

            SolverResult result = LevelSolver.Solve(_open.Data);
            if (result.Solvability != Solvability.Unsolvable)
            {
                return true;
            }

            string warning = _open.File.name + " cannot be solved.\n\n" + result.Reason;
            return EditorUtility.DisplayDialog("Level cannot be solved", warning, "Save anyway", "Do not save");
        }
    }
}
