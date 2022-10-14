using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    public class CityPolygon
    {
        public int[] LocalBoundaries { get; set; }
        public Vector3Double[] Vertices { get; set; } // used by the CityJSONFormatter to add to the total vertices object
        public int Count => LocalBoundaries.Length;
        //public bool BoundaryConverterIsSet { get; set; }

        //public Dictionary<int, int> localToAbsoluteBoundaryConverter { get; set; }  //note: this is only valid when generating the json

        public CityPolygon(Vector3Double[] vertices, int[] localBoundaries)
        {
            Vertices = vertices;
            LocalBoundaries = localBoundaries;
        }

        public CityPolygon()
        {
            Vertices = new Vector3Double[0];
            LocalBoundaries = new int[0];
        }

        //public void UpdateVertices(Vector3[] vertices)
        //{
        //    Vertices = vertices;
        //}

        public JSONArray GetJSONPolygon(bool isHole, int indexOffset)
        {
            int[] absoluteBoundaries = new int[LocalBoundaries.Length];
            for (int i = 0; i < LocalBoundaries.Length; i++)
            {
                absoluteBoundaries[i] = LocalBoundaries[i] + indexOffset; 
            }

            if (isHole)
                absoluteBoundaries = absoluteBoundaries.Reverse().ToArray();

            var boundaryArray = new JSONArray(); // defines a polygon (1st is surface, 2+ is holes in first surface)
            for (int i = 0; i < absoluteBoundaries.Length; i++)
            {
                boundaryArray.Add(absoluteBoundaries[i]);
            }

            return boundaryArray;
        }

        public static CityPolygon FomJsonNode(JSONArray polygonNode, List<Vector3Double> combinedVertices)
        {
            var localIndices = new int[polygonNode.Count];
            var localVertices = new Vector3Double[polygonNode.Count];

            for (int i = 0; i < polygonNode.Count; i++)
            {
                localIndices[i] = i;
                var absoluteIndex = polygonNode[i].AsInt;
                localVertices[i] = combinedVertices[absoluteIndex];
            }
            var polygon = new CityPolygon(localVertices, localIndices);
            return polygon;
        }
    }
}
