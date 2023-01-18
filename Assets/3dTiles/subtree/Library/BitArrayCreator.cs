using System.Collections;

namespace subtree
{

    public static class BitArrayCreator
    {
        public static BitArray FromString(string bits)
        {
            var bitArray = new BitArray(bits.Length, false);
            for (var i = 0; i < bits.Length; i++)
            {
                var c = bits[i];
                if (c == '1')
                {
                    bitArray[i] = true;
                }
            }
            return bitArray;
        }

    }
}
