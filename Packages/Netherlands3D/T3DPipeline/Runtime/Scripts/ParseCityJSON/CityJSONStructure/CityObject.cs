using Netherlands3D.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// A City Object is a JSON object for which the type member’s value is one of the following (of type string):
    /// </summary>
    public enum CityObjectType
    {
        // 1000-2000: 1st level city objects
        Building = 1000,
        Bridge = 1010,
        CityObjectGroup = 1020,
        CityFurniture = 1030,
        GenericCityObject = 1040,
        LandUse = 1050,
        PlantCover = 1060,
        Railway = 1070,
        Road = 1080,
        SolitaryVegetationObject = 1090,
        TINRelief = 1100,
        TransportSquare = 1110,
        Tunnel = 1120,
        WaterBody = 1130,

        //2000-3000: 2nd level city objects. the middle numbers indicates the required parent. e.g 200x has to be a parent of 1000, 201x of 1010 etc.
        BuildingPart = 2000,
        BuildingInstallation = 2001,
        BridgePart = 2010,
        BridgeInstallation = 2011,
        BridgeConstructionElement = 2012,

        TunnelPart = 2120,
        TunnelInstallation = 2121
    }

    public class CityObject : MonoBehaviour
    {
        /// <summary>
        /// Each CityObject has to have a unique id. This class also supports a prefix
        /// </summary>
        public string Id { get; private set; }
        //private static string idPrefix = "NL.IMBAG.Pand.";
        //public static string IdPrefix
        //{
        //    get => idPrefix;
        //    set
        //    {
        //        idPrefix = value;
        //        print("set id prefix value to: " + value);
        //    }
        //}
        //private static int IdCounter = 0;

        /// <summary>
        /// Each CityObject must have a Type. Each type has a different depth of arrays in arrays for the geometry boundaries
        /// </summary>
        public CityObjectType Type { get; set; }
        public CoordinateSystem CoordinateSystem { get; private set; }
        public Vector3Double MinExtent { get; private set; }
        public Vector3Double MaxExtent { get; private set; }
        public Vector3Double RelativeCenter { get { return (MaxExtent - MinExtent) / 2; } }
        public Vector3Double AbsoluteCenter { get { return MinExtent + RelativeCenter; } }

        public List<CityGeometry> Geometries { get; private set; }
        protected JSONNode attributes;

        protected List<CityObject> cityChildren = new List<CityObject>();
        public CityObject[] CityChildren => cityChildren.ToArray();
        public CityObject[] CityParents { get; private set; } = new CityObject[0];

        private void OnEnable()
        {
            CityJSONFormatter.AddCityObejct(this);
        }
        private void OnDisable()
        {
            CityJSONFormatter.RemoveCityObject(this);
        }

        public void SetParents(CityObject[] newParents)
        {
            // remove this as the child of old parents
            foreach (var parent in CityParents)
            {
                parent.cityChildren.Remove(this);
            }

            // add this as child of new parents
            foreach (var parent in newParents)
            {
                Assert.IsTrue(IsValidParent(this, parent));
                parent.cityChildren.Add(this);
            }
            // set newparents for this
            CityParents = newParents;
        }

        public static bool IsValidParent(CityObject child, CityObject parent)
        {
            if (parent == null && ((int)child.Type < 2000))
                return true;

            if ((int)((int)child.Type / 10 - 200) == (int)((int)parent.Type / 10 - 100) || ((int)child.Type / 10) == ((int)parent.Type / 10))
                return true;


            //Debug.Log(child.Type + "\t" + parent, child.gameObject);
            return false;
        }

        public JSONObject GetJsonObjectAndAddVertices(Dictionary<Vector3Double, int> currentCityJSONVertices)
        {
            var obj = new JSONObject();
            obj["type"] = Type.ToString();
            if (CityParents.Length > 0)
            {
                var parents = new JSONArray();
                for (int i = 0; i < CityParents.Length; i++)
                {
                    Assert.IsTrue(IsValidParent(this, CityParents[i]));
                    parents[i] = CityParents[i].Id;
                }
                obj["parents"] = parents;
            }
            if (CityChildren.Length > 0)
            {
                var children = new JSONArray();
                for (int i = 0; i < CityChildren.Length; i++)
                {
                    children[i] = CityChildren[i].Id;
                }
                obj["children"] = children;
            }


            var geometryArray = new JSONArray();
            foreach (var g in Geometries)
            {
                geometryArray.Add(g.GetGeometryNodeAndAddVertices(currentCityJSONVertices));
            }
            obj["geometry"] = geometryArray;

            var attributes = GetAttributes();
            if (attributes.Count > 0)
                obj["attributes"] = attributes;

            var extentArray = new JSONArray();
            extentArray.Add(MinExtent.x);
            extentArray.Add(MinExtent.y);
            extentArray.Add(MinExtent.z);
            extentArray.Add(MaxExtent.x);
            extentArray.Add(MaxExtent.y);
            extentArray.Add(MaxExtent.z);
            obj["geographicalExtent"] = extentArray;

            return obj;
        }

        public List<Vector3Double> GetGeometryVertices()
        {
            List<Vector3Double> geometryVertices = new List<Vector3Double>();
            foreach (var g in Geometries)
            {
                geometryVertices = geometryVertices.Concat(g.GetVertices()).ToList();
            }
            return geometryVertices;
        }

        protected virtual JSONObject GetAttributes()
        {
            var obj = attributes;
            //obj.Add("annotations", GetAnnotationNode());
            return obj.AsObject;
        }

        public void FromJSONNode(string id, JSONNode cityObjectNode, CoordinateSystem coordinateSystem, List<Vector3Double> combinedVertices)
        {
            Id = id;
            Type = (CityObjectType)Enum.Parse(typeof(CityObjectType), cityObjectNode["type"]);
            CoordinateSystem = coordinateSystem;
            Geometries = new List<CityGeometry>();
            var geometryNode = cityObjectNode["geometry"];
            foreach (var g in geometryNode)
            {
                var geometry = CityGeometry.FromJSONNode(g.Value, combinedVertices);
                Assert.IsTrue(CityGeometry.IsValidType(Type, geometry.Type));
                Geometries.Add(geometry);
            }
            attributes = cityObjectNode["attributes"];

            var geographicalExtent = cityObjectNode["geographicalExtent"];
            if (geographicalExtent.Count > 0)
            {
                MinExtent = new Vector3Double(geographicalExtent[0].AsDouble, geographicalExtent[1].AsDouble, geographicalExtent[2].AsDouble);
                MaxExtent = new Vector3Double(geographicalExtent[3].AsDouble, geographicalExtent[4].AsDouble, geographicalExtent[5].AsDouble);
            }
            else
            {
                var verts = GetGeometryVertices();
                var minX = verts.Min(v => v.x);
                var minY = verts.Min(v => v.y);
                var minZ = verts.Min(v => v.z);
                var maxX = verts.Max(v => v.x);
                var maxY = verts.Max(v => v.y);
                var maxZ = verts.Max(v => v.z);

                MinExtent = new Vector3Double(minX, minY, minZ);
                MaxExtent = new Vector3Double(maxX, maxY, maxZ);
            }

            //Parents and Children cannot be added here because they might not be parsed yet. Setting parents/children happens in CityJSONParser after all objects have been created.
        }

        //protected JSONObject GetAnnotationNode()
        //{
        //    var annotationNode = new JSONObject();
        //    foreach (var ann in AnnotationState.AnnotationUIs)
        //    {
        //        if (Id == ann.ParentCityObject)
        //        {
        //            var annotation = new JSONObject();
        //            var point = new JSONArray();
        //            point.Add("x", ann.ConnectionPointRD.x);
        //            point.Add("y", ann.ConnectionPointRD.y);
        //            point.Add("z", ann.ConnectionPointRD.z);
        //            annotation.Add("location", point);
        //            annotation.Add("text", ann.Text);
        //            annotationNode.Add("Annotation " + (ann.Id + 1), annotation);
        //        }
        //    }
        //    return annotationNode;
        //}

    }
}