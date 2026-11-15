using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Editing;
using ColorBlockJam.Core.Validation;
using ColorBlockJam.Game.Visuals;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>
    /// The level editor. It holds the open level and the designer's choices, and connects the panels,
    /// the canvas and the tools. Every change to the level goes through Edit, so it can be undone;
    /// opening, saving and deleting levels is the work of the level switcher.
    /// </summary>
    internal sealed class LevelEditorWindow : EditorWindow, IEditingContext
    {
        private const string MenuPath = "Tools/Color Block Jam/Level Editor";
        private const string WindowTitle = "Level Editor";
        private const string UnsavedMark = " *";
        private const string DraftMark = "  (draft, not in the game)";
        private const float MinWindowWidth = 760f;
        private const float MinWindowHeight = 560f;
        private const int RightMouseButton = 1;
        private const int BlockToolIndex = 0;
        private const int MaxBoardSize = 10;

        [SerializeField] private OpenLevel _open = new OpenLevel();
        [SerializeField] private int _toolIndex;
        [SerializeField] private BlockColor _color = BlockColor.Red;
        [SerializeField] private int _shapeIndex;
        [SerializeField] private List<Cell> _shape = new List<Cell>();

        private LevelLibrary _library;
        private LevelSwitcher _levels;
        private BlockPalette _palette;
        private IBoardTool[] _tools;
        private IBoardTool _eraseTool;
        private BoardCanvas _canvas;
        private LevelListPanel _levelList;
        private BoardSettingsPanel _settings;
        private ToolPanel _toolPanel;
        private ProblemsPanel _problems;
        private EditorHeader _header;
        private Label _emptyState;

        public LevelData Level => _open.Data;
        public BlockColor Color => _color;
        public IReadOnlyList<Cell> Shape => _shape;
        public BoardCanvas Canvas => _canvas;

        private IBoardTool ActiveTool => _tools[_toolIndex];

        public void Edit(string undoName, Action<LevelData> change)
        {
            Undo.RecordObject(this, undoName);
            change(_open.Data);
            RefreshLevel();
        }

        public override void SaveChanges()
        {
            if (_levels.Save())
            {
                base.SaveChanges();
            }
        }

        [MenuItem(MenuPath)]
        private static void Open()
        {
            var window = GetWindow<LevelEditorWindow>(WindowTitle);
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
            _levels = new LevelSwitcher(_library, _open, this);
            _levels.Changed += RefreshAll;
            _palette = EditorAssets.FindPalette();
            _eraseTool = new EraseTool(this);
            _tools = new IBoardTool[]
            {
                new BlockTool(this), new DoorTool(this), new MoveTool(this), new IceTool(this), new ArrowTool(this), _eraseTool,
            };

            if (_shape.Count == 0)
            {
                _shape.AddRange(BlockShapes.All[_shapeIndex].Cells);
            }

            VisualElement root = rootVisualElement;
            root.AddToClassList("level-editor");
            root.focusable = true;
            root.RegisterCallback<KeyDownEvent>(OnKeyDown);

            StyleSheet styles = EditorAssets.LoadStyleSheet(this);
            if (styles != null)
            {
                root.styleSheets.Add(styles);
            }

            root.Add(BuildSidebar());
            root.Add(BuildMain());

            _levels.OpenFirst();
            RefreshAll();
        }

        private VisualElement BuildSidebar()
        {
            _levelList = new LevelListPanel();
            _levelList.LevelChosen += _levels.Choose;
            _levelList.LevelMoved += _levels.Move;
            _levelList.NewRequested += _levels.New;
            _levelList.DeleteRequested += _levels.Delete;

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
            _header = new EditorHeader();
            _header.SaveRequested += () => _levels.Save();
            _header.PlayRequested += _levels.Play;

            _canvas = new BoardCanvas();
            _canvas.PointerPressed += (hit, button) => (button == RightMouseButton ? _eraseTool : ActiveTool).PointerDown(hit);
            _canvas.PointerMoved += hit => ActiveTool.PointerMove(hit);
            _canvas.PointerReleased += hit => ActiveTool.PointerUp(hit);
            _canvas.PointerLeft += () => ActiveTool.Cancel();

            _emptyState = new Label("No level yet. Press New Level to make the first one.");
            _emptyState.AddToClassList("empty-state");

            var main = new VisualElement();
            main.AddToClassList("main");
            main.Add(_header);
            main.Add(_canvas);
            main.Add(_emptyState);
            return main;
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
                string name = _library.GetFile(i).name;
                names.Add(_library.IsPlayable(i) ? name : name + DraftMark);
            }

            _levelList.Refresh(names, _open.Exists ? _library.IndexOf(_open.File) : LevelEditing.NotFound);
            RefreshChoices();
            RefreshLevel();
        }

        private void RefreshChoices()
        {
            _toolPanel.Refresh(_toolIndex, _color, _shapeIndex, _shape);
        }

        private void RefreshLevel()
        {
            hasUnsavedChanges = _open.HasUnsavedChanges;

            SetShown(_emptyState, !_open.Exists);
            SetShown(_canvas, _open.Exists);
            SetShown(_settings, _open.Exists);
            SetShown(_toolPanel, _open.Exists);
            SetShown(_problems, _open.Exists);

            if (!_open.Exists)
            {
                _header.Refresh(WindowTitle, false, false);
                return;
            }

            List<LevelProblem> problems = LevelValidator.Validate(_open.Data);
            string title = _open.File.name + (hasUnsavedChanges ? UnsavedMark : string.Empty);
            _header.Refresh(title, hasUnsavedChanges, problems.Count == 0);
            _canvas.Show(_open.Data, _palette);
            _settings.Refresh(_open.Data);
            _problems.Refresh(problems);
        }

        private static void SetShown(VisualElement element, bool isShown)
        {
            element.style.display = isShown ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
