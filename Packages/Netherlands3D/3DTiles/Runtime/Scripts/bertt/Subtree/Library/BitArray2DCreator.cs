using System;

namespace subtree
{

    public class BitArray2DCreator
    {
        public static BitArray2D GetBitArray2D(string availability)
        {
            var width = GetWidth(availability);
            var result = new BitArray2D(width, width);

            var bitarray = BitArrayCreator.FromString(availability);
            for (uint x = 0; x < width; x++)
            {
                for (uint y = 0; y < width; y++)
                {
                    var mortonIndex = MortonOrder.Encode2D(x, y);
                    var cel = bitarray.Get((int)mortonIndex);
                    result.Set((int)x, (int)y, cel);
                }

            }
            return result;
        }

        public static int GetWidth(string mortonIndex)
        {
            var length = mortonIndex.Length;
            var size = Math.Sqrt(length);
            return (int)size;
        }

    }
}
