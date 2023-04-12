using System;
using System.Collections;

namespace subtree
{

    public static class BitstreamReader
    {
        public static BitArray Read(byte[] subtreeBinary, int offset, int length)
        {
            var slicedBytes = new Span<byte>(subtreeBinary).Slice(start: offset, length: length);
            return new BitArray(slicedBytes.ToArray());
        }
    }
}
