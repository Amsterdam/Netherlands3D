using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text;

namespace subtree
{

    public static class SubtreeReader
    {
        public static Subtree ReadSubtree(BinaryReader reader)
        {
            var subtreeHeader = new SubtreeHeader(reader);
            var subtreeJson = Encoding.UTF8.GetString(reader.ReadBytes((int)subtreeHeader.JsonByteLength));
            var subTreeBinary = reader.ReadBytes((int)subtreeHeader.BinaryByteLength);
            var subtree = new Subtree
            {
                SubtreeHeader = subtreeHeader,
                SubtreeJson = subtreeJson,
                SubtreeBinary = subTreeBinary
            };

            var subtreeJsonObject = JsonConvert.DeserializeObject<SubtreeJson>(subtree.SubtreeJson);
            if (subtreeJsonObject != null)
            {
                if (subtreeJsonObject.tileAvailability != null && subtreeJsonObject.tileAvailability.bitstream != null)
                {
                    var bufferViewTileAvailability = subtreeJsonObject.bufferViews[(int)subtreeJsonObject.tileAvailability.bitstream];
                    subtree.TileAvailability = BitstreamReader.Read(subtree.SubtreeBinary, bufferViewTileAvailability.byteOffset, bufferViewTileAvailability.byteLength);
                }
                else
                {
                    subtree.TileAvailabiltyConstant = (int)subtreeJsonObject.tileAvailability.constant;
                }

                // todo: implement multiple content (do not use first() here)...
                var contentBitstream = subtreeJsonObject.contentAvailability.First().bitstream;
                if (contentBitstream != null)
                {
                    var bufferViewContent = subtreeJsonObject.bufferViews[(int)contentBitstream];
                    subtree.ContentAvailability = BitstreamReader.Read(subtree.SubtreeBinary, bufferViewContent.byteOffset, bufferViewContent.byteLength);
                }
                else
                {
                    // todo: implement multiple content (do not use first() here)...
                    subtree.ContentAvailabiltyConstant = (int)subtreeJsonObject.contentAvailability.First().constant;
                }

                if (subtreeJsonObject.childSubtreeAvailability != null && subtreeJsonObject.childSubtreeAvailability.bitstream != null)
                {
                    var bufferViewChildsubtree = subtreeJsonObject.bufferViews[(int)subtreeJsonObject.childSubtreeAvailability.bitstream];
                    subtree.ChildSubtreeAvailability = BitstreamReader.Read(subtree.SubtreeBinary, bufferViewChildsubtree.byteOffset, bufferViewChildsubtree.byteLength);
                }
            }

            return subtree;
        }

        public static Subtree ReadSubtree(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                var subtree = ReadSubtree(reader);
                return subtree;
            }
        }
    }
}
