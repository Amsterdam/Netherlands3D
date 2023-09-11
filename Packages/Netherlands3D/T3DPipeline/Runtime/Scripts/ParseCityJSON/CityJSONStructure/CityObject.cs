using System;
using System.Collections.Generic;
using System.Linq;
using Netherlands3D.Coordinates;
using Netherlands3D.Core;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

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
        // Each CityObject has to have a unique id. This class also supports a prefix
        public string Id { get; protected set; }
        // Each CityObject must have a Type. Each type has a different depth of arrays in arrays for the geometry boundaries
        public CityObjectType Type { get; set; }
        public CoordinateSystem CoordinateSystem { get; protected set; }
        public Vector3Double MinExtent { get; protected set; }
        public Vector3Double MaxExtent { get; protected set; }
        public Vector3Double RelativeCenter { get { return (MaxExtent - MinExtent) / 2; } }
        public Vector3Double AbsoluteCenter { get { return (MaxExtent + MinExtent) / 2; } }

        public List<CityGeometry> Geometries { get; protected set; } = new List<CityGeometry>();
        public List<CityObjectAttribute> Attributes { get; protected set; } = new List<CityObjectAttribute>();

        protected List<CityObject> cityChildren = new List<CityObject>();
        public CityObject[] CityChildren => cityChildren.ToArray();
        public CityObject[] CityParents { get; private set; } = new CityObject[0];

        public UnityEvent CityObjectParsed { get; private set; } = new UnityEvent();

        private bool includeInExport;
        public bool IncludeInExport
        {
            get
            {
                return includeInExport;
            }
            set
            {
                if (value)
                    CityJSONFormatter.AddCityObject(this);
                else
                    CityJSONFormatter.RemoveCityObject(this);

                includeInExport = value;
            }
        }

        public void UnparentFromAll()
        {
            SetParents(new CityObject[] { });
        }

        public void SetId(string newId)
        {
            Id = newId;
            gameObject.name = newId;
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

        // A CityObject of a certain type may only be a child of an object of a certain type. this function tests that validity. See specs for details
        public static bool IsValidParent(CityObject child, CityObject parent)
        {
            if (parent == null && ((int)child.Type < 2000))
                return true;

            if ((int)((int)child.Type / 10 - 200) == (int)((int)parent.Type / 10 - 100) || ((int)child.Type / 10) == ((int)parent.Type / 10))
                return true;


            //Debug.Log(child.Type + "\t" + parent, child.gameObject);
            return false;
        }

        //Get the CityObject as a JSONObject, append the missing vertices, and renumber this CityObject's boundaries to the combined vertex list
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

        //returns a list of vertices, without removing duplicates
        public List<Vector3Double> GetUncombinedGeometryVertices()
        {
            List<Vector3Double> geometryVertices = new List<Vector3Double>();
            foreach (var g in Geometries)
            {
                geometryVertices = geometryVertices.Concat(g.GetUncombinedVertices()).ToList();
            }
            return geometryVertices;
        }

        protected virtual JSONObject GetAttributes()
        {
            var obj = new JSONObject();
            foreach (var attribute in Attributes)
            {
                obj.Add(attribute.Key, attribute.GetJSONValue());
            }
            return obj;
        }

        public void AddAttribute(CityObjectAttribute attribute)
        {
            Attributes.Add(attribute);
        }

        public static CityObject CreateEmpty(string id, CityObjectType type = CityObjectType.GenericCityObject, CoordinateSystem coordinateSystem = CoordinateSystem.RD)
        {
            var co = new GameObject().AddComponent<CityObject>();

            co.SetId(id);
            co.Type = type;
            co.CoordinateSystem = coordinateSystem;
            co.Geometries = new List<CityGeometry>();
            co.Attributes = new List<CityObjectAttribute>();

            return co;
        }

        public CityGeometry AddEmptyGeometry(GeometryType type, int lod, bool includeSemantics, bool includeMaterials, bool includeTextures)
        {
            var geometry = new CityGeometry(type, lod, includeSemantics, includeMaterials, includeTextures);

            Assert.IsTrue(CityGeometry.IsValidType(Type, geometry.Type));
            Geometries.Add(geometry);
            return geometry;
        }

        public void FromJSONNode(string id, JSONNode cityObjectNode, CoordinateSystem coordinateSystem, List<Vector3Double> combinedVertices)
        {
            SetId(id);
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
            Attributes = CityObjectAttribute.ParseAttributesNode(this, cityObjectNode["attributes"]);

            var geographicalExtent = cityObjectNode["geographicalExtent"];
            if (geographicalExtent.Count > 0)
            {
                MinExtent = new Vector3Double(geographicalExtent[0].AsDouble, geographicalExtent[1].AsDouble, geographicalExtent[2].AsDouble);
                MaxExtent = new Vector3Double(geographicalExtent[3].AsDouble, geographicalExtent[4].AsDouble, geographicalExtent[5].AsDouble);
            }
            else
            {
                RecalculateExtents();
            }

            //Parents and Children cannot be added here because they might not be parsed yet. Setting parents/children happens in CityJSONParser after all objects have been created.
        }

        public void RecalculateExtents()
        {
            var verts = GetUncombinedGeometryVertices();
            if (verts.Count > 0)
            {
                var minX = verts.Min(v => v.x);
                var minY = verts.Min(v => v.y);
                var minZ = verts.Min(v => v.z);
                var maxX = verts.Max(v => v.x);
                var maxY = verts.Max(v => v.y);
                var maxZ = verts.Max(v => v.z);

                MinExtent = new Vector3Double(minX, minY, minZ);
                MaxExtent = new Vector3Double(maxX, maxY, maxZ);
            }
        }

        //called by CityJSON.cs when CityObject is fully parsed and ready for further processing (such as visualization)
        public void OnCityObjectParseCompleted()
        {
            CityObjectParsed.Invoke();
        }
    }
}
