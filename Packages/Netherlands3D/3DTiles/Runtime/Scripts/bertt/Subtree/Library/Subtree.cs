#nullable enable

using System.Collections;
using System.Collections.Generic;

namespace subtree
{

    public record Subtree
    {
        public Subtree()
        {
            SubtreeHeader = new SubtreeHeader();
            TileAvailabiltyConstant = 0;
            ContentAvailabiltyConstant = 0;
        }

        public SubtreeHeader SubtreeHeader { get; set; } = null!;
        public string SubtreeJson { get; set; } = null!;
        public byte[] SubtreeBinary { get; set; } = null!;

        public BitArray? ChildSubtreeAvailability { get; set; }
        public BitArray? ContentAvailability { get; set; } = null!;

        public BitArray TileAvailability { get; set; } = null!;

        public int TileAvailabiltyConstant { get; set; }


        public int ContentAvailabiltyConstant { get; set; }
        public List<string> GetExpectedSubtreeFiles()
        {
            var subtreefiles = new List<string>();
            if (ChildSubtreeAvailability != null)
            {
                var length = ChildSubtreeAvailability.Length;
                var level = Level.GetLevel(length);

                var childSubtreeAvailability = BitArray2DCreator.GetBitArray2D(ChildSubtreeAvailability.AsString());

                for (var x = 0; x < childSubtreeAvailability.GetWidth(); x++)
                {
                    for (var y = 0; y < childSubtreeAvailability.GetHeight(); y++)
                    {
                        if (childSubtreeAvailability.Get(x, y))
                        {
                            subtreefiles.Add($"{level}.{x}.{y}.subtree");
                        }
                    }
                }
            }

            return subtreefiles;
        }
    }
}
