using System;
using System.IO;
using System.Text;

namespace B3dm.Tile
{
    public static class B3dmReader
    {
        public static B3dm ReadB3dm(BinaryReader reader)
        {
            var b3dmHeader = new B3dmHeader(reader);

            var featureTableJsonBuffer = new byte[b3dmHeader.FeatureTableJsonByteLength];
            reader.Read(featureTableJsonBuffer, 0, b3dmHeader.FeatureTableJsonByteLength);
            var featureTableJson = Encoding.UTF8.GetString(featureTableJsonBuffer);

            var featureTableBytesBuffer = new byte[b3dmHeader.FeatureTableBinaryByteLength];
            reader.Read(featureTableBytesBuffer, 0, b3dmHeader.FeatureTableBinaryByteLength);
            var featureTableBytes = new byte[b3dmHeader.FeatureTableBinaryByteLength];
            Buffer.BlockCopy(featureTableBytesBuffer, 0, featureTableBytes, 0, b3dmHeader.FeatureTableBinaryByteLength);

            var batchTableJsonBuffer = new byte[b3dmHeader.BatchTableJsonByteLength];
            reader.Read(batchTableJsonBuffer, 0, b3dmHeader.BatchTableJsonByteLength);
            var batchTableJson = Encoding.UTF8.GetString(batchTableJsonBuffer);

            var batchTableBytesBuffer = new byte[b3dmHeader.BatchTableBinaryByteLength];
            reader.Read(batchTableBytesBuffer, 0, b3dmHeader.BatchTableBinaryByteLength);
            var batchTableBytes = new byte[b3dmHeader.BatchTableBinaryByteLength];
            Buffer.BlockCopy(batchTableBytesBuffer, 0, batchTableBytes, 0, b3dmHeader.BatchTableBinaryByteLength);

            var glbLength = b3dmHeader.ByteLength - b3dmHeader.Length;
            var glbBuffer = new byte[glbLength];
            reader.Read(glbBuffer, 0, glbLength);
            var glbBytes = new byte[glbLength];
            Buffer.BlockCopy(glbBuffer, 0, glbBytes, 0, glbLength);

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

        public static byte[] ReadB3dmGlbContentOnly(MemoryStream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                //Read the header to determine our length and offset
                var b3dmHeader = new B3dmHeader(reader);

                byte[] buffer = new byte[b3dmHeader.ByteLength - b3dmHeader.Length];
                stream.Position = b3dmHeader.Length;
                stream.Read(buffer);

                return buffer;
            }
        }
    }
}