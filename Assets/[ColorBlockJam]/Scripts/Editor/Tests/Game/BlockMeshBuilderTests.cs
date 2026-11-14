using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Editing;
using ColorBlockJam.Game.BoardViews;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.Tests.Game
{
    public class BlockMeshBuilderTests
    {
        private const float Tolerance = 0.01f;

        /// <summary>
        /// The supplied inner corner is rounded and reaches 0.16 units into the empty quarter
        /// (knowledge/asset-notes.md). A misplaced part is off by a whole quarter, which is 1 unit.
        /// </summary>
        private const float RoundedCornerReach = 0.2f;

        private static BoardKit LoadProjectKit()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(BoardKit));
            Assert.AreEqual(1, guids.Length, "The project needs exactly one board kit.");
            return AssetDatabase.LoadAssetAtPath<BoardKit>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        [Test]
        public void Build_EveryEditorShapeInEveryTurn_FillsExactlyTheCellsOfTheShape()
        {
            BoardKit kit = LoadProjectKit();

            for (int i = 0; i < BlockShapes.All.Count; i++)
            {
                IReadOnlyList<Cell> shape = BlockShapes.All[i].Cells;
                for (int turn = 0; turn < 4; turn++)
                {
                    AssertMeshFillsShape(shape, kit, BlockShapes.All[i].Name + ", turn " + turn);
                    shape = ShapeTransform.RotateClockwise(shape);
                }
            }
        }

        private static void AssertMeshFillsShape(IReadOnlyList<Cell> shape, BoardKit kit, string shapeName)
        {
            int width = 0;
            int height = 0;
            for (int i = 0; i < shape.Count; i++)
            {
                width = Mathf.Max(width, shape[i].X + 1);
                height = Mathf.Max(height, shape[i].Y + 1);
            }

            Mesh mesh = BlockMeshBuilder.Build(shape, kit);
            try
            {
                Bounds bounds = mesh.bounds;
                Assert.AreEqual(0f, bounds.min.x, Tolerance, shapeName + ": left side");
                Assert.AreEqual(0f, bounds.min.z, Tolerance, shapeName + ": lower side");
                Assert.AreEqual(width * BoardSpace.CellSize, bounds.max.x, Tolerance, shapeName + ": right side");
                Assert.AreEqual(height * BoardSpace.CellSize, bounds.max.z, Tolerance, shapeName + ": upper side");
                Assert.AreEqual(-BoardView.BlockRestHeight, bounds.min.y, Tolerance, shapeName + ": underside");

                AssertEveryVertexLiesInACell(mesh, shape, shapeName);
            }
            finally
            {
                Object.DestroyImmediate(mesh);
            }
        }

        private static void AssertEveryVertexLiesInACell(Mesh mesh, IReadOnlyList<Cell> shape, string shapeName)
        {
            Vector3[] vertices = mesh.vertices;
            for (int v = 0; v < vertices.Length; v++)
            {
                bool isInside = false;
                for (int i = 0; i < shape.Count && !isInside; i++)
                {
                    float x = vertices[v].x - shape[i].X * BoardSpace.CellSize;
                    float z = vertices[v].z - shape[i].Y * BoardSpace.CellSize;
                    isInside = x > -RoundedCornerReach && x < BoardSpace.CellSize + RoundedCornerReach
                        && z > -RoundedCornerReach && z < BoardSpace.CellSize + RoundedCornerReach;
                }

                Assert.IsTrue(isInside, shapeName + ": a vertex at " + vertices[v] + " lies outside the cells of the shape.");
            }
        }
    }
}
