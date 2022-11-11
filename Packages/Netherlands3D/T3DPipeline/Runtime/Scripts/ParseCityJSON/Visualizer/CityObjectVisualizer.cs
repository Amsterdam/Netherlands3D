using System;
using System.Collections.Generic;
using System.Linq;
using Netherlands3D.Core;
using Netherlands3D.Events;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// This class visualizes a CityObject by creating a mesh for each LOD geometry.
    /// </summary>
    [RequireComponent(typeof(CityObject))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CityObjectVisualizer : MonoBehaviour
    {
        private CityObject cityObject;
        private Dictionary<CityGeometry, Mesh> meshes;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        [SerializeField]
        private int activeLOD;
        public int ActiveLod => activeLOD;

        [SerializeField]
        private GameObjectEvent jsonVisualized;

#if UNITY_EDITOR
        // allow to change the visible LOD from the inspector during runtime
        private void OnValidate()
        {
            if (meshes != null)
                SetLODActive(activeLOD);
        }
#endif
        private void Awake()
        {
            cityObject = GetComponent<CityObject>();
            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();
        }

        private void OnEnable()
        {
            cityObject.CityObjectParsed.AddListener(Visualize);
        }

        private void OnDisable()
        {
            cityObject.CityObjectParsed.RemoveAllListeners();
        }

        //create the meshes
        private void Visualize()
        {
            meshes = CreateMeshes(cityObject);
            var highestLod = meshes.Keys.Max(g => g.Lod);
            SetLODActive(highestLod);
            transform.position = SetPosition(cityObject);
            jsonVisualized.Invoke(gameObject);
        }

        private Vector3 SetPosition(CityObject cityObject)
        {
            var center = cityObject.AbsoluteCenter;
            switch (cityObject.CoordinateSystem)
            {
                case CoordinateSystem.WGS84:
                    var wgs = new Vector3WGS(center.x, center.y, center.z);
                    return CoordConvert.WGS84toUnity(wgs);
                case CoordinateSystem.RD:
                    var rd = new Vector3RD(center.x, center.y, center.z);
                    return CoordConvert.RDtoUnity(rd);
            }
            return new Vector3((float)center.x, (float)center.y, (float)center.z);
        }

        //enable the mesh of a certain LOD
        public bool SetLODActive(int lod)
        {
            var geometry = meshes.Keys.FirstOrDefault(g => g.Lod == lod);
            if (geometry != null)
            {
                var activeMesh = meshes[geometry];
                meshFilter.mesh = activeMesh;
                activeLOD = lod;

                if (meshCollider)
                    meshCollider.sharedMesh = activeMesh;

                return true;
            }
            return false;
        }

        //create the meshes for the object geometries
        private Dictionary<CityGeometry, Mesh> CreateMeshes(CityObject cityObject)
        {
            meshes = new Dictionary<CityGeometry, Mesh>();
            foreach (var geometry in cityObject.Geometries)
            {
                var mesh = CreateMeshFromGeometry(geometry, cityObject.CoordinateSystem, transform.localToWorldMatrix);
                meshes.Add(geometry, mesh);
            }
            return meshes;
        }

        public static Mesh CreateMeshFromGeometry(CityGeometry geometry, CoordinateSystem coordinateSystem, Matrix4x4 localToWorldMatrix)
        {
            var boundaryMeshes = BoundariesToMeshes(geometry.BoundaryObject, coordinateSystem);
            return CombineMeshes(boundaryMeshes, localToWorldMatrix);
        }

        public static Mesh CombineMeshes(List<Mesh> meshes, Matrix4x4 localToWorldMatrix)
        {
            CombineInstance[] combine = new CombineInstance[meshes.Count];

            for (int i = 0; i < meshes.Count; i++)
            {
                combine[i].mesh = meshes[i];
                combine[i].transform = localToWorldMatrix;
            }

            var mesh = new Mesh();
            mesh.CombineMeshes(combine);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        //Different boundary objects need to be parsed into meshes in different ways because of the different depths of the boundary arrays. We need to go as deep as needed to create meshes from surfaces.
        private static List<Mesh> BoundariesToMeshes(CityBoundary boundary, CoordinateSystem coordinateSystem)
        {
            if (boundary is CityMultiPoint || boundary is CityMultiLineString) //todo these boundary types are not supported as meshes
                return new List<Mesh>();
            if (boundary is CitySurface)
                return BoundariesToMeshes(boundary as CitySurface, coordinateSystem);
            if (boundary is CityMultiOrCompositeSurface)
                return BoundariesToMeshes(boundary as CityMultiOrCompositeSurface, coordinateSystem);
            if (boundary is CitySolid)
                return BoundariesToMeshes(boundary as CitySolid, coordinateSystem);
            if (boundary is CityMultiOrCompositeSolid)
                return BoundariesToMeshes(boundary as CityMultiOrCompositeSolid, coordinateSystem);

            throw new ArgumentException("Unknown boundary type: " + boundary.GetType() + " is not supported to convert to mesh");
        }

        private static List<Mesh> BoundariesToMeshes(CitySurface boundary, CoordinateSystem coordinateSystem)
        {
            var meshes = new List<Mesh>();
            var mesh = CitySurfaceToMesh(boundary, coordinateSystem);
            meshes.Add(mesh);
            return meshes;
        }

        private static List<Mesh> BoundariesToMeshes(CityMultiOrCompositeSurface boundary, CoordinateSystem coordinateSystem)
        {
            var meshes = new List<Mesh>();
            foreach (var surface in boundary.Surfaces)
            {
                var mesh = CitySurfaceToMesh(surface, coordinateSystem);
                meshes.Add(mesh);
            }
            return meshes;
        }

        private static List<Mesh> BoundariesToMeshes(CitySolid boundary, CoordinateSystem coordinateSystem)
        {
            var meshes = new List<Mesh>();
            foreach (var shell in boundary.Shells)
            {
                var shellMeshes = BoundariesToMeshes(shell, coordinateSystem);
                meshes = meshes.Concat(shellMeshes).ToList();
            }
            return meshes;
        }

        private static List<Mesh> BoundariesToMeshes(CityMultiOrCompositeSolid boundary, CoordinateSystem coordinateSystem)
        {
            var meshes = new List<Mesh>();
            foreach (var solid in boundary.Solids)
            {
                var solidMeshes = BoundariesToMeshes(solid, coordinateSystem);
                meshes = meshes.Concat(solidMeshes).ToList();
            }
            return meshes;
        }

        //create a mesh of a surface.
        private static Mesh CitySurfaceToMesh(CitySurface surface, CoordinateSystem coordinateSystem)
        {
            if (surface.VertexCount == 0)
                return null;

            List<Vector3> solidSurfacePolygon = GetConvertedPolygonVertices(surface.SolidSurfacePolygon, coordinateSystem);
            List<List<Vector3>> holePolygons = new List<List<Vector3>>();
            foreach (var hole in surface.HolePolygons)
            {
                holePolygons.Add(GetConvertedPolygonVertices(hole, coordinateSystem));
            }

            Poly2Mesh.Polygon polygon = new Poly2Mesh.Polygon();
            polygon.outside = solidSurfacePolygon;
            polygon.holes = holePolygons;

            return Poly2Mesh.CreateMesh(polygon);
        }

        // convert the list of Vector3Doubles to a list of Vector3s and convert the coordinates to unity in the process.
        private static List<Vector3> GetConvertedPolygonVertices(CityPolygon polygon, CoordinateSystem coordinateSystem)
        {
            List<Vector3> convertedPolygon = new List<Vector3>();
            foreach (var vert in polygon.Vertices)
            {
                var relativeVert = vert;
                Vector3 convertedVert;
                switch (coordinateSystem)
                {
                    case CoordinateSystem.WGS84:
                        var wgs = new Vector3WGS(relativeVert.x, relativeVert.y, relativeVert.z);
                        convertedVert = CoordConvert.WGS84toUnity(wgs);
                        break;
                    case CoordinateSystem.RD:
                        var rd = new Vector3RD(relativeVert.x, relativeVert.y, relativeVert.z);
                        convertedVert = CoordConvert.RDtoUnity(rd);
                        break;
                    default:
                        convertedVert = new Vector3((float)relativeVert.x, (float)relativeVert.y, (float)relativeVert.z);
                        break;
                }

                convertedPolygon.Add(convertedVert);
            }

            convertedPolygon.Reverse();
            return convertedPolygon;
        }
    }
}
