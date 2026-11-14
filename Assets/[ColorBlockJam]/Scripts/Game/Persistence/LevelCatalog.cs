using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Data;
using UnityEngine;

namespace ColorBlockJam.Game.Persistence
{
    /// <summary>The level files in play order. The level editor keeps this list; the game loads levels from it.</summary>
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField] private List<TextAsset> _levels = new List<TextAsset>();

        public int Count => _levels.Count;

        public LevelData Load(int index)
        {
            TextAsset file = _levels[index];
            if (file == null)
            {
                throw new InvalidOperationException("Level " + (index + 1) + " has no file in the level catalog.");
            }

            return LevelSerializer.FromJson(file.text);
        }
    }
}
