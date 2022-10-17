using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3DPipeline;
using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    public static class CityJSONFormatter
    {
        private static JSONObject RootObject;
        private static JSONObject Metadata;
        private static JSONObject cityObjects;
        private static JSONArray Vertices;
        private static JSONArray RDVertices;
        private static JSONObject presentLoDs;
        private static Dictionary<string, JSONNode> extensionNodes = new Dictionary<string, JSONNode>();

        public static List<CityObject> CityObjects { get; private set; } = new List<CityObject>();

        public static void Reset()
        {
            CityObjects = new List<CityObject>();
            extensionNodes = new Dictionary<string, JSONNode>();
        }

        public static string GetCityJSON()
        {
            RootObject = new JSONObject();
            cityObjects = new JSONObject();
            Vertices = new JSONArray();
            RDVertices = new JSONArray();
            Metadata = new JSONObject();
            presentLoDs = new JSONObject();

            Metadata.Add("referenceSystem", "urn:ogc:def:crs:EPSG::28992");

            RootObject["type"] = "CityJSON";
            RootObject["version"] = "1.0";
            RootObject["metadata"] = Metadata;
            RootObject["CityObjects"] = cityObjects;

            var indexOffset = RDVertices.Count;
            foreach (var obj in CityObjects)
            {
                var cityObjectNode = obj.GetJsonObject(indexOffset);
                var verts = obj.GetGeometryVertices(); // getting vertices like this is inefficient but readable.
                indexOffset += verts.Count;
                foreach (var vert in verts) //todo: remove duplicate vertices, and make indices point to the same one, HashSet<T> is probably fastest for this
                {
                    var vertArray = new JSONArray();
                    vertArray.Add(vert.x);
                    vertArray.Add(vert.y);
                    vertArray.Add(vert.z);
                    RDVertices.Add(vertArray);
                }
                cityObjects[obj.Id] = cityObjectNode;

                foreach (var geometry in cityObjectNode["geometry"])
                {
                    var lodKey = geometry.Value["lod"].ToString();
                    var lodCount = presentLoDs[lodKey].AsInt;
                    presentLoDs[lodKey] = lodCount + 1;
                }

            }
            RootObject["vertices"] = RDVertices;
            Metadata.Add("presentLoDs", presentLoDs);

            foreach (var node in extensionNodes)
            {
                RootObject[node.Key] = node.Value;
            }

            //todo geographical extents
            //if (convertToRD)
            //    RecalculateGeographicalExtents(RDVertices);
            //else
            //    RecalculateGeographicalExtents(Vertices);

            return RootObject.ToString();
        }

        //register city object to be added to the JSON when requested
        public static void AddCityObejct(CityObject obj)
        {
            CityObjects.Add(obj);
        }

        public static void RemoveCityObject(CityObject obj)
        {
            CityObjects.Remove(obj);
        }

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
