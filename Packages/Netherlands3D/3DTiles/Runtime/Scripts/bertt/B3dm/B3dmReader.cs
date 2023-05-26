using System;
using System.IO;
using System.Linq;
using System.Text;

public static class B3dmReader
{
    private static Encoding encoding = Encoding.GetEncoding("Windows-1252");

    public static B3dm ReadB3dm(BinaryReader reader)
    {
        var b3dmHeader = new B3dmHeader(reader);
        var featureTableJson = encoding.GetString(reader.ReadBytes(b3dmHeader.FeatureTableJsonByteLength));
        var featureTableBytes = reader.ReadBytes(b3dmHeader.FeatureTableBinaryByteLength);
        var batchTableJson = encoding.GetString(reader.ReadBytes(b3dmHeader.BatchTableJsonByteLength));
        var batchTableBytes = reader.ReadBytes(b3dmHeader.BatchTableBinaryByteLength);

        var glbLength = b3dmHeader.ByteLength - b3dmHeader.Length;
        var glbBuffer = reader.ReadBytes(glbLength);

        // remove the trailing glb padding characters if any
        glbBuffer = glbBuffer.TakeWhile((v, index) => glbBuffer.Skip(index).Any(w => (w != 0x20))).ToArray();

        var b3dm = new B3dm {
            B3dmHeader = b3dmHeader,
            GlbData = glbBuffer,
            FeatureTableJson = featureTableJson,
            FeatureTableBinary = featureTableBytes,
            BatchTableJson = batchTableJson,
            BatchTableBinary = batchTableBytes
        };
        return b3dm;
    }

    public static B3dm ReadB3dm(Stream stream)
    {
        using (var reader = new BinaryReader(stream)) {
            var b3dm = ReadB3dm(reader);
            return b3dm;
        }
    }
}
