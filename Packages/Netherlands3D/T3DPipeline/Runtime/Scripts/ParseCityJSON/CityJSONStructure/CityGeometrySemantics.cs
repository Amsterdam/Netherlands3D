using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine.Assertions;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// This class serves as the nested array of indices to point to which semantic object the boundary object with a corresponding index points.
    /// The index of the int Value is the index of the boundary, while the int value is the index of the semantic object that the boundary has.
    /// The value can be null, in which case the boundary at that index has no semantics
    /// </summary>
    public class CitySemanticsValues
    {
        //the value of "values" is a hierarchy of arrays with integers.
        //The depth depends on the Geometry object: for MultiPoint and MultiLineString this is a simple array of integers;
        //for any other geometry type it is two less than the array "boundaries".
        // The Array field manages thenest depth of the arrays.
        private List<CitySemanticsValues> Array;
        public List<int?> Values; // a Value can be null, this means the corresponding boundary has no semantics
        public int Depth; //depth of the current arary

        public CitySemanticsValues(int depth)
        {
            Depth = depth;
            if (depth == 0)
                Values = new List<int?>(); //if this is the deepest nest of the array, Values will be used.
            else
                Array = new List<CitySemanticsValues>(); //if this is not the deepest nest of the array, an new Array will be used with a depth of this.Depth - 1.
        }

        public void FromJSONArray(JSONArray jsonArray)
        {
            if (Depth == 0)
            {
                // A null value is used to specify that a given surface has no semantics, but to avoid having arrays filled with null, it is also possible to specify null for a shell or a whole Solid in a MultiSolid, the null propagates to the nested arrays.
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
                // A null value is used to specify that a given surface has no semantics, but to avoid having arrays filled with null, it is also possible to specify null for a shell or a whole Solid in a MultiSolid, the null propagates to the nested arrays.
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
            // A null value is used to specify that a given surface has no semantics, but to avoid having arrays filled with null, it is also possible to specify null for a shell or a whole Solid in a MultiSolid, the null propagates to the nested arrays.
            //--- CityJSON 1.0 allows for Null propagation of arrays. CityJSON 1.1 seems to have lost support for this so delete this if-block when upgrading
            if (Values == null && Array == null)
                return null;
            //---

            if (Depth == 0)
            {
                foreach (var i in Values)
                    jsonObject.Add(i); //add the int values to the export
            }
            else
            {
                foreach (var arr in Array)
                    jsonObject.Add(arr.GetValuesArray()); //recurse one deeper if this is not the deepest array
            }
            return jsonObject;
        }
    }

    /// <summary>
    /// This class serves as the object that describes one set of semantics.
    /// from: https://www.cityjson.org/specs/1.0.3/#semantics-of-geometric-primitives
    /// </summary>
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
        private static string[] definedNodes = { "type", "parent", "children" }; // Nodes defined in the specs. Other nodes can be user defined.

        public SurfaceSemanticType SurfaceType { get; set; } // A Semantic Object must have one member with the name "type", whose value is one of the allowed value.These depend on the City Object, see below.
        private CityGeometrySemanticsObject semanticParent;  // A Semantic Object may have an attribute "parent".
        private List<CityGeometrySemanticsObject> semanticChildren = new List<CityGeometrySemanticsObject>(); // A semantic object may have multiple children
        private JSONObject customAttributes = new JSONObject(); // User defined attributes.

        // Certain semantic types may only belong to a city object of a certain type. This function tests if the type is valid. See specs for full details
        public static bool IsValidSemanticType(CityObjectType parent, SurfaceSemanticType type)
        {
            if (type == SurfaceSemanticType.Null) //no semantic type is not allowed
                return false;

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

        //parse JSONNode to semantics object. Parents/children must be set outside of this function because that object might not be parsed yet and therefore cannot be referenced.
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

        // Get the semantic object to add to the CityJSON export. An ordered list of all semantic objects of the CityObject is needed because the indices are used as the values.
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
                node.Add(customAttribute.Key, customAttribute.Value); //todo: this currently correcttly exports invalid types such as JSON objects (valid JSON, invalid CityJSON). The value must not be a JSON object (but a string/number/integer/boolean).
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

    /// <summary>
    /// This class represents the entire semantics object of a Geometry. It contains a list of semantic objects and a hierarchy of arrays of values, represented by the values object.
    /// </summary>
    public class CityGeometrySemantics
    {
        private List<CityGeometrySemanticsObject> surfaces = new List<CityGeometrySemanticsObject>();
        private CitySemanticsValues values;

        //parse an existing Semantics node
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

        //export the semantics object
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
            node["values"] = values.GetValuesArray();

            return node;
        }
    }
}
