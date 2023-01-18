namespace subtree
{

    public static class MortonOrder
    {
        public static uint Encode2D(uint x, uint y)
        {
            return (insertOneSpacing(x) | (insertOneSpacing(y) << 1)) >> 0;
        }

        public static (uint x, uint y) Decode2D(uint mortonIndex)
        {
            var x = removeOneSpacing(mortonIndex);
            var y = removeOneSpacing(mortonIndex >> 1);
            return (x, y);
        }

        private static uint insertOneSpacing(uint v)
        {
            v = (v ^ (v << 8)) & 0x00ff00ff;
            v = (v ^ (v << 4)) & 0x0f0f0f0f;
            v = (v ^ (v << 2)) & 0x33333333;
            v = (v ^ (v << 1)) & 0x55555555;
            return v;
        }

        private static uint removeOneSpacing(uint v)
        {
            v &= 0x55555555;
            v = (v ^ (v >> 1)) & 0x33333333;
            v = (v ^ (v >> 2)) & 0x0f0f0f0f;
            v = (v ^ (v >> 4)) & 0x00ff00ff;
            v = (v ^ (v >> 8)) & 0x0000ffff;
            return v;
        }
    }
}
