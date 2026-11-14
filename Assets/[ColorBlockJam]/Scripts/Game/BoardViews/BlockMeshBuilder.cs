using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using UnityEngine;

namespace ColorBlockJam.Game.BoardViews
{
    /// <summary>
    /// Builds one mesh for a block of any shape. Each cell is four quarters, and each quarter gets the
    /// part that fits its neighbours: a flat centre, a straight edge, an outer corner or an inner corner.
    /// The mesh starts at the lower left corner of cell (0, 0) of the shape.
    /// </summary>
    public static class BlockMeshBuilder
    {
        private const int QuartersPerCell = 4;
        private const float QuarterSize = BoardSpace.CellSize / 2f;
        private const float QuarterTurn = 90f;

        private static readonly Quaternion Upright = Quaternion.Euler(90f, 0f, 0f);

        /// <summary>Quarter i lies towards this corner of the cell, and a part turned i quarter turns covers it.</summary>
        private static readonly Cell[] QuarterDirections = { new Cell(1, 1), new Cell(1, -1), new Cell(-1, -1), new Cell(-1, 1) };

        public static Mesh Build(IReadOnlyList<Cell> shape, BoardKit kit)
        {
            var parts = new List<CombineInstance>(shape.Count * QuartersPerCell);

            for (int i = 0; i < shape.Count; i++)
            {
                Vector3 cellCenter = new Vector3(shape[i].X + 0.5f, 0f, shape[i].Y + 0.5f) * BoardSpace.CellSize;
                for (int quarter = 0; quarter < QuartersPerCell; quarter++)
                {
                    AddQuarter(parts, shape, shape[i], cellCenter, quarter, kit);
                }
            }

            var mesh = new Mesh { name = "Block" };
            mesh.CombineMeshes(parts.ToArray(), true, true);
            return mesh;
        }

        private static void AddQuarter(
            List<CombineInstance> parts,
            IReadOnlyList<Cell> shape,
            Cell cell,
            Vector3 cellCenter,
            int quarter,
            BoardKit kit)
        {
            Cell direction = QuarterDirections[quarter];
            bool hasSide = Contains(shape, cell + new Cell(direction.X, 0));
            bool hasFront = Contains(shape, cell + new Cell(0, direction.Y));
            bool hasDiagonal = Contains(shape, cell + direction);

            if (hasSide && hasFront && hasDiagonal)
            {
                parts.Add(Part(kit.BlockCenter, QuarterOrigin(cellCenter, quarter, 0), 0));
            }
            else if (hasSide && hasFront)
            {
                Vector3 innerCorner = cellCenter + new Vector3(direction.X, 0f, direction.Y) * QuarterSize;
                parts.Add(Part(kit.BlockInnerCorner, innerCorner, quarter));
            }
            else if (hasSide && !hasDiagonal)
            {
                int turns = direction.Y > 0 ? 0 : 2;
                parts.Add(Part(kit.BlockEdge, QuarterOrigin(cellCenter, quarter, turns), turns));
            }
            else if (hasFront && !hasDiagonal)
            {
                int turns = direction.X > 0 ? 1 : 3;
                parts.Add(Part(kit.BlockEdge, QuarterOrigin(cellCenter, quarter, turns), turns));
            }
            else if (!hasSide && !hasFront)
            {
                parts.Add(Part(kit.BlockOuterCorner, cellCenter, quarter));
            }
        }

        /// <summary>
        /// Where to put a part that is turned by the given quarter turns so that it covers this quarter.
        /// A part turned i times covers quarter i of the cell it is placed at.
        /// </summary>
        private static Vector3 QuarterOrigin(Vector3 cellCenter, int quarter, int turns)
        {
            Vector3 wanted = MinCorner(QuarterDirections[quarter]);
            Vector3 covered = MinCorner(QuarterDirections[turns]);
            return cellCenter + (wanted - covered) * QuarterSize;
        }

        private static Vector3 MinCorner(Cell direction)
        {
            return new Vector3(direction.X > 0 ? 0f : -1f, 0f, direction.Y > 0 ? 0f : -1f);
        }

        private static CombineInstance Part(Mesh mesh, Vector3 position, int turns)
        {
            Quaternion rotation = Quaternion.Euler(0f, turns * QuarterTurn, 0f) * Upright;
            return new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(position, rotation, Vector3.one) };
        }

        private static bool Contains(IReadOnlyList<Cell> shape, Cell cell)
        {
            for (int i = 0; i < shape.Count; i++)
            {
                if (shape[i] == cell)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
