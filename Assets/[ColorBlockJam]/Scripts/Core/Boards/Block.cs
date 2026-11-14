using System;
using System.Collections.Generic;
using ColorBlockJam.Core.Data;

namespace ColorBlockJam.Core.Boards
{
    public sealed class Block
    {
        public Block(int id, BlockColor color, IReadOnlyList<Cell> shape)
        {
            Id = id;
            Color = color;
            Shape = shape;

            for (int i = 0; i < shape.Count; i++)
            {
                Width = Math.Max(Width, shape[i].X + 1);
                Height = Math.Max(Height, shape[i].Y + 1);
            }
        }

        public int Id { get; }
        public BlockColor Color { get; }

        /// <summary>The cells of the block, relative to its origin: the lower left corner of its bounds.</summary>
        public IReadOnlyList<Cell> Shape { get; }

        public int Width { get; }
        public int Height { get; }
    }
}
