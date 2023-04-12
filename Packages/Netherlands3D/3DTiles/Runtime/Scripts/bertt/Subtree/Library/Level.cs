using System;

namespace subtree
{

    public static class Level
    {
        public static int GetLevel(int bitStreamLength)
        {
            // for quadtree, use 8 for octree
            var level = Math.Log(bitStreamLength) / Math.Log(4);
            return Convert.ToInt32(level);
        }
    }
}
