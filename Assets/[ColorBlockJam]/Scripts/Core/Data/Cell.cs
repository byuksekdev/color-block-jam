using System;

namespace ColorBlockJam.Core.Data
{
    /// <summary>A board cell. X counts columns from the left, Y counts rows from the bottom.</summary>
    [Serializable]
    public struct Cell : IEquatable<Cell>
    {
        public int X;
        public int Y;

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Cell operator +(Cell a, Cell b)
        {
            return new Cell(a.X + b.X, a.Y + b.Y);
        }

        public static Cell operator -(Cell a, Cell b)
        {
            return new Cell(a.X - b.X, a.Y - b.Y);
        }

        public static bool operator ==(Cell a, Cell b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Cell a, Cell b)
        {
            return !a.Equals(b);
        }

        public bool Equals(Cell other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Cell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }
    }
}
