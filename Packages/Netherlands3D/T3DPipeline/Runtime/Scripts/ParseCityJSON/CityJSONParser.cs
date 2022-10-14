using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Netherlands3D.T3DPipeline
{
    public static class CityJSONParser
    {
        private static string[] definedNodes = { "type", "version", "CityObjects", "vertices", "extensions", "metadata", "transform", "appearance", "geometry-templates" };

        public static string version;
        public static JSONNode extensions;
        public static JSONNode metadata;
        public static JSONNode transform;
        public static JSONNode appearance;
        public static JSONNode geometryTemplates;

        public static void ParseCityJSON(string cityJson)
        {
            var node = JSONNode.Parse(cityJson);
            var type = node["type"];
            Assert.IsTrue(type == "CityJSON");
            version = node["version"].ToString();

            //vertices
            List<Vector3Double> parsedVertices = new List<Vector3Double>();
            foreach (var vert in node["vertices"])
            {
                parsedVertices.Add(new Vector3Double(vert.Value.AsArray));
            }
            Dictionary<JSONNode, CityObject> cityObjects = new Dictionary<JSONNode, CityObject>();
            foreach (var cityObjectNode in node["CityObjects"])
            {
                var go = new GameObject(cityObjectNode.Key);
                var co = go.AddComponent<CityObject>();
                co.FromJSONNode(cityObjectNode.Key, cityObjectNode.Value, parsedVertices);
                cityObjects.Add(cityObjectNode.Value, co);
            }

            //after creating all the objects, set the parent/child relations. this can only be done after, since the referenced parent/child might not exist yet during initialization in the previous loop
            foreach (var co in cityObjects)
            {
                var parents = co.Key["parents"];
                var parentObjects = new CityObject[parents.Count];
                for (int i = 0; i < parents.Count; i++)
                {
                    string parentId = parents[i];
                    parentObjects[i] = cityObjects.First(co => co.Value.Id == parentId).Value;
                }
                co.Value.SetParents(parentObjects);
            }

            //optional data
            extensions = node["extensions"];
            metadata = node["metadata"];
            transform = node["transform"];
            appearance = node["appearance"];
            geometryTemplates = node["geometry-templates"];

            AddExtensionNodes(node);
        }

        public static void AddExtensionNodes(JSONNode cityJsonNode)
        {
            foreach (var node in cityJsonNode)
            {
                if (definedNodes.Contains(node.Key))
                    continue;

                CityJSONFormatter.AddExtensionNode(node.Key, node.Value);
            }
        }

        public static void RemoveExtensionNodes(JSONNode cityJsonNode)
        {
            foreach (var node in cityJsonNode)
            {
                if (definedNodes.Contains(node.Key))
                    continue;

                CityJSONFormatter.RemoveExtensionNode(node.Key);
            }
        }
    }
}
