using System.Collections;
using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Sessions;
using ColorBlockJam.Game.BoardViews;
using ColorBlockJam.Game.Persistence;
using UnityEngine;

namespace ColorBlockJam.Game
{
    /// <summary>
    /// Runs one level after another: it builds a session from the level file, hands it to the board
    /// view and the input, and advances it each frame. A new attempt is always a new session.
    /// </summary>
    public sealed class LevelRunner : MonoBehaviour
    {
        private const float NextLevelDelaySeconds = 0.8f;

        [SerializeField] private LevelCatalog _catalog;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private BoardInput _input;
        [SerializeField] private BoardCamera _camera;

        private readonly WaitForSeconds _nextLevelDelay = new WaitForSeconds(NextLevelDelaySeconds);
        private LevelSession _session;
        private int _levelIndex;

        public void StartLevel(int levelIndex)
        {
            StopAllCoroutines();
            ReleaseSession();

            _levelIndex = levelIndex % _catalog.Count;
            _session = new LevelSession(_catalog.Load(_levelIndex));
            _session.BlockExited += OnBlockExited;
            _session.StateChanged += OnStateChanged;

            _boardView.Show(_session.Board);
            _camera.Frame(_boardView.Space);
            _input.Bind(_session, _boardView);
        }

        private void Start()
        {
            if (_session == null)
            {
                StartLevel(_levelIndex);
            }
        }

        private void Update()
        {
            _input.Process();
            _session.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            ReleaseSession();
        }

        private void ReleaseSession()
        {
            if (_session == null)
            {
                return;
            }

            _session.BlockExited -= OnBlockExited;
            _session.StateChanged -= OnStateChanged;
            _session = null;
        }

        private void OnBlockExited(Block block, Door door)
        {
            _boardView.PlayExit(block, door);
        }

        private void OnStateChanged(SessionState state)
        {
            if (state == SessionState.Won)
            {
                StartCoroutine(StartLevelAfterDelay(_levelIndex + 1));
            }
            else if (state == SessionState.Failed)
            {
                StartCoroutine(StartLevelAfterDelay(_levelIndex));
            }
        }

        private IEnumerator StartLevelAfterDelay(int levelIndex)
        {
            yield return _nextLevelDelay;
            StartLevel(levelIndex);
        }
    }
}
