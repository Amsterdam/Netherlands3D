using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
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

        public static CitySemanticsValues GetValues(CityBoundary boundaryObject, out List<CityGeometrySemanticsObject> semanticObjects)
        {
            if (boundaryObject is CityMultiPoint)
            {
                var multiPoint = boundaryObject as CityMultiPoint;
                return GetMultiPointSemanticValues(multiPoint, out semanticObjects);
            }
            if(boundaryObject is CityMultiLineString)
            {
                var multiLineString = boundaryObject as CityMultiLineString;
                return GetMultiLineStringSemanticValues(multiLineString, out semanticObjects);
            }

            if (boundaryObject is CityMultiOrCompositeSurface)
            {
                var multiOrCompositeSurface = boundaryObject as CityMultiOrCompositeSurface;
                semanticObjects = new List<CityGeometrySemanticsObject>(); //the GetSemantics function below wants to know which semantic objects are already processed to that the index can be reused. It will add missing objects. Pass an empty list since currently no objects have been added yet. After this function, the list will contain all appropriate SemanticObjects.
                return GetMultiOrCompositeSurfaceSemantics(multiOrCompositeSurface, semanticObjects);
            }

            if (boundaryObject is CitySolid)
            {
                var solid = boundaryObject as CitySolid;
                semanticObjects = new List<CityGeometrySemanticsObject>(); //the GetSemantics function below wants to know which semantic objects are already processed to that the index can be reused. It will add missing objects. Pass an empty list since currently no objects have been added yet. After this function, the list will contain all appropriate SemanticObjects.
                return GetSolidSemantics(solid, semanticObjects);
            }

            if (boundaryObject is CityMultiOrCompositeSolid)
            {
                var multiOrCompositeSolid = boundaryObject as CityMultiOrCompositeSolid;
                semanticObjects = new List<CityGeometrySemanticsObject>(); //the GetSemantics function below wants to know which semantic objects are already processed to that the index can be reused. It will add missing objects. Pass an empty list since currently no objects have been added yet. After this function, the list will contain all appropriate SemanticObjects.
                return GetMultiOrCompositeSolidSemantics(multiOrCompositeSolid, semanticObjects);
            }

            throw new NotImplementedException("the type of this boundary object is not supported: " + boundaryObject.GetType());
        }

        private static CitySemanticsValues GetMultiPointSemanticValues(CityMultiPoint multiPoint, out List<CityGeometrySemanticsObject> surfaceSemantics)
        {
            var depth = CityGeometrySemantics.GetSemanticsValuesDepth(multiPoint); //0
            var semanticValues = new CitySemanticsValues(depth);
            var addedSemanticObjects = new List<CityGeometrySemanticsObject>();

            surfaceSemantics = multiPoint.SemanticsObjects;

            for (int i = 0; i < surfaceSemantics.Count; i++)
            {
                if (surfaceSemantics[i] == null)
                {
                    semanticValues.Values.Add(null);
                }
                else
                {
                    if (!addedSemanticObjects.Contains(surfaceSemantics[i]))
                        addedSemanticObjects.Add(surfaceSemantics[i]);

                    var index = addedSemanticObjects.IndexOf(surfaceSemantics[i]);
                    semanticValues.Values.Add(index);
                }
            }
            return semanticValues;
        }

        private static CitySemanticsValues GetMultiLineStringSemanticValues(CityMultiLineString multiLineString, out List<CityGeometrySemanticsObject> surfaceSemantics)
        {
            var depth = CityGeometrySemantics.GetSemanticsValuesDepth(multiLineString); //0
            var semanticValues = new CitySemanticsValues(depth);
            var addedSemanticObjects = new List<CityGeometrySemanticsObject>();

            surfaceSemantics = multiLineString.SemanticsObjects;

            for (int i = 0; i < surfaceSemantics.Count; i++)
            {
                if (surfaceSemantics[i] == null)
                {
                    semanticValues.Values.Add(null);
                }
                else
                {
                    if (!addedSemanticObjects.Contains(surfaceSemantics[i]))
                        addedSemanticObjects.Add(surfaceSemantics[i]);

                    var index = addedSemanticObjects.IndexOf(surfaceSemantics[i]);
                    semanticValues.Values.Add(index);
                }
            }
            return semanticValues;
        }

        private static CitySemanticsValues GetMultiOrCompositeSurfaceSemantics(CityMultiOrCompositeSurface multiOrCompositeSurface, List<CityGeometrySemanticsObject> existingSurfaceSemantics)
        {
            var depth = CityGeometrySemantics.GetSemanticsValuesDepth(multiOrCompositeSurface); //0
            var semanticValues = new CitySemanticsValues(depth);

            for (int i = 0; i < multiOrCompositeSurface.Surfaces.Count; i++)
            {
                var surface = multiOrCompositeSurface.Surfaces[i];
                if (surface.SemanticsObject == null)
                {
                    semanticValues.Values.Add(null);
                }
                else
                {
                    if (!existingSurfaceSemantics.Contains(surface.SemanticsObject))
                        existingSurfaceSemantics.Add(surface.SemanticsObject);

                    var index = existingSurfaceSemantics.IndexOf(surface.SemanticsObject);
                    semanticValues.Values.Add(index);
                }
            }
            return semanticValues;
        }

        private static CitySemanticsValues GetSolidSemantics(CitySolid solid, List<CityGeometrySemanticsObject> existingSurfaceSemantics)
        {
            var depth = CityGeometrySemantics.GetSemanticsValuesDepth(solid); //1
            var semanticValues = new CitySemanticsValues(depth);

            for (int i = 0; i < solid.Shells.Count; i++)
            {
                var shellValues = GetMultiOrCompositeSurfaceSemantics(solid.Shells[i], existingSurfaceSemantics);
                semanticValues.Array.Add(shellValues); //add the value object to the Array to maintain the correct depth structure
            }
            return semanticValues;
        }

        private static CitySemanticsValues GetMultiOrCompositeSolidSemantics(CityMultiOrCompositeSolid multiOrCompositeSolid, List<CityGeometrySemanticsObject> existingSurfaceSemantics)
        {
            var depth = CityGeometrySemantics.GetSemanticsValuesDepth(multiOrCompositeSolid); //2
            var semanticValues = new CitySemanticsValues(depth);

            for (int i = 0; i < multiOrCompositeSolid.Solids.Count; i++)
            {
                var solidValues = GetSolidSemantics(multiOrCompositeSolid.Solids[i], existingSurfaceSemantics);
                semanticValues.Array.Add(solidValues); //add the value object to the Array to maintain the correct depth structure
            }
            return semanticValues;
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

        public int? GetSemanticIndex(int surfaceIndex)
        {
            if (Depth != 0)
            {
                Debug.LogError("incorrect depth: " + Depth + ", expected: 0");
                return null;
            }

            if (Values != null)
                return Values[surfaceIndex];

            return null;
        }

        public int? GetSemanticIndex(int surfaceIndex, int shellIndex)
        {
            if (Depth != 1)
            {
                Debug.LogError("incorrect depth: " + Depth + ", expected: 1");
                return null;
            }

            if (Array != null)
                return Array[shellIndex].GetSemanticIndex(surfaceIndex);

            return null;
        }

        public int? GetSemanticIndex(int surfaceIndex, int shellIndex, int solidIndex)
        {
            if (Depth != 2)
            {
                Debug.LogError("incorrect depth: " + Depth + ", expected: 2");
                return null;
            }

            if (Array != null)
                return Array[solidIndex].GetSemanticIndex(surfaceIndex, shellIndex);

            return null;
        }
    }

    /// <summary>
    /// This enum defines the valid Surface semantic types
    /// from: https://www.cityjson.org/specs/1.0.3/#semantics-of-geometric-primitives
    /// </summary>
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

    /// <summary>
    /// This class serves as the object that describes one set of semantics.
    /// from: https://www.cityjson.org/specs/1.0.3/#semantics-of-geometric-primitives
    /// </summary>
    public class CityGeometrySemanticsObject
    {
        private static string[] definedNodes = { "type", "parent", "children" }; // Nodes defined in the specs. Other nodes can be user defined.

        public SurfaceSemanticType SurfaceType { get; set; } // A Semantic Object must have one member with the name "type", whose value is one of the allowed value.These depend on the City Object, see below.
        private CityGeometrySemanticsObject semanticParent;  // A Semantic Object may have an attribute "parent".
        private List<CityGeometrySemanticsObject> semanticChildren = new List<CityGeometrySemanticsObject>(); // A semantic object may have multiple children
        private JSONObject customAttributes = new JSONObject(); // User defined attributes.

        public CityGeometrySemanticsObject(SurfaceSemanticType surfaceType)
        {
            SurfaceType = surfaceType;
        }

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
        public static CityGeometrySemanticsObject FromJSONNode(JSONNode semanticsObjectNode)
        {
            var surfaceType = (SurfaceSemanticType)Enum.Parse(typeof(SurfaceSemanticType), semanticsObjectNode["type"]);
            var semanticsObject = new CityGeometrySemanticsObject(surfaceType);


            semanticsObject.customAttributes = new JSONObject();
            foreach (var attribute in semanticsObjectNode)
            {
                if (definedNodes.Contains(attribute.Key))
                    continue;

                semanticsObject.customAttributes.Add(attribute.Key, attribute.Value);
            }
            return semanticsObject;
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
    public static class CityGeometrySemantics
    {
        //parse an existing Semantics node
        public static void FromJSONNode(JSONNode semanticsNode, CityBoundary boundaryObject)
        {
            var surfaces = new List<CityGeometrySemanticsObject>();
            List<JSONNode> semanticObjectNodes = new List<JSONNode>();
            foreach (var surface in semanticsNode["surfaces"])
            {
                //var semanticsObject = new CityGeometrySemanticsObject();
                var semanticsObject = CityGeometrySemanticsObject.FromJSONNode(surface.Value);
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
            int depth = GetSemanticsValuesDepth(boundaryObject);


            var values = new CitySemanticsValues(depth);
            values.FromJSONArray(valuesNode);

            LinkSemanticObjectsToBoundaries(boundaryObject, values, surfaces);
        }

        private static void LinkSemanticObjectsToBoundaries(CityBoundary boundaryObject, CitySemanticsValues values, List<CityGeometrySemanticsObject> semanticObjects)
        {
            if (boundaryObject is CityMultiPoint)
            {
                var cityMultiPoint = boundaryObject as CityMultiPoint;
                cityMultiPoint.SemanticsObjects = new List<CityGeometrySemanticsObject>();
                for (int i = 0; i < cityMultiPoint.SemanticsObjects.Count; i++)
                {
                    var semanticIndex = values.GetSemanticIndex(i);
                    var semanticsObject = semanticIndex == null ? null : semanticObjects[(int)semanticIndex];
                    cityMultiPoint.SemanticsObjects.Add(semanticsObject);
                }
                return;
            }

            if (boundaryObject is CityMultiLineString)
            {
                var cityMultiLineString = boundaryObject as CityMultiLineString;
                cityMultiLineString.SemanticsObjects = new List<CityGeometrySemanticsObject>();
                for (int i = 0; i < cityMultiLineString.SemanticsObjects.Count; i++)
                {
                    var semanticIndex = values.GetSemanticIndex(i);
                    var semanticsObject = semanticIndex == null ? null : semanticObjects[(int)semanticIndex];
                    cityMultiLineString.SemanticsObjects.Add(semanticsObject);
                }
                return;
            }

            if (boundaryObject is CityMultiOrCompositeSurface)
            {
                var multiSurface = boundaryObject as CityMultiOrCompositeSurface;
                for (int i = 0; i < multiSurface.Surfaces.Count; i++)
                {
                    var semanticIndex = values.GetSemanticIndex(i);
                    var semanticsObject = semanticIndex == null ? null : semanticObjects[(int)semanticIndex];
                    CitySurface surface = multiSurface.Surfaces[i];
                    surface.SemanticsObject = semanticsObject;
                }
                return;
            }

            if (boundaryObject is CitySolid)
            {
                var solid = boundaryObject as CitySolid;
                for (int j = 0; j < solid.Shells.Count; j++)
                {
                    var shell = solid.Shells[j];
                    for (int i = 0; i < shell.Surfaces.Count; i++)
                    {
                        var semanticIndex = values.GetSemanticIndex(i, j);
                        var semanticsObject = semanticIndex == null ? null : semanticObjects[(int)semanticIndex];
                        CitySurface surface = shell.Surfaces[i];
                        surface.SemanticsObject = semanticsObject;
                    }
                }
            }

            if (boundaryObject is CityMultiOrCompositeSolid)
            {
                var multiSolid = boundaryObject as CityMultiOrCompositeSolid;
                for (int k = 0; k < multiSolid.Solids.Count; k++)
                {
                    var solid = multiSolid.Solids[k];
                    for (int j = 0; j < solid.Shells.Count; j++)
                    {
                        var shell = solid.Shells[j];
                        for (int i = 0; i < shell.Surfaces.Count; i++)
                        {
                            var semanticIndex = values.GetSemanticIndex(i, j, k);
                            var semanticsObject = semanticIndex == null ? null : semanticObjects[(int)semanticIndex];
                            CitySurface surface = shell.Surfaces[i];
                            surface.SemanticsObject = semanticsObject;
                        }
                    }
                }
            }
        }

        public static int GetSemanticsValuesDepth(CityBoundary boundaryObject)
        {
            if (boundaryObject is CitySolid)
                return 1;
            else if (boundaryObject is CityMultiOrCompositeSolid)
                return 2;

            return 0; //for all geometry types except Solid, MultiSolid, and CompositeSolid
        }

        //export the semantics object
        public static JSONNode GetSemanticObject(CityBoundary boundaryObject)
        {
            var node = new JSONObject();
            var surfaceSemantics = new JSONArray();

            var semanticValues = CitySemanticsValues.GetValues(boundaryObject, out var semanticObjects); //calculate the values array and associated SemanticObject list

            for (int i = 0; i < semanticObjects.Count; i++)
            {
                var surfaceSemanticNode = semanticObjects[i].GetSemanticObject(semanticObjects);
                surfaceSemantics.Add(surfaceSemanticNode);
            }

            node["surfaces"] = surfaceSemantics;
            node["values"] = semanticValues.GetValuesArray();

            return node;
        }
    }
}
