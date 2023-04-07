using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// from https://www.cityjson.org/specs/1.0.3/#geometry-objects:
    /// A geometry object must have one member with the name "boundaries", whose value is a hierarchy of arrays (the depth depends on the Geometry object) with integers. An integer refers to the index in the "vertices" array of the CityJSON object, and it is 0-based (ie the first element in the array has the index "0", the second one "1", etc.).
    /// The abstract class CityBoundray is used to represent the boundaries object, and its derivatives are used to represent the boundary objects of the different types.
    /// 
    /// To make editing the geometry easier in the application, the vertices are stored per polygon in Unity. They are separated from the single Vertices list when creating a boundary object from a JSONNode and they are recombined when exporting the CityJSON
    /// </summary>
    public abstract class CityBoundary
    {
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
        public abstract void FromJSONNode(JSONArray boundariesNode, List<Vector3Double> combinedVertices); //pass the complete list of vertices, the needed vertices will be saved per boundary object.
        public abstract JSONArray GetBoundariesAndAddNewVertices(Dictionary<Vector3Double, int> currentCityJSONVertices); // pass a dictionary of the already processed vertices for the CityJSON to recombine them into a single List without duplicates
        public abstract List<Vector3Double> GetUncombinedVertices(); // returns a list of all vertices used by the boundary object without filtering for duplicates
    }

    public class CityMultiPoint : CityBoundary
    {
        public CityPolygon Points { get; set; } = new CityPolygon(); // Use a Polygon to represent the boundary array, because it behaves the same, even though it is not technically a polygon.
        public List<CityGeometrySemanticsObject> SemanticsObjects { get; set; } = new List<CityGeometrySemanticsObject>();

        public override int VertexCount => Points.Count;
        public override void FromJSONNode(JSONArray boundariesNode, List<Vector3Double> combinedVertices)
        {
            Points = CityPolygon.FomJsonNode(boundariesNode, combinedVertices);
        }
        public override JSONArray GetBoundariesAndAddNewVertices(Dictionary<Vector3Double, int> currentCityJSONVertices)
        {
            return Points.GetJSONPolygonAndAddNewVertices(false, currentCityJSONVertices);
        }
        public override List<Vector3Double> GetUncombinedVertices()
        {
            return Points.Vertices.ToList();
        }
    }

    public class CityMultiLineString : CityBoundary
    {
        public List<CityPolygon> LineStrings { get; set; } = new List<CityPolygon>() { new CityPolygon() };// Use a List of Polygon to represent the boundary array, because it behaves the same, even though it is not technically a List of polygon.
        public List<CityGeometrySemanticsObject> SemanticsObjects { get; set; } = new List<CityGeometrySemanticsObject>();
        public override int VertexCount
        {
            get
            {
                int count = 0;
                foreach (var lineString in LineStrings)
                    count += lineString.Count;
                return count;
            }
        }

        public override void FromJSONNode(JSONArray boundariesNode, List<Vector3Double> combinedVertices)
        {
            LineStrings = new List<CityPolygon>();
            foreach (var lineStringNode in boundariesNode)
            {
                var polygon = CityPolygon.FomJsonNode(lineStringNode.Value.AsArray, combinedVertices);
                LineStrings.Add(polygon);
            }
        }

        public override JSONArray GetBoundariesAndAddNewVertices(Dictionary<Vector3Double, int> currentCityJSONVertices)
        {
            var node = new JSONArray();
            foreach (var polygon in LineStrings)
            {
                var polygonNode = polygon.GetJSONPolygonAndAddNewVertices(false, currentCityJSONVertices);
                node.Add(polygonNode);
            }
            return node;
        }
        public override List<Vector3Double> GetUncombinedVertices()
        {
            var vertices = new List<Vector3Double>();
            foreach (var polygon in LineStrings)
            {
                vertices = vertices.Concat(polygon.Vertices).ToList();
            }
            return vertices;
        }
    }

    public class CitySurface : CityBoundary
    {
        public List<CityPolygon> Polygons { get; set; } = new List<CityPolygon>() { new CityPolygon() }; //add an empty polygon as the SolidSurfacePolygon
        public CityPolygon SolidSurfacePolygon => Polygons[0]; //the first polygon is solid, with all other polygons being holes in the first polygon
        public CityPolygon[] HolePolygons => Polygons.Skip(1).ToArray();
        public CityGeometrySemanticsObject SemanticsObject { get; set; }

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

        public override JSONArray GetBoundariesAndAddNewVertices(Dictionary<Vector3Double, int> currentCityJSONVertices)
        {
            var surfaceArray = new JSONArray(); //defines the entire surface with holes

            // the following line and loop could be replaced by 1 loop through all the polygons of the surface, but separating them makes it clearer how the structure of the array works

            // add surface
            surfaceArray.Add(SolidSurfacePolygon.GetJSONPolygonAndAddNewVertices(false, currentCityJSONVertices));
            // add holes
            var holes = HolePolygons;
            for (int i = 0; i < holes.Length; i++)
            {
                surfaceArray.Add(holes[i].GetJSONPolygonAndAddNewVertices(true, currentCityJSONVertices));
            }
            return surfaceArray;
        }

        public override List<Vector3Double> GetUncombinedVertices()
        {
            var vertices = new List<Vector3Double>();
            foreach (var polygon in Polygons)
            {
                vertices = vertices.Concat(polygon.Vertices).ToList();
            }
            return vertices;
        }

        public void SetSolidSurfacePolygon(CityPolygon solidSurfacePolygon)
        {
            Polygons[0] = solidSurfacePolygon;
        }

        public bool TryAddHole(CityPolygon hole)
        {
            if (SolidSurfacePolygon == hole)
            {
                Debug.LogError("Cannot add the Solid Surface Polygon as a hole");
                return false;
            }

            if (!Polygons.Contains(hole))
            {
                Polygons.Add(hole);
                return true;
            }
            return false;
        }

        public bool TryRemoveHole(CityPolygon hole)
        {
            if (SolidSurfacePolygon == hole)
            {
                Debug.LogError("Cannot remove the Solid Surface Polygon as a hole, use SetSolidSurfacePolygon() to assign a new SolidSurfacePolygon");
                return false;
            }

            if (Polygons.Contains(hole))
            {
                Polygons.Remove(hole);
                return true;
            }
            return false;
        }
    }

    public class CityMultiOrCompositeSurface : CityBoundary
    {
        public List<CitySurface> Surfaces { get; set; } = new List<CitySurface>() { new CitySurface() }; // from the specs: A "MultiSurface", or a "CompositeSurface", has an array containing surfaces, each surface is modelled by an array of array, the first array being the exterior boundary of the surface, and the others the interior boundaries.
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

        public override JSONArray GetBoundariesAndAddNewVertices(Dictionary<Vector3Double, int> currentCityJSONVertices)
        {
            var boundariesNode = new JSONArray();
            foreach (var surface in Surfaces)
            {
                var surfaceNode = surface.GetBoundariesAndAddNewVertices(currentCityJSONVertices);
                boundariesNode.Add(surfaceNode);
            }
            return boundariesNode;
        }

        public override List<Vector3Double> GetUncombinedVertices()
        {
            var vertices = new List<Vector3Double>();
            foreach (var surface in Surfaces)
            {
                vertices = vertices.Concat(surface.GetUncombinedVertices()).ToList();
            }
            return vertices;
        }

        public void FromMesh(Mesh mesh)
        {
            Surfaces = new List<CitySurface>();
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                var surface = new CitySurface();
                var triIndex1 = mesh.triangles[i];
                var triIndex2 = mesh.triangles[i+1];
                var triIndex3 = mesh.triangles[i+2];
                var polygon = new CityPolygon(new Vector3Double[3] { mesh.vertices[triIndex1], mesh.vertices[triIndex2], mesh.vertices[triIndex3] }, new int[3] { 0, 1, 2 });
                surface.Polygons = new List<CityPolygon>() { polygon }; //the polygon list only contains a solid surface polygon
                Surfaces.Add(surface);
            }
        }
    }

    public class CitySolid : CityBoundary
    {
        public List<CityMultiOrCompositeSurface> Shells { get; set; } = new List<CityMultiOrCompositeSurface>() { new CityMultiOrCompositeSurface() }; //from the specs: A "Solid" has an array of shells, the first array being the exterior shell of the solid, and the others the interior shells. Each shell has an array of surfaces, modelled in the exact same way as a MultiSurface/CompositeSurface.
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

        public override JSONArray GetBoundariesAndAddNewVertices(Dictionary<Vector3Double, int> currentCityJSONVertices)
        {
            var boundariesNode = new JSONArray();
            foreach (var shell in Shells)
            {
                boundariesNode.Add(shell.GetBoundariesAndAddNewVertices(currentCityJSONVertices));
            }
            return boundariesNode;
        }

        public override List<Vector3Double> GetUncombinedVertices()
        {
            var vertices = new List<Vector3Double>();
            foreach (var shell in Shells)
            {
                vertices = vertices.Concat(shell.GetUncombinedVertices()).ToList();
            }
            return vertices;
        }
    }

    public class CityMultiOrCompositeSolid : CityBoundary
    {
        public List<CitySolid> Solids { get; set; } = new List<CitySolid>() { new CitySolid() }; // from the specs: A "MultiSolid", or a "CompositeSolid", has an array containing solids, each solid is modelled as above.
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

        public override JSONArray GetBoundariesAndAddNewVertices(Dictionary<Vector3Double, int> currentCityJSONVertices)
        {
            var boundariesNode = new JSONArray();
            foreach (var solid in Solids)
            {
                boundariesNode.Add(solid.GetBoundariesAndAddNewVertices(currentCityJSONVertices));
            }
            return boundariesNode;
        }

        public override List<Vector3Double> GetUncombinedVertices()
        {
            var vertices = new List<Vector3Double>();
            foreach (var solid in Solids)
            {
                vertices = vertices.Concat(solid.GetUncombinedVertices()).ToList();
            }
            return vertices;
        }
    }
}
