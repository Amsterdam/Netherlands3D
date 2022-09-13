using System;
namespace Netherlands3D.ModelParsing
{
    [Serializable]
    public class SubMeshData
    {
        public long vertexOffset = 0;
        public long startIndex;
        public long Indexcount;
        public string materialname;
    }

}