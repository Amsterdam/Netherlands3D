using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3DPipeline;
using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// Class to Format a CityJSON and export all the CityObjects.
    /// </summary>
    public static class CityJSONFormatter
    {
        private static JSONObject rootObject;
        private static JSONObject metadata;
        private static JSONObject cityObjects;
        private static JSONArray vertices;
        private static JSONObject presentLoDs;
        private static Dictionary<string, JSONNode> extensionNodes = new Dictionary<string, JSONNode>();

        public static List<CityObject> CityObjects { get; private set; } = new List<CityObject>();

        public static void Reset()
        {
            CityObjects = new List<CityObject>();
            extensionNodes = new Dictionary<string, JSONNode>();
        }

        // Main function to format the CityJSON
        public static string GetCityJSON()
        {
            rootObject = new JSONObject();
            cityObjects = new JSONObject();
            vertices = new JSONArray();
            metadata = new JSONObject();
            presentLoDs = new JSONObject();

            metadata.Add("referenceSystem", "urn:ogc:def:crs:EPSG::28992");

            rootObject["type"] = "CityJSON";
            rootObject["version"] = "1.0";
            rootObject["metadata"] = metadata;
            rootObject["CityObjects"] = cityObjects;

            //var indexOffset = RDVertices.Count;
            Dictionary<Vector3Double, int> currentCityJSONVertices = new Dictionary<Vector3Double, int>();
            foreach (var obj in CityObjects)
            {
                var cityObjectNode = obj.GetJsonObjectAndAddVertices(currentCityJSONVertices); // currentCityJSONVertices gets updated in CityPolygon.cs
                cityObjects[obj.Id] = cityObjectNode;

                foreach (var geometry in cityObjectNode["geometry"])
                {
                    var lodKey = geometry.Value["lod"].ToString();
                    var lodCount = presentLoDs[lodKey].AsInt;
                    presentLoDs[lodKey] = lodCount + 1;
                }

            }
            foreach (var vertPair in currentCityJSONVertices)
            {
                var vert = vertPair.Key;
                var index = vertPair.Value;

                var vertArray = new JSONArray();
                vertArray.Add(vert.x);
                vertArray.Add(vert.y);
                vertArray.Add(vert.z);
                vertices[index] = vertArray;
            }
            rootObject["vertices"] = vertices;
            metadata.Add("presentLoDs", presentLoDs);
            metadata.Add("geographicalExtent", GetGeographicalExtents(currentCityJSONVertices));

            foreach (var node in extensionNodes)
            {
                rootObject[node.Key] = node.Value;
            }

            return rootObject.ToString();
        }

        private static JSONArray GetGeographicalExtents(Dictionary<Vector3Double, int> vertices)
        {
            var extentArray = new JSONArray();
            var minX = vertices.Keys.MinBy(v => v.x).x;
            var minY = vertices.Keys.MinBy(v => v.y).y;
            var minZ = vertices.Keys.MinBy(v => v.z).z;

            var maxX = vertices.Keys.MinBy(v => -v.x).x; //there is only a MinBy extension function, so multiply by -1 to be able to use this
            var maxY = vertices.Keys.MinBy(v => -v.y).y;
            var maxZ = vertices.Keys.MinBy(v => -v.z).z;

            extentArray.Add(minX);
            extentArray.Add(minY);
            extentArray.Add(minZ);
            extentArray.Add(maxX);
            extentArray.Add(maxY);
            extentArray.Add(maxZ);

            return extentArray;
        }

        //register city object to be added to the JSON when requested
        public static void AddCityObject(CityObject obj)
        {
            CityObjects.Add(obj);
        }

        public static void RemoveCityObject(CityObject obj)
        {
            CityObjects.Remove(obj);
        }

        // Add extra nodes that are not part of the main specs to the CityJSON
        public static void AddExtensionNode(string key, JSONNode node)
        {
            if (extensionNodes.ContainsKey(key))
                extensionNodes[key] = node;
            else
                extensionNodes.Add(key, node);
        }

        public static void RemoveExtensionNode(string key)
        {
            if (extensionNodes.ContainsKey(key))
                extensionNodes.Remove(key);
        }
    }
}
