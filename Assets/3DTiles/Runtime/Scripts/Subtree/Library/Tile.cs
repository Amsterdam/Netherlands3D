
using System.Collections.Generic;
using UnityEngine;

namespace subtree
{
    [System.Serializable]
    public class Tile
    {
        public Tile(int z, int x, int y)
        {
            Z = z;
            X = x;
            Y = y;
        }

        public Tile(int z, int x, int y, bool available) : this(z, x, y)
        {
            Available = available;
        }

        public int Z;
        public int X;
        public int Y;

        public List<Tile> Children()
        {
            var t1 = new Tile(X * 2, Y * 2, Z + 1);
            var t2 = new Tile(X * 2 + 1, Y * 2, Z + 1);
            var t3 = new Tile(X * 2 + 1, Y * 2 + 1, Z + 1);
            var t4 = new Tile(X * 2, Y * 2 + 1, Z + 1);
            return new List<Tile>() { t1, t2, t3, t4 };
        }
        public bool Available { get; set; }
    }
}
