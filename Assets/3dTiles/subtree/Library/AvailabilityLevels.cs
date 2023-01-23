using System;
using System.Collections.Generic;
using System.Text;

namespace subtree
{


    public class AvailabilityLevels : List<AvailabilityLevel>
    {
        public string ToMortonIndex()
        {
            var res = new StringBuilder();
            foreach (var level in this)
            {
                res.Append(level.ToMortonIndex());
            }
            return res.ToString();
        }
    }

    public class AvailabilityLevel
    {
        private int width;
        private int height;

        public AvailabilityLevel(int level)
        {
            Level = level;
            width = (int)Math.Sqrt(Math.Pow(4, level));
            height = (int)Math.Sqrt(Math.Pow(4, level));
            BitArray2D = new BitArray2D(width, height);
        }
        public int Level { get; set; }

        public BitArray2D BitArray2D { get; set; }

        public string ToMortonIndex()
        {
            var s = new char[width * height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var index = MortonOrder.Encode2D((uint)x, (uint)y);
                    s[index] = BitArray2D.Get(x, y) ? '1' : '0';
                }
            }
            return new string(s);
        }
    }
}