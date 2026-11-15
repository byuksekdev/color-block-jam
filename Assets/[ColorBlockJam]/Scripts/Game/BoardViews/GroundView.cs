using ColorBlockJam.Core.Data;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>The ground of the board: one grid tile for each cell, combined into one mesh, on a plain base.</summary>
    public sealed class GroundView : MonoBehaviour
    {
        [SerializeField] private Mesh _tile;
        [SerializeField] private MeshFilter _grid;
        [SerializeField] private Transform _base;

        private Mesh _gridMesh;

        public void Show(BoardSpace space)
        {
            var tiles = new CombineInstance[space.Width * space.Height];
            for (int y = 0; y < space.Height; y++)
            {
                for (int x = 0; x < space.Width; x++)
                {
                    tiles[y * space.Width + x] = new CombineInstance
                    {
                        mesh = _tile,
                        transform = Matrix4x4.Translate(space.CellCenter(new Cell(x, y)) - space.Center),
                    };
                }
            }

            if (_gridMesh == null)
            {
                _gridMesh = new Mesh { name = "Ground" };
                _grid.sharedMesh = _gridMesh;
            }

            _gridMesh.Clear();
            _gridMesh.CombineMeshes(tiles, true, true);

            Vector3 boardSize = space.Max - space.Min;
            float tileSize = _tile.bounds.size.x;
            _base.localScale = new Vector3(boardSize.x / tileSize, 1f, boardSize.z / tileSize);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Destroy(_gridMesh);
        }
    }
}
