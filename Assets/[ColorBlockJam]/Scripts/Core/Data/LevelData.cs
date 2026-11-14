using System;
using System.Collections.Generic;

namespace ColorBlockJam.Core.Data
{
    [Serializable]
    public sealed class LevelData
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public int Width;
        public int Height;
        public int TimeLimitSeconds;
        public List<BlockData> Blocks = new List<BlockData>();
        public List<DoorData> Doors = new List<DoorData>();
    }
}
