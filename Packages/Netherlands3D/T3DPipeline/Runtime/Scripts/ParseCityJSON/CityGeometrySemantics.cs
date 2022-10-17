using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Netherlands3D.T3DPipeline
{
    public class CitySemanticsValues
    {
        private List<CitySemanticsValues> Array;
        public List<int?> Values;
        public int Depth;

        public CitySemanticsValues(int depth)
        {
            Depth = depth;
            if (depth == 0)
                Values = new List<int?>();
            else
                Array = new List<CitySemanticsValues>();
        }

        public void FromJSONArray(JSONArray jsonArray)
        {
            if (Depth == 0)
            {
                //--- CityJSON 1.0 allows for Null propagation of arrays. CityJSON 1.1 seems to have lost support for this so delete this if-block when upgrading
                if (jsonArray == null) 
                {
                    Values = null;
                    return;
                }
                //---

                foreach (var i in jsonArray)
                {
                    if (i.Value.IsNull)
                        Values.Add(null);
                    else
                        Values.Add(i.Value);
                }
            }
            else
            {
                //--- CityJSON 1.0 allows for Null propagation of arrays. CityJSON 1.1 seems to have lost support for this so delete this if-block when upgrading
                if (jsonArray == null)
                {
                    Array = null;
                    return;
                }
                //---

                foreach (var arr in jsonArray)
                {
                    var newIndex = Array.Count;
                    Array.Add(new CitySemanticsValues(Depth - 1));
                    Array[newIndex].FromJSONArray(arr.Value.AsArray);
                }
            }
        }

        public JSONArray GetValuesArray()
        {
            var jsonObject = new JSONArray();
            //--- CityJSON 1.0 allows for Null propagation of arrays. CityJSON 1.1 seems to have lost support for this so delete this if-block when upgrading
            if (Values == null && Array == null)
                return null;
            //---

            if (Depth == 0)
            {
                foreach (var i in Values)
                    jsonObject.Add(i);
            }
            else
            {
                foreach (var arr in Array)
                    jsonObject.Add(arr.GetValuesArray());
            }
            return jsonObject;
        }
    }

    public class CityGeometrySemantics
    {
        private List<CityGeometrySemanticsObject> surfaces = new List<CityGeometrySemanticsObject>();
        //private Dictionary<CityBoundary, CityGeometrySemanticsObject> values;
        private CitySemanticsValues values;

        public void FromJSONNode(JSONNode semanticsNode, CityBoundary boundaryObject)
        {
            surfaces = new List<CityGeometrySemanticsObject>();
            List<JSONNode> semanticObjectNodes = new List<JSONNode>();
            foreach (var surface in semanticsNode["surfaces"])
            {
                var semanticsObject = new CityGeometrySemanticsObject();
                semanticsObject.FromJSONNode(surface.Value);
                semanticObjectNodes.Add(surface.Value); //add to list so we can set the parents after all objects have been created.
                surfaces.Add(semanticsObject);
            }
            //after creating all the objects, set the parent/child relations. this can only be done after, since the referenced parent/child might not exist yet during initialization in the previous loop
            for (int i = 0; i < surfaces.Count; i++)
            {
                CityGeometrySemanticsObject surface = surfaces[i];
                var parent = semanticObjectNodes[i]["parent"];
                if (parent)
                {
                    int parentIndex = parent.AsInt;
                    var parentObject = surfaces[parentIndex];
                    surface.SetParent(parentObject);
                }
            }

            var valuesNode = semanticsNode["values"].AsArray;
            int depth = 0; //for all geometry types except Solid, MultiSolid, and CompositeSolid
            if (boundaryObject is CitySolid)
                depth = 1;
            else if (boundaryObject is CityMultiOrCompositeSolid)
                depth = 2;

            values = new CitySemanticsValues(depth);
            values.FromJSONArray(valuesNode);
        }

        public JSONNode GetSemanticObject()
        {
            var node = new JSONObject();
            var surfaceSemantics = new JSONArray();
            //var indices = new JSONArray();
            for (int i = 0; i < surfaces.Count; i++)
            {
                var surfaceSemanticNode = surfaces[i].GetSemanticObject(surfaces);
                surfaceSemantics.Add(surfaceSemanticNode);
                //indices.Add(i);
            }

            node["surfaces"] = surfaceSemantics;
            node["values"] = GetValuesArray();

            return node;
        }

        private JSONArray GetValuesArray()
        {
            return values.GetValuesArray();
        }

    }

    public class CityGeometrySemanticsObject
    {
        public enum SurfaceSemanticType
        {
            Null = 0,

            RoofSurface = 1000,
            GroundSurface = 1001,
            WallSurface = 1002,
            ClosureSurface = 1003,
            OuterCeilingSurface = 1004,
            OuterFloorSurface = 1005,
            Window = 1006,
            Door = 1007,

            WaterSurface = 1130,
            WaterGroundSurface = 1131,
            WaterClosureSurface = 1132,

            TrafficArea = 1080,
            AuxiliaryTrafficArea = 1081,
        }
        private static string[] definedNodes = { "type", "parent", "children" };

        public SurfaceSemanticType SurfaceType { get; set; }
        private CityGeometrySemanticsObject semanticParent;
        private List<CityGeometrySemanticsObject> semanticChildren = new List<CityGeometrySemanticsObject>();
        private JSONObject customAttributes = new JSONObject();

        public static bool IsValidSemanticType(CityObjectType parent, SurfaceSemanticType type)
        {
            if (type == SurfaceSemanticType.Null) //no semantic type is always allowed
                return true;

            var testInt = (int)type / 10;
            var parentInt = (int)parent / 10;

            if (testInt == parentInt) //default test
            {
                return true;
            }
            if (testInt == parentInt - 100) // child test
            {
                return true;
            }

            if (testInt == 108 && (parent == CityObjectType.Road || parent == CityObjectType.Railway || parent == CityObjectType.TransportSquare)) //custom test
            {
                return true;
            }
            return false;
        }

        public void FromJSONNode(JSONNode semanticsObjectNode)
        {
            SurfaceType = (SurfaceSemanticType)Enum.Parse(typeof(SurfaceSemanticType), semanticsObjectNode["type"]);

            customAttributes = new JSONObject();
            foreach (var attribute in semanticsObjectNode)
            {
                if (definedNodes.Contains(attribute.Key))
                    continue;

                customAttributes.Add(attribute.Key, attribute.Value);
            }
        }

        public JSONNode GetSemanticObject(List<CityGeometrySemanticsObject> allSemanticObjects)
        {
            var node = new JSONObject();
            node["type"] = SurfaceType.ToString();

            if (semanticParent != null)
                node["parent"] = GetParentIndex(allSemanticObjects);

            if (semanticChildren.Count > 0)
            {
                var childrenNode = new JSONArray();
                var childIndices = GetChildIndices(allSemanticObjects);
                foreach (var c in childIndices)
                {
                    childrenNode.Add(c);
                }
                node["children"] = childrenNode;
            }

            foreach (var customAttribute in customAttributes)
            {
                node.Add(customAttribute.Key, customAttribute.Value); //todo: currently correcttly exports invalid types such as JSON objects (valid JSON, invalid CityJSON)
            }

            return node;
        }

        public void SetParent(CityGeometrySemanticsObject newParent)
        {
            if (semanticParent != null)
                semanticParent.RemoveChild(this);

            semanticParent = newParent;

            if (semanticParent != null)
                newParent.AddChild(this);
        }

        private void AddChild(CityGeometrySemanticsObject child)
        {
            Assert.IsFalse(semanticChildren.Contains(child));
            semanticChildren.Add(child);
        }

        private void RemoveChild(CityGeometrySemanticsObject child)
        {
            semanticChildren.Remove(child);
        }

        private int GetParentIndex(List<CityGeometrySemanticsObject> allSemanticObjects)
        {
            return allSemanticObjects.IndexOf(semanticParent);
        }

        private int[] GetChildIndices(List<CityGeometrySemanticsObject> allSemanticObjects)
        {
            var array = new int[semanticChildren.Count];
            for (int i = 0; i < semanticChildren.Count; i++)
            {
                array[i] = allSemanticObjects.IndexOf(semanticChildren[i]);
            }
            return array;
        }

        //A Semantic Obejct may have other attributes in the form of a JSON key-value pair, where the value must not be a JSON object (but a string/number/integer/boolean).
        public void AddCustomAttribute(string key, string value)
        {
            customAttributes.Add(key, value);
        }

        public void AddCustomAttribute(string key, double value)
        {
            customAttributes.Add(key, value);
        }

        public void AddCustomAttribute(string key, int value)
        {
            customAttributes.Add(key, value);
        }

        public void AddCustomAttribute(string key, bool value)
        {
            customAttributes.Add(key, value);
        }

        public void RemoveCustomAttribute(string key)
        {
            customAttributes.Remove(key);
        }
    }
}
