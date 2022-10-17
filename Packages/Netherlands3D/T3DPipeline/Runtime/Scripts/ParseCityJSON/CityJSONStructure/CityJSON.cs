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

        public string Version { get; private set; }
        public JSONNode Extensions { get; private set; }
        public JSONNode Metadata { get; private set; }
        public Vector3Double TransformScale { get; private set; } = new Vector3Double(1d, 1d, 1d);
        public Vector3Double TransformTranslate { get; private set; } = new Vector3Double(0d, 0d, 0d);
        public JSONNode Appearance { get; private set; }
        public JSONNode GeometryTemplates { get; private set; }

        public CityObject[] CityObjects { get; private set; }
        public Vector3Double MinExtent { get; private set; }
        public Vector3Double MaxExtent { get; private set; }
        public CoordinateSystem CoordinateSystem { get; private set; }

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
                co.gameObject.GetComponent<CityObjectVisualizer>();
            }
            string exportJson = CityJSONFormatter.GetCityJSON();
            print(exportJson);
            //HandleTextFile.WriteString("export.json", exportJson);
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
            Appearance = node["appearance"]; //todo: not implemented yet
            GeometryTemplates = node["geometry-templates"]; //todo: not implemented yet
            var transformNode = node["transform"];

            if (transformNode.Count > 0)
            {
                TransformScale = new Vector3Double(transformNode["scale"][0], transformNode["scale"][1], transformNode["scale"][2]);
                TransformTranslate = new Vector3Double(transformNode["translate"][0], transformNode["translate"][1], transformNode["translate"][2]);
            }

            AddExtensionNodesToExporter(node);

            //vertices
            List<Vector3Double> parsedVertices = new List<Vector3Double>();
            foreach (var vertArray in node["vertices"])
            {
                var vert = new Vector3Double(vertArray.Value.AsArray);
                vert *= TransformScale;
                vert += TransformTranslate;
                parsedVertices.Add(vert);
            }

            if (Metadata.Count > 0)
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
                Debug.Log("Setting Relative RD Center to: " + relativeCenterRD);
                CoordConvert.zeroGroundLevelY = (float)relativeCenterRD.z;
                CoordConvert.relativeCenterRD = new Vector2RD(relativeCenterRD.x, relativeCenterRD.y);
            }
        }
    }
}
