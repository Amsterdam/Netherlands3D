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
        public Vector3[] Vertices { get; set; } // used by the CityJSONFormatter to add to the total vertices object
        //public bool BoundaryConverterIsSet { get; set; }

        public Dictionary<int, int> localToAbsoluteBoundaryConverter { get; set; }  //note: this is only valid when generating the json

        public CityPolygon(Vector3[] vertices, int[] localBoundaries)
        {
            Vertices = vertices;
            LocalBoundaries = localBoundaries;
        }

        //public void UpdateVertices(Vector3[] vertices)
        //{
        //    Vertices = vertices;
        //}

        public JSONNode GetJSONPolygon(bool isHole/*, Dictionary<int, int> localToAbsoluteBoundaryConverter*/)
        {
            int[] absoluteBoundaries = new int[LocalBoundaries.Length];
            for (int i = 0; i < LocalBoundaries.Length; i++)
            {
                absoluteBoundaries[i] = localToAbsoluteBoundaryConverter[i]; //this relies on the CityJSONFormatter to set the absolute boundaries of this object before calling this function. This is not possible locally, since all vetices are stored in 1 big array in the CityJsonObject
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
    }
}
