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
        private const int RequiredLevelCount = 5;

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
            Assert.GreaterOrEqual(catalog.Count, RequiredLevelCount, "The game ships at least " + RequiredLevelCount + " levels.");

            for (int i = 0; i < catalog.Count; i++)
            {
                LevelData level = catalog.Load(i);
                List<LevelProblem> problems = LevelValidator.Validate(level);
                Assert.AreEqual(0, problems.Count, "Level " + (i + 1) + ": " + (problems.Count > 0 ? problems[0].Message : string.Empty));

                SolverResult result = LevelSolver.Solve(level);
                Assert.AreEqual(Solvability.Solvable, result.Solvability, "Level " + (i + 1) + ": " + result.Reason);
            }
        }
    }
}
