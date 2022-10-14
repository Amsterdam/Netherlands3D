using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Netherlands3D.T3DPipeline
{
    public abstract class CityBoundary
    {
        //public List<CityBoundary> Boundaries { get; private set; } = new List<CityBoundary>(1); //depth is dependant on GeometryType
        //public CityBoundary OuterBoundary => Boundaries[0];
        //public CityBoundary[] InteriorBoundaries => Boundaries.Skip(1).ToArray();

        public static readonly Dictionary<GeometryType, int> GeometryDepth = new Dictionary<GeometryType, int>{
            {GeometryType.MultiPoint, 0 }, //A "MultiPoint" has an array with the indices of the vertices; this array can be empty.
            {GeometryType.MultiLineString, 1 }, //A "MultiLineString" has an array of arrays, each containing the indices of a LineString
            {GeometryType.MultiSurface, 2 }, //A "MultiSurface", or a "CompositeSurface", has an array containing surfaces, each surface is modelled by an array of array, the first array being the exterior boundary of the surface, and the others the interior boundaries.
            {GeometryType.CompositeSurface, 2 },
            {GeometryType.Solid, 3 }, //A "Solid" has an array of shells, the first array being the exterior shell of the solid, and the others the interior shells. Each shell has an array of surfaces, modelled in the exact same way as a MultiSurface/CompositeSurface.
            {GeometryType.MultiSolid, 4 }, //A "MultiSolid", or a "CompositeSolid", has an array containing solids, each solid is modelled as above.
            {GeometryType.CompositeSolid, 4 },
        };

        public abstract int VertexCount { get; }
        public abstract void FromJSONNode(JSONArray boundariesNode, List<Vector3Double> combinedVertices);
        public abstract JSONArray GetBoundaries(int indexOffset);
        public abstract List<Vector3Double> GetVertices();
    }

    public class CityMultiPoint : CityBoundary
    {
        public CityPolygon Polygon = new CityPolygon();

        public override int VertexCount => Polygon.Count;
        public override void FromJSONNode(JSONArray boundariesNode, List<Vector3Double> combinedVertices)
        {
            Polygon = CityPolygon.FomJsonNode(boundariesNode, combinedVertices);
        }
        public override JSONArray GetBoundaries(int indexOffset)
        {
            return Polygon.GetJSONPolygon(false, indexOffset);
        }
        public override List<Vector3Double> GetVertices()
        {
            return Polygon.Vertices.ToList();
        }
    }

    public class CityMultiLineString : CityBoundary
    {
        public List<CityPolygon> Polygons = new List<CityPolygon>() { new CityPolygon() };
        public override int VertexCount
        {
            get
            {
                int count = 0;
                foreach (var polygon in Polygons)
                    count += polygon.Count;
                return count;
            }
        }

        public override void FromJSONNode(JSONArray boundariesNode, List<Vector3Double> combinedVertices)
        {
            Polygons = new List<CityPolygon>();
            foreach (var lineStringNode in boundariesNode)
            {
                var polygon = CityPolygon.FomJsonNode(lineStringNode.Value.AsArray, combinedVertices);
                Polygons.Add(polygon);
            }
        }

        public override JSONArray GetBoundaries(int indexOffset)
        {
            var node = new JSONArray();
            foreach (var polygon in Polygons)
            {
                var polygonNode = polygon.GetJSONPolygon(false, indexOffset);
                node.Add(polygonNode);
                indexOffset += polygon.Count;
            }
            return node;
        }
        public override List<Vector3Double> GetVertices()
        {
            var vertices = new List<Vector3Double>();
            foreach (var polygon in Polygons)
            {
                vertices = vertices.Concat(polygon.Vertices).ToList();
            }
            return vertices;
        }
    }

    public class CitySurface : CityBoundary
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

        public List<CityPolygon> Polygons = new List<CityPolygon>() { new CityPolygon() };
        public CityPolygon SolidSurfacePolygon => Polygons[0];
        public CityPolygon[] HolePolygons => Polygons.Skip(1).ToArray();

        public SurfaceSemanticType SurfaceType { get; set; }
        private List<CitySurface> semanticChildren = new List<CitySurface>();
        private CitySurface semanticParent;

        public override int VertexCount
        {
            get
            {
                int count = 0;
                foreach (var polygon in Polygons)
                    count += polygon.Count;
                return count;
            }
        }

        public override void FromJSONNode(JSONArray boundariesNode, List<Vector3Double> combinedVertices)
        {
            Polygons = new List<CityPolygon>();
            foreach (var polygonNode in boundariesNode)
            {
                //CityPolygon polygon = new CityPolygon();
                var polygon = CityPolygon.FomJsonNode(polygonNode.Value.AsArray, combinedVertices);
                Polygons.Add(polygon);
            }
        }

        public override JSONArray GetBoundaries(int indexOffset)
        {
            var surfaceArray = new JSONArray(); //defines the entire surface with holes

            // the following line and loop could be replaced by 1 loop through all the polygons of the surface, but separating them makes it clearer how the structure of the array works

            // add surface
            surfaceArray.Add(SolidSurfacePolygon.GetJSONPolygon(false, indexOffset));
            indexOffset += SolidSurfacePolygon.Count;
            // add holes
            var holes = HolePolygons;
            for (int i = 0; i < holes.Length; i++)
            {
                surfaceArray.Add(holes[i].GetJSONPolygon(true, indexOffset));
                indexOffset += holes[i].Count;
            }
            return surfaceArray;
        }

        public override List<Vector3Double> GetVertices()
        {
            var vertices = new List<Vector3Double>();
            foreach (var polygon in Polygons)
            {
                vertices = vertices.Concat(polygon.Vertices).ToList();
            }
            return vertices;
        }

        //public CitySurface(CityPolygon solidSurfacePolygon, SurfaceSemanticType type = SurfaceSemanticType.Null)
        //{
        //    SurfaceType = type;
        //    Polygons.Add(solidSurfacePolygon);
        //}

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

        public void TryAddHole(CityPolygon hole)
        {
            if (!Polygons.Contains(hole))
                Polygons.Add(hole);
        }

        public void TryRemoveHole(CityPolygon hole)
        {
            if (Polygons.Contains(hole))
                Polygons.Remove(hole);
        }

        public JSONNode GetSemanticObject(CitySurface[] allGeometrySurfaces)
        {
            var node = new JSONObject();
            node["type"] = SurfaceType.ToString();
            //node["name"] = name;

            if (semanticParent != null)
                node["parent"] = GetParentIndex(allGeometrySurfaces);

            if (semanticChildren.Count > 0)
            {
                var childrenNode = new JSONArray();
                var childIndices = GetChildIndices(allGeometrySurfaces);
                foreach (var c in childIndices)
                {
                    childrenNode.Add(c);
                }
                node["children"] = childrenNode;
            }
            return node;
        }

        public void SetParent(CitySurface newParent)
        {
            if (semanticParent != null)
                semanticParent.RemoveChild(this);

            semanticParent = newParent;

            if (semanticParent != null)
                newParent.AddChild(this);
        }

        private void AddChild(CitySurface child)
        {
            Assert.IsFalse(semanticChildren.Contains(child));
            semanticChildren.Add(child);
        }

        private void RemoveChild(CitySurface child)
        {
            semanticChildren.Remove(child);
        }

        private int GetParentIndex(CitySurface[] surfaces)
        {
            return Array.IndexOf(surfaces, semanticParent);
        }

        private int[] GetChildIndices(CitySurface[] surfaces)
        {
            var array = new int[semanticChildren.Count];
            for (int i = 0; i < semanticChildren.Count; i++)
            {
                array[i] = Array.IndexOf(surfaces, semanticChildren[i]);
            }
            return array;
        }
    }

    public class CityMultiOrCompositeSurface : CityBoundary
    {
        public List<CitySurface> Surfaces = new List<CitySurface>() { new CitySurface() };
        public override int VertexCount
        {
            get
            {
                int count = 0;
                foreach (var surface in Surfaces)
                    foreach (var polygon in surface.Polygons)
                        count += polygon.Count;
                return count;
            }
        }

        public override void FromJSONNode(JSONArray boundariesNode, List<Vector3Double> combinedVertices)
        {
            Surfaces = new List<CitySurface>();
            foreach (var surfaceNode in boundariesNode)
            {
                var surface = new CitySurface();
                surface.FromJSONNode(surfaceNode.Value.AsArray, combinedVertices);
                Surfaces.Add(surface);
            }
        }

        public override JSONArray GetBoundaries(int indexOffset)
        {
            var boundariesNode = new JSONArray();
            foreach (var surface in Surfaces)
            {
                var surfaceNode = surface.GetBoundaries(indexOffset);
                indexOffset += surface.VertexCount;
                boundariesNode.Add(surfaceNode);
            }
            return boundariesNode;
        }

        public override List<Vector3Double> GetVertices()
        {
            var vertices = new List<Vector3Double>();
            foreach (var surface in Surfaces)
            {
                vertices = vertices.Concat(surface.GetVertices()).ToList();
            }
            return vertices;
        }
    }

    //public class CityCompositeSurface : CityBoundary
    //{
    //    public List<CitySurface> Surfaces;
    //    public override JSONArray GetBoundaries()
    //    {
    //        var boundariesNode = new JSONArray();
    //        foreach (var surface in Surfaces)
    //        {
    //            var surfaceNode = new JSONArray();
    //            surfaceNode.Add(surface.SolidSurfacePolygon.GetJSONPolygon(false));
    //            foreach (var hole in surface.HolePolygons)
    //            {
    //                surfaceNode.Add(hole.GetJSONPolygon(true));
    //            }
    //            boundariesNode.Add(surfaceNode);
    //        }
    //        return boundariesNode;
    //    }
    //}

    public class CitySolid : CityBoundary
    {
        public List<CityMultiOrCompositeSurface> Shells = new List<CityMultiOrCompositeSurface>() { new CityMultiOrCompositeSurface() };
        public override int VertexCount
        {
            get
            {
                int count = 0;
                foreach (var shell in Shells)
                    count += shell.VertexCount;
                return count;
            }
        }

        public override void FromJSONNode(JSONArray boundariesNode, List<Vector3Double> combinedVertices)
        {
            Shells = new List<CityMultiOrCompositeSurface>();
            foreach (var shellNode in boundariesNode)
            {
                var multiSurface = new CityMultiOrCompositeSurface();
                multiSurface.FromJSONNode(shellNode.Value.AsArray, combinedVertices);
                Shells.Add(multiSurface);
            }
        }

        public override JSONArray GetBoundaries(int indexOffset)
        {
            var boundariesNode = new JSONArray();
            foreach (var shell in Shells)
            {
                boundariesNode.Add(shell.GetBoundaries(indexOffset));
                indexOffset += shell.VertexCount;
            }
            return boundariesNode;
        }

        public override List<Vector3Double> GetVertices()
        {
            var vertices = new List<Vector3Double>();
            foreach (var shell in Shells)
            {
                vertices = vertices.Concat(shell.GetVertices()).ToList();
            }
            return vertices;
        }
    }

    public class CityMultiOrCompositSolid : CityBoundary
    {
        public List<CitySolid> Solids = new List<CitySolid>() { new CitySolid() };
        public override int VertexCount
        {
            get
            {
                int count = 0;
                foreach (var solid in Solids)
                    count += solid.VertexCount;
                return count;
            }
        }

        public override void FromJSONNode(JSONArray boundariesNode, List<Vector3Double> combinedVertices)
        {
            Solids = new List<CitySolid>();
            foreach (var solidNode in boundariesNode)
            {
                var solid = new CitySolid();
                solid.FromJSONNode(solidNode.Value.AsArray, combinedVertices);
                Solids.Add(solid);
            }
        }

        public override JSONArray GetBoundaries(int indexOffset)
        {
            var boundariesNode = new JSONArray();
            foreach (var solid in Solids)
            {
                boundariesNode.Add(solid.GetBoundaries(indexOffset));
                indexOffset += solid.VertexCount;
            }
            return boundariesNode;
        }

        public override List<Vector3Double> GetVertices()
        {
            var vertices = new List<Vector3Double>();
            foreach (var solid in Solids)
            {
                vertices = vertices.Concat(solid.GetVertices()).ToList();
            }
            return vertices;
        }
    }

    //public class CityCompositeSolid : CityBoundary
    //{
    //    public List<CitySolid> Solids;
    //    public override JSONArray GetBoundaries()
    //    {
    //        var boundariesNode = new JSONArray();
    //        foreach (var solid in Solids)
    //        {
    //            boundariesNode.Add(solid.GetBoundaries());
    //        }
    //        return boundariesNode;
    //    }
    //}
}
