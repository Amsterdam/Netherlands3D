using System.Linq;
using System.Text;

namespace subtree
{

    public static class BufferPadding
    {
        private static int boundary = 8;
        public static byte[] AddPadding(byte[] bytes, int offset = 0)
        {
            var remainder = (offset + bytes.Length) % boundary;
            var padding = (remainder == 0) ? 0 : boundary - remainder;
            var whitespace = new string(' ', padding);
            var paddingBytes = Encoding.UTF8.GetBytes(whitespace);
            var res = bytes.Concat(paddingBytes);
            return res.ToArray();
        }
        public static string AddPadding(string input, int offset = 0)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var paddedBytes = BufferPadding.AddPadding(bytes, offset);
            var result = Encoding.UTF8.GetString(paddedBytes);
            return result;
        }

        public static byte[] AddBinaryPadding(byte[] bytes, int offset = 0)
        {
            var remainder = (offset + bytes.Length) % boundary;
            var padding = (remainder == 0) ? 0 : boundary - remainder;
            var res = bytes.Concat(GetByteArray(padding));
            return res.ToArray();
        }
        public static byte[] GetByteArray(int length)
        {
            var arr = new byte[length];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = 0;
            }
            return arr;
        }
    }
}
