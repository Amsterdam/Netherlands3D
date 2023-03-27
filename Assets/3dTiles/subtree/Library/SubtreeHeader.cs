using System;
using System.IO;
using System.Linq;
using System.Text;

namespace subtree
{

    public record SubtreeHeader
    {
        public SubtreeHeader()
        {
            Magic = "subt";
            Version = 1;
        }

        public SubtreeHeader(BinaryReader reader)
        {
            Magic = Encoding.UTF8.GetString(reader.ReadBytes(4));
            Version = (int)reader.ReadUInt32();
            JsonByteLength = reader.ReadUInt64();
            BinaryByteLength = reader.ReadUInt64();
        }
        public string Magic { get; set; }
        public int Version { get; set; }
        public UInt64 JsonByteLength { get; set; }
        public UInt64 BinaryByteLength { get; set; }

        public byte[] AsBinary()
        {
            var magicBytes = Encoding.UTF8.GetBytes(Magic);
            var versionBytes = BitConverter.GetBytes(Version);
            var jsonByteLength = BitConverter.GetBytes(JsonByteLength);
            var binaryByteLength = BitConverter.GetBytes(BinaryByteLength);


            return magicBytes.
                Concat(versionBytes).
                Concat(jsonByteLength).
                Concat(binaryByteLength).
                ToArray();
        }
    }
}

