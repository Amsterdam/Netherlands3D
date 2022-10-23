using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// A representation of a polygon, with vertices and indices.
    /// </summary>
    public class CityPolygon
    {
        public int[] LocalBoundaries { get; set; }
        public Vector3Double[] Vertices { get; set; } // used by the CityJSONFormatter to add to the total vertices object
        public int Count => LocalBoundaries.Length;

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

        // Return a JSONArray of the polygon, and combine the vertices of this polygon with the provided dictionary of existing vertices.
        public JSONArray GetJSONPolygonAndAddNewVertices(bool isHole, Dictionary<Vector3Double, int> currentCityJSONVertices)
        {
            int[] absoluteBoundaries = new int[LocalBoundaries.Length];
            var indexOffset = currentCityJSONVertices.Count;
            for (int i = 0; i < LocalBoundaries.Length; i++)
            {
                var localIndex = LocalBoundaries[i];
                var vert = Vertices[localIndex];
                if (currentCityJSONVertices.ContainsKey(vert)) //renumber the boundary and use the existing vertex
                {
                    var absoluteIndex = currentCityJSONVertices[vert];
                    absoluteBoundaries[i] = absoluteIndex;
                    indexOffset--; // reusing a vertex means that when adding a new vertex, the local index needs to be offset by one less to retain an increment of 1
                }
                else
                {
                    absoluteBoundaries[i] = localIndex + indexOffset;
                    currentCityJSONVertices.Add(vert, absoluteBoundaries[i]); //add new vertex
                }
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
