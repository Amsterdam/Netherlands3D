using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace subtree
{

    public static class SubtreeWriter
    {
        public static byte[] ToBytes(string tileAvailability, string contentAvailability, string subtreeAvailability = null)
        {
            var subtree_root = new Subtree();
            var tileavailability = BitArrayCreator.FromString(tileAvailability);

            subtree_root.TileAvailability = tileavailability;

            var s0_root = BitArrayCreator.FromString(contentAvailability);
            subtree_root.ContentAvailability = s0_root;

            if (subtreeAvailability != null)
            {
                var c0_root = BitArrayCreator.FromString(subtreeAvailability);
                subtree_root.ChildSubtreeAvailability = c0_root;
            }

            var subtreebytes = ToBytes(subtree_root);
            return subtreebytes;
        }

        public static byte[] ToBytes(Subtree subtree)
        {
            var bin = ToSubtreeBinary(subtree);
            var subtreeJsonPadded = BufferPadding.AddPadding(JsonConvert.SerializeObject(bin.subtreeJson, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }));
            var subtreeBinaryPadded = BufferPadding.AddPadding(bin.bytes);

            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);
            var subtreeHeader = new SubtreeHeader();
            subtreeHeader.JsonByteLength = (ulong)subtreeJsonPadded.Length;
            subtreeHeader.BinaryByteLength = (ulong)subtreeBinaryPadded.Length;

            binaryWriter.Write(subtreeHeader.AsBinary());
            binaryWriter.Write(Encoding.UTF8.GetBytes(subtreeJsonPadded));
            binaryWriter.Write(bin.bytes);

            binaryWriter.Close();
            var arr = memoryStream.ToArray();
            return arr;
        }

        public static (byte[] bytes, SubtreeJson subtreeJson) ToSubtreeBinary(Subtree subtree)
        {
            var substreamBinary = new List<byte>();
            var subtreeJson = new SubtreeJson();
            var bufferViews = new List<Bufferview>();

            if (subtree.TileAvailability != null)
            {
                var resultTileAvailability = HandleBitArray(subtree.TileAvailability);
                bufferViews.Add(resultTileAvailability.bufferView);
                substreamBinary.AddRange(resultTileAvailability.bytes.ToArray());
                subtreeJson.tileAvailability = new Tileavailability() { bitstream = 0, availableCount = resultTileAvailability.trueBits };
            }
            else
            {
                subtreeJson.tileAvailability = new Tileavailability() { constant = subtree.TileAvailabiltyConstant };
            }

            if (subtree.ContentAvailability != null)
            {
                var resultContentAvailability = HandleBitArray(subtree.ContentAvailability);
                var bufferView = resultContentAvailability.bufferView;
                bufferView.byteOffset = substreamBinary.Count;
                subtreeJson.contentAvailability = new List<Contentavailability>() { new Contentavailability() { bitstream = bufferViews.Count, availableCount = resultContentAvailability.trueBits } }.ToArray();
                bufferViews.Add(bufferView);
                substreamBinary.AddRange(resultContentAvailability.bytes.ToArray());
            }
            else
            {
                subtreeJson.contentAvailability = new List<Contentavailability>() { new Contentavailability() { constant = subtree.ContentAvailabiltyConstant } }.ToArray();
            }

            if (subtree.ChildSubtreeAvailability != null)
            {
                var resultSubstreamAvailability = HandleBitArray(subtree.ChildSubtreeAvailability);
                var bufferView = resultSubstreamAvailability.bufferView;
                bufferView.byteOffset = substreamBinary.Count;
                subtreeJson.childSubtreeAvailability = new Childsubtreeavailability() { bitstream = bufferViews.Count, availableCount = resultSubstreamAvailability.trueBits };
                bufferViews.Add(bufferView);
                substreamBinary.AddRange(resultSubstreamAvailability.bytes.ToArray());
            }
            else
            {
                subtreeJson.childSubtreeAvailability = new Childsubtreeavailability() { constant = 0 };
            }

            subtreeJson.buffers = new List<Buffer>() { new Buffer() { byteLength = substreamBinary.Count } }.ToArray();
            subtreeJson.bufferViews = bufferViews.ToArray();
            return (substreamBinary.ToArray(), subtreeJson);
        }

        private static (List<byte> bytes, int trueBits, Bufferview bufferView) HandleBitArray(BitArray bitArray)
        {
            var trueBits = 0;
            trueBits += bitArray.Count(true);
            var bits = bitArray.ToByteArray();
            var bytes = BufferPadding.AddBinaryPadding(bits);

            var bufferView = new Bufferview() { buffer = 0, byteLength = bits.Length, byteOffset = 0 };
            return (bytes.ToList(), trueBits, bufferView);
        }
    }
}