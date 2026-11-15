using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using ColorBlockJam.Core.Validation;
using ColorBlockJam.Game.Persistence;
using NUnit.Framework;
using UnityEditor;

namespace ColorBlockJam.Tests.Game
{
    public class LevelCatalogTests
    {
        private static LevelCatalog LoadProjectCatalog()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(LevelCatalog));
            Assert.AreEqual(1, guids.Length, "The project needs exactly one level catalog.");
            return AssetDatabase.LoadAssetAtPath<LevelCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        [Test]
        public void ProjectCatalog_EveryLevel_IsWellFormedAndCanBeSolved()
        {
            LevelCatalog catalog = LoadProjectCatalog();
            Assert.Greater(catalog.Count, 0, "The level catalog is empty.");

            for (int i = 0; i < catalog.Count; i++)
            {
                LevelData level = catalog.Load(i);
                List<LevelProblem> problems = LevelValidator.Validate(level);
                Assert.AreEqual(0, problems.Count, "Level " + (i + 1) + ": " + (problems.Count > 0 ? problems[0].Message : string.Empty));

                SolverResult result = LevelSolver.Solve(level);
                Assert.AreNotEqual(Solvability.Unsolvable, result.Solvability, "Level " + (i + 1) + ": " + result.Reason);
            }
        }
    }
}
