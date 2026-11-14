using System;
using System.Collections.Generic;
using System.IO;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Editing;
using ColorBlockJam.Core.Validation;
using ColorBlockJam.Game.Persistence;
using ColorBlockJam.Game.Visuals;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>
    /// The level editor. It holds the open level and the designer's choices, and connects the panels,
    /// the canvas and the tools. Every change to the level goes through Edit, so it can be undone.
    /// </summary>
    internal sealed class LevelEditorWindow : EditorWindow, IEditingContext
    {
        private const string MenuPath = "Tools/Color Block Jam/Level Editor";
        private const string StyleSheetFile = "LevelEditor.uss";
        private const float MinWindowWidth = 760f;
        private const float MinWindowHeight = 560f;
        private const int RightMouseButton = 1;
        private const int BlockToolIndex = 0;
        private const int MaxBoardSize = 10;
        private const int NewBoardSize = 5;
        private const int NewTimeLimitSeconds = 300;
        private const int SaveChoice = 0;
        private const int CancelChoice = 1;

        [SerializeField] private LevelData _level = new LevelData();
        [SerializeField] private TextAsset _file;
        [SerializeField] private int _toolIndex;
        [SerializeField] private BlockColor _color = BlockColor.Red;
        [SerializeField] private int _shapeIndex;
        [SerializeField] private List<Cell> _shape = new List<Cell>();

        private LevelLibrary _library;
        private BlockPalette _palette;
        private IBoardTool[] _tools;
        private IBoardTool _eraseTool;
        private BoardCanvas _canvas;
        private LevelListPanel _levelList;
        private BoardSettingsPanel _settings;
        private ToolPanel _toolPanel;
        private ProblemsPanel _problems;
        private Label _title;
        private Button _saveButton;
        private Label _emptyState;

        public LevelData Level => _level;
        public BlockColor Color => _color;
        public IReadOnlyList<Cell> Shape => _shape;
        public BoardCanvas Canvas => _canvas;

        private bool HasLevel => _file != null;
        private IBoardTool ActiveTool => _tools[_toolIndex];

        public void Edit(string undoName, Action<LevelData> change)
        {
            Undo.RecordObject(this, undoName);
            change(_level);
            RefreshLevel();
        }

        public override void SaveChanges()
        {
            SaveLevel();
            base.SaveChanges();
        }

        [MenuItem(MenuPath)]
        private static void Open()
        {
            var window = GetWindow<LevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
        }

        private void OnEnable()
        {
            saveChangesMessage = "The open level has changes that are not saved.";
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void CreateGUI()
        {
            _library = new LevelLibrary();
            _palette = FindPalette();
            _eraseTool = new EraseTool(this);
            _tools = new IBoardTool[] { new BlockTool(this), new DoorTool(this), new MoveTool(this), _eraseTool };

            if (_shape.Count == 0)
            {
                _shape.AddRange(BlockShapes.All[_shapeIndex].Cells);
            }

            VisualElement root = rootVisualElement;
            root.AddToClassList("level-editor");
            root.focusable = true;
            root.RegisterCallback<KeyDownEvent>(OnKeyDown);

            StyleSheet styles = LoadStyleSheet();
            if (styles != null)
            {
                root.styleSheets.Add(styles);
            }

            root.Add(BuildSidebar());
            root.Add(BuildMain());

            if (!HasLevel && _library.Count > 0)
            {
                LoadLevel(_library.GetFile(0));
            }

            RefreshAll();
        }

        private VisualElement BuildSidebar()
        {
            _levelList = new LevelListPanel();
            _levelList.LevelChosen += ChooseLevel;
            _levelList.LevelMoved += MoveLevel;
            _levelList.NewRequested += NewLevel;
            _levelList.DeleteRequested += DeleteLevel;

            _settings = new BoardSettingsPanel(LevelValidator.MinBoardSize, MaxBoardSize);
            _settings.SizeChosen += (width, height) => Edit("Resize Board", level => LevelEditing.Resize(level, width, height));
            _settings.TimeChosen += seconds => Edit("Change Timer", level => level.TimeLimitSeconds = seconds);

            _toolPanel = new ToolPanel(_tools, _palette);
            _toolPanel.ToolChosen += ChooseTool;
            _toolPanel.ColorChosen += ChooseColor;
            _toolPanel.ShapeChosen += ChooseShape;
            _toolPanel.TurnRequested += TurnShape;
            _toolPanel.MirrorRequested += MirrorShape;

            _problems = new ProblemsPanel();

            var sidebar = new ScrollView();
            sidebar.AddToClassList("sidebar");
            sidebar.Add(_levelList);
            sidebar.Add(_settings);
            sidebar.Add(_toolPanel);
            sidebar.Add(_problems);
            return sidebar;
        }

        private VisualElement BuildMain()
        {
            _title = new Label();
            _title.AddToClassList("header-title");
            _saveButton = new Button(SaveLevel) { text = "Save" };

            var header = new VisualElement();
            header.AddToClassList("header");
            header.Add(_title);
            header.Add(_saveButton);

            _canvas = new BoardCanvas();
            _canvas.PointerPressed += (hit, button) => (button == RightMouseButton ? _eraseTool : ActiveTool).PointerDown(hit);
            _canvas.PointerMoved += hit => ActiveTool.PointerMove(hit);
            _canvas.PointerReleased += hit => ActiveTool.PointerUp(hit);
            _canvas.PointerLeft += () => ActiveTool.Cancel();

            _emptyState = new Label("No level yet. Press New Level to make the first one.");
            _emptyState.AddToClassList("empty-state");

            var main = new VisualElement();
            main.AddToClassList("main");
            main.Add(header);
            main.Add(_canvas);
            main.Add(_emptyState);
            return main;
        }

        private void ChooseLevel(int index)
        {
            if (index < 0 || index >= _library.Count || _library.GetFile(index) == _file)
            {
                return;
            }

            if (TryLeaveLevel())
            {
                LoadLevel(_library.GetFile(index));
            }

            RefreshAll();
        }

        private void NewLevel()
        {
            if (!TryLeaveLevel())
            {
                return;
            }

            var level = new LevelData { Width = NewBoardSize, Height = NewBoardSize, TimeLimitSeconds = NewTimeLimitSeconds };
            LoadLevel(_library.Create(level));
            RefreshAll();
        }

        private void DeleteLevel()
        {
            string question = "Delete " + _file.name + "? This cannot be undone.";
            if (!EditorUtility.DisplayDialog("Delete level", question, "Delete", "Cancel"))
            {
                return;
            }

            int index = _library.IndexOf(_file);
            _library.Delete(_file);
            _file = null;

            if (_library.Count > 0)
            {
                LoadLevel(_library.GetFile(Mathf.Min(index, _library.Count - 1)));
            }

            RefreshAll();
        }

        private void MoveLevel(int from, int to)
        {
            _library.Move(from, to);
            RefreshAll();
        }

        private void SaveLevel()
        {
            _library.Save(_file, _level);
            RefreshLevel();
        }

        private void LoadLevel(TextAsset file)
        {
            try
            {
                _level = _library.Load(file);
            }
            catch (FormatException exception)
            {
                EditorUtility.DisplayDialog("Level cannot be opened", file.name + ": " + exception.Message, "OK");
                return;
            }

            _file = file;
            Undo.ClearUndo(this);
        }

        private bool TryLeaveLevel()
        {
            if (!HasLevel || !hasUnsavedChanges)
            {
                return true;
            }

            string question = "Save the changes to " + _file.name + "?";
            int choice = EditorUtility.DisplayDialogComplex("Unsaved changes", question, "Save", "Cancel", "Discard");
            if (choice == SaveChoice)
            {
                SaveLevel();
            }

            return choice != CancelChoice;
        }

        private void ChooseTool(int index)
        {
            ActiveTool.Cancel();
            _toolIndex = index;
            RefreshChoices();
        }

        private void ChooseColor(BlockColor color)
        {
            _color = color;
            RefreshChoices();
        }

        private void ChooseShape(int index)
        {
            _shapeIndex = index;
            SetShape(BlockShapes.All[index].Cells);
            ChooseTool(BlockToolIndex);
        }

        private void TurnShape()
        {
            SetShape(ShapeTransform.RotateClockwise(_shape));
        }

        private void MirrorShape()
        {
            SetShape(ShapeTransform.FlipHorizontally(_shape));
        }

        private void SetShape(IReadOnlyList<Cell> cells)
        {
            _shape.Clear();
            _shape.AddRange(cells);
            ActiveTool.Cancel();
            RefreshChoices();
        }

        private void OnKeyDown(KeyDownEvent keyEvent)
        {
            if (keyEvent.keyCode == KeyCode.R)
            {
                TurnShape();
            }
            else if (keyEvent.keyCode == KeyCode.F)
            {
                MirrorShape();
            }
        }

        private void OnUndoRedo()
        {
            if (_canvas == null)
            {
                return;
            }

            ActiveTool.Cancel();
            RefreshAll();
        }

        private void RefreshAll()
        {
            var names = new List<string>(_library.Count);
            for (int i = 0; i < _library.Count; i++)
            {
                names.Add(_library.GetFile(i).name);
            }

            _levelList.Refresh(names, HasLevel ? _library.IndexOf(_file) : LevelEditing.NotFound);
            RefreshChoices();
            RefreshLevel();
        }

        private void RefreshChoices()
        {
            _toolPanel.Refresh(_toolIndex, _color, _shapeIndex, _shape);
        }

        private void RefreshLevel()
        {
            hasUnsavedChanges = HasLevel && LevelSerializer.ToJson(_level) != _file.text;
            _saveButton.SetEnabled(hasUnsavedChanges);
            _title.text = HasLevel ? _file.name + (hasUnsavedChanges ? " *" : string.Empty) : "Level Editor";

            SetShown(_emptyState, !HasLevel);
            SetShown(_canvas, HasLevel);
            SetShown(_settings, HasLevel);
            SetShown(_toolPanel, HasLevel);
            SetShown(_problems, HasLevel);

            if (HasLevel)
            {
                _canvas.Show(_level, _palette);
                _settings.Refresh(_level);
                _problems.Refresh(LevelValidator.Validate(_level));
            }
        }

        private static void SetShown(VisualElement element, bool isShown)
        {
            element.style.display = isShown ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static BlockPalette FindPalette()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(BlockPalette));
            return guids.Length > 0
                ? AssetDatabase.LoadAssetAtPath<BlockPalette>(AssetDatabase.GUIDToAssetPath(guids[0]))
                : null;
        }

        private StyleSheet LoadStyleSheet()
        {
            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            string folder = Path.GetDirectoryName(scriptPath).Replace('\\', '/');
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(folder + "/" + StyleSheetFile);
        }
    }
}
