using ColorBlockJam.Core.Boards;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Flow;
using ColorBlockJam.Core.Sessions;
using ColorBlockJam.Game.BoardViews;
using UnityEngine;

namespace ColorBlockJam.Game
{
    /// <summary>
    /// Shows the level that is being played and keeps it running: it gives each new session to the
    /// board view and the input, and advances the session every frame.
    /// </summary>
    public sealed class LevelRunner : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private BoardInput _input;
        [SerializeField] private BoardCamera _camera;

        private GameFlow _flow;
        private LevelSession _session;

        public void Bind(GameFlow flow)
        {
            _flow = flow;
            _flow.LevelStarted += OnLevelStarted;
            _flow.LevelLeft += OnLevelLeft;
            enabled = false;
        }

        private void Update()
        {
            _input.Process();
            _session.Tick(Time.deltaTime);
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused && _session != null)
            {
                _session.Pause();
            }
        }

        private void OnDestroy()
        {
            ReleaseSession();
            if (_flow != null)
            {
                _flow.LevelStarted -= OnLevelStarted;
                _flow.LevelLeft -= OnLevelLeft;
            }
        }

        private void OnLevelStarted(LevelSession session)
        {
            ReleaseSession();
            _session = session;
            _session.BlockExited += OnBlockExited;

            _boardView.Show(session.Board);
            _camera.Frame(_boardView.Space);
            _input.Bind(session, _boardView);
            enabled = true;
        }

        private void OnLevelLeft()
        {
            ReleaseSession();
            _boardView.Clear();
            enabled = false;
        }

        private void ReleaseSession()
        {
            if (_session == null)
            {
                return;
            }

            _session.BlockExited -= OnBlockExited;
            _session = null;
        }

        private void OnBlockExited(Block block, Cell origin, Door door)
        {
            _boardView.PlayExit(block, origin, door);
        }
    }
}
