using System.Collections;
using System.Text;
using System.Linq;

namespace subtree
{

    public static class BitArrayExtensions
    {
        public static byte[] ToByteArray(this BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }
        public static string AsString(this BitArray bitArray)
        {
            var sb = new StringBuilder();
            foreach (var b in bitArray)
            {
                sb.Append((bool)b ? "1" : "0");
            }
            return sb.ToString();
        }

        public static int Count(this BitArray bitArray, bool whereClause = false)
        {
            return (from bool m in bitArray where m == whereClause select m).Count();
        }
    }
}
