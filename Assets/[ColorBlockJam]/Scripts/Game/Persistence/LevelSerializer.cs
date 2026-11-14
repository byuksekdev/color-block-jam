using System;
using ColorBlockJam.Core.Data;
using UnityEngine;

namespace ColorBlockJam.Game.Persistence
{
    /// <summary>The one place that turns a level into file text and back. The level editor writes with it and the game reads with it.</summary>
    public static class LevelSerializer
    {
        public static string ToJson(LevelData level)
        {
            return JsonUtility.ToJson(level, true);
        }

        public static LevelData FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new FormatException("The level file is empty.");
            }

            LevelData level;
            try
            {
                level = JsonUtility.FromJson<LevelData>(json);
            }
            catch (ArgumentException exception)
            {
                throw new FormatException("The level file is not valid JSON.", exception);
            }

            if (level == null)
            {
                throw new FormatException("The level file holds no level.");
            }

            return level;
        }
    }
}
