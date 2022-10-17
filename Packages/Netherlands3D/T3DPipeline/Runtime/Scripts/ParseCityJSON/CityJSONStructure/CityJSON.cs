using Netherlands3D.Core;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Netherlands3D.T3DPipeline
{
    public class CityJSON : MonoBehaviour
    {
        private static string[] definedNodes = { "type", "version", "CityObjects", "vertices", "extensions", "metadata", "transform", "appearance", "geometry-templates" };

        public string Version;
        public JSONNode Extensions;
        public JSONNode Metadata;
        public JSONNode Transform; //todo
        public JSONNode Appearance;
        public JSONNode GeometryTemplates;

        public CityObject[] CityObjects { get; private set; }
        public Vector3Double MinExtent, MaxExtent;
        public CoordinateSystem CoordinateSystem;

        [SerializeField]
        private TextAsset testJson;
        [SerializeField]
        private GameObject cityObjectPrefab;
        [SerializeField]
        private bool useAsRelativeRDCenter;

        protected void Start()
        {
            print(testJson.text);
            //var cityObjects = CityJSONParser.ParseCityJSON(testJson.text);
            //var parsedJson = new CityJSON(testJson.text, true);
            ParseCityJSON(testJson.text, useAsRelativeRDCenter);

            foreach (var co in CityObjects)
            {
                co.gameObject.AddComponent<CityObjectVisualizer>();
            }
            string exportJson = CityJSONFormatter.GetCityJSON();
            print(exportJson);
            HandleTextFile.WriteString("export.json", exportJson);
        }

        public void ParseCityJSON(string cityJson, bool useAsRelativeRDCenter)
        {
            var node = JSONNode.Parse(cityJson);
            var type = node["type"];
            Assert.IsTrue(type == "CityJSON");
            Version = node["version"];

            //optional data
            Extensions = node["extensions"];
            Metadata = node["metadata"];
            Transform = node["transform"];
            Appearance = node["appearance"];
            GeometryTemplates = node["geometry-templates"];

            AddExtensionNodesToExporter(node);

            //vertices
            List<Vector3Double> parsedVertices = new List<Vector3Double>();
            foreach (var vert in node["vertices"])
            {
                parsedVertices.Add(new Vector3Double(vert.Value.AsArray));
            }

            if (!Metadata.IsNull)
            {
                var coordinateSystemNode = Metadata["referenceSystem"];
                CoordinateSystem = ParseCoordinateSystem(coordinateSystemNode);

                var geographicalExtent = Metadata["geographicalExtent"];
                if (geographicalExtent.Count > 0)
                {
                    MinExtent = new Vector3Double(geographicalExtent[0].AsDouble, geographicalExtent[1].AsDouble, geographicalExtent[2].AsDouble);
                    MaxExtent = new Vector3Double(geographicalExtent[3].AsDouble, geographicalExtent[4].AsDouble, geographicalExtent[5].AsDouble);
                }
                else
                {
                    var minX = parsedVertices.Min(v => v.x);
                    var minY = parsedVertices.Min(v => v.y);
                    var minZ = parsedVertices.Min(v => v.z);
                    var maxX = parsedVertices.Max(v => v.x);
                    var maxY = parsedVertices.Max(v => v.y);
                    var maxZ = parsedVertices.Max(v => v.z);

                    MinExtent = new Vector3Double(minX, minY, minZ);
                    MaxExtent = new Vector3Double(maxX, maxY, maxZ);
                }
            }

            Dictionary<JSONNode, CityObject> cityObjects = new Dictionary<JSONNode, CityObject>();
            foreach (var cityObjectNode in node["CityObjects"])
            {
                var go = Instantiate(cityObjectPrefab, transform);
                go.name = cityObjectNode.Key;
                var co = go.GetComponent<CityObject>();
                co.FromJSONNode(cityObjectNode.Key, cityObjectNode.Value, CoordinateSystem, parsedVertices);
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

            CityObjects = cityObjects.Values.ToArray();

            if (useAsRelativeRDCenter)
                SetRelativeCenter();
        }

        private static CoordinateSystem ParseCoordinateSystem(JSONNode coordinateSystemNode)
        {
            Debug.Log("Parsing coordinate system: " + coordinateSystemNode.Value);
            if (coordinateSystemNode.Value == "urn:ogc:def:crs:EPSG::28992")
                return CoordinateSystem.RD;
            if (coordinateSystemNode.Value == "urn:ogc:def:crs:EPSG::4979")
                return CoordinateSystem.WGS84;

            Debug.Log("Parsing coordinateSystem failed, using Unity Coordinate Sytem");
            return CoordinateSystem.Unity;
        }

        public static void AddExtensionNodesToExporter(JSONNode cityJsonNode)
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

        private void SetRelativeCenter()
        {
            if (CoordinateSystem == CoordinateSystem.RD)
            {
                var relativeCenterRD = (MinExtent + MaxExtent) / 2;
                Debug.Log("Setting Relative RD Center to: x:" + relativeCenterRD.x + "\ty:" + relativeCenterRD.y + "\th:" + relativeCenterRD.z);
                CoordConvert.zeroGroundLevelY = (float)relativeCenterRD.z;
                CoordConvert.relativeCenterRD = new Vector2RD(relativeCenterRD.x, relativeCenterRD.y);
            }
        }
    }
}
