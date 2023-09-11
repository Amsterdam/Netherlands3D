using System;
using System.Collections.Generic;
using System.Linq;
using Netherlands3D.Coordinates;
using Netherlands3D.Events;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    public class BoundaryMesh
    {
        public Mesh Mesh;
        public CityGeometrySemanticsObject SemanticsObject;

        public BoundaryMesh(Mesh mesh, CityGeometrySemanticsObject semanticsObject)
        {
            Mesh = mesh;
            SemanticsObject = semanticsObject;
        }
    }

    public class MeshWithMaterials
    {
        public Mesh Mesh;
        public Material[] Materials;

        public MeshWithMaterials(Mesh mesh, Material[] materials)
        {
            Mesh = mesh;
            Materials = materials;
        }
    }

    [Serializable]
    public class SemanticMaterials
    {
        public SurfaceSemanticType Type;
        public Material Material;
    }

    /// <summary>
    /// This class visualizes a CityObject by creating a mesh for each LOD geometry.
    /// </summary>
    [RequireComponent(typeof(CityObject))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CityObjectVisualizer : MonoBehaviour
    {
        private CityObject cityObject;
        private Dictionary<CityGeometry, MeshWithMaterials> meshes;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        [SerializeField]
        private int activeLOD;
        public int ActiveLod => activeLOD;
        public Mesh ActiveMesh { get; private set; }

        [SerializeField]
        private GameObjectEvent jsonVisualized;
        [SerializeField]
        private SemanticMaterials[] materials;

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
            meshRenderer = GetComponent<MeshRenderer>();
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
            transform.localPosition = SetPosition(cityObject); //set position first so the CityObject's transformationMatrix can be used to position the mesh.
            meshes = CreateMeshes(cityObject);
            var highestLod = meshes.Keys.Max(g => g.Lod);
            SetLODActive(highestLod);
            jsonVisualized.InvokeStarted(gameObject);
        }

        private Vector3 SetPosition(CityObject cityObject)
        {
            var center = cityObject.AbsoluteCenter; 
            var coordinate = new Coordinate(cityObject.CoordinateSystem, center.x, center.y, center.z);
            return CoordinateConverter.ConvertTo(coordinate, CoordinateSystem.Unity).ToVector3();
        }


        //enable the mesh of a certain LOD
        public bool SetLODActive(int lod)
        {
            activeLOD = lod;

            var geometry = meshes.Keys.FirstOrDefault(g => g.Lod == lod);
            if (geometry != null)
            {
                SetMesh(meshes[geometry]);
                return true;
            }
            SetMesh(null);
            return false;
        }

        private void SetMesh(MeshWithMaterials mesh)
        {
            if (mesh != null)
            {
                ActiveMesh = mesh.Mesh;
                meshRenderer.materials = mesh.Materials;
            }
            else
            {
                ActiveMesh = null;
            }

            meshFilter.mesh = ActiveMesh;

            if (meshCollider)
                meshCollider.sharedMesh = ActiveMesh;
        }

        private Material GetMaterial(SurfaceSemanticType type)
        {
            var mat = materials.FirstOrDefault(m => m.Type == type);
            if (mat != null)
                return mat.Material;

            mat = materials.FirstOrDefault(m => m.Type == SurfaceSemanticType.Null);
            if (mat != null)
                return mat.Material;

            return null;
        }

        //create the meshes for the object geometries
        private Dictionary<CityGeometry, MeshWithMaterials> CreateMeshes(CityObject cityObject)
        {
            meshes = new Dictionary<CityGeometry, MeshWithMaterials>();
            foreach (var geometry in cityObject.Geometries)
            {
                var mesh = CreateMeshFromGeometry(geometry, cityObject.CoordinateSystem, cityObject.transform.worldToLocalMatrix);
                meshes.Add(geometry, mesh);
            }
            return meshes;
        }

        public MeshWithMaterials CreateMeshFromGeometry(CityGeometry geometry, CoordinateSystem coordinateSystem, Matrix4x4 transformationMatrix)
        {
            var boundaryMeshes = BoundariesToMeshes(geometry.BoundaryObject, coordinateSystem);
            var subMeshes = CombineBoundaryMeshesWithTheSameSemanticObject(boundaryMeshes, transformationMatrix, out var types);
            var materials = new Material[types.Count];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = GetMaterial(types[i]);
            }
            var mesh = CombineMeshes(subMeshes, Matrix4x4.identity, false); //use identity matrix because we already transformed the submeshes
            return new MeshWithMaterials(mesh, materials);
        }

        public static List<Mesh> CombineBoundaryMeshesWithTheSameSemanticObject(List<BoundaryMesh> boundaryMeshes, Matrix4x4 transformationMatrix, out List<SurfaceSemanticType> types)
        {
            List<Mesh> combinedMeshes = new List<Mesh>();
            types = new List<SurfaceSemanticType>();
            while (boundaryMeshes.Count > 0)
            {
                List<Mesh> meshesToCombine = new List<Mesh>();
                CityGeometrySemanticsObject activeSemanticsObject = boundaryMeshes[boundaryMeshes.Count - 1].SemanticsObject;
                for (int i = boundaryMeshes.Count - 1; i >= 0; i--) //go backwards because collection will be modified
                {
                    var boundaryMesh = boundaryMeshes[i];
                    if (boundaryMesh.SemanticsObject == activeSemanticsObject)
                    {
                        meshesToCombine.Add(boundaryMesh.Mesh);
                        boundaryMeshes.Remove(boundaryMesh);
                    }
                }
                var combinedMesh = CombineMeshes(meshesToCombine, transformationMatrix, true);
                combinedMeshes.Add(combinedMesh);
                if (activeSemanticsObject != null)
                    types.Add(activeSemanticsObject.SurfaceType);
                else
                    types.Add(SurfaceSemanticType.Null);
            }
            return combinedMeshes;
        }

        public static Mesh CombineMeshes(List<Mesh> meshes, Matrix4x4 transformationMatrix, bool mergeSubMeshes)
        {
            CombineInstance[] combine = new CombineInstance[meshes.Count];

            for (int i = 0; i < meshes.Count; i++)
            {
                combine[i].mesh = meshes[i];
                combine[i].transform = transformationMatrix;
            }

            var mesh = new Mesh();
            mesh.CombineMeshes(combine, mergeSubMeshes);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        //Different boundary objects need to be parsed into meshes in different ways because of the different depths of the boundary arrays. We need to go as deep as needed to create meshes from surfaces.
        protected virtual List<BoundaryMesh> BoundariesToMeshes(CityBoundary boundary, CoordinateSystem coordinateSystem)
        {
            if (boundary is CityMultiPoint)
                throw new NotSupportedException("Boundary of type " + typeof(CityMultiPoint) + "is not supported by this Visualiser script since it contains no mesh data. Use MultiPointVisualiser instead and assign an object to use as visualization of the points");
            if (boundary is CityMultiLineString) //todo this boundary type is not supported at all
                throw new NotSupportedException("Boundary of type " + typeof(CityMultiLineString) + "is currently not supported.");
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

        private static List<BoundaryMesh> BoundariesToMeshes(CitySurface boundary, CoordinateSystem coordinateSystem)
        {
            var meshes = new List<BoundaryMesh>();
            var mesh = CitySurfaceToMesh(boundary, coordinateSystem);
            meshes.Add(mesh);
            return meshes;
        }

        private static List<BoundaryMesh> BoundariesToMeshes(CityMultiOrCompositeSurface boundary, CoordinateSystem coordinateSystem)
        {
            var meshes = new List<BoundaryMesh>();
            foreach (var surface in boundary.Surfaces)
            {
                var mesh = CitySurfaceToMesh(surface, coordinateSystem);
                meshes.Add(mesh);
            }
            return meshes;
        }

        private static List<BoundaryMesh> BoundariesToMeshes(CitySolid boundary, CoordinateSystem coordinateSystem)
        {
            var meshes = new List<BoundaryMesh>();
            foreach (var shell in boundary.Shells)
            {
                var shellMeshes = BoundariesToMeshes(shell, coordinateSystem);
                meshes = meshes.Concat(shellMeshes).ToList();
            }
            return meshes;
        }

        private static List<BoundaryMesh> BoundariesToMeshes(CityMultiOrCompositeSolid boundary, CoordinateSystem coordinateSystem)
        {
            var meshes = new List<BoundaryMesh>();
            foreach (var solid in boundary.Solids)
            {
                var solidMeshes = BoundariesToMeshes(solid, coordinateSystem);
                meshes = meshes.Concat(solidMeshes).ToList();
            }
            return meshes;
        }

        //create a mesh of a surface.
        private static BoundaryMesh CitySurfaceToMesh(CitySurface surface, CoordinateSystem coordinateSystem)
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

            var mesh = Poly2Mesh.CreateMesh(polygon);
            var semanticsObject = surface.SemanticsObject;
            return new BoundaryMesh(mesh, semanticsObject);
        }

        // convert the list of Vector3Doubles to a list of Vector3s and convert the coordinates to unity in the process.
        public static List<Vector3> GetConvertedPolygonVertices(CityPolygon polygon, CoordinateSystem coordinateSystem)
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
                        convertedVert = CoordinateConverter.WGS84toUnity(wgs);
                        break;
                    case CoordinateSystem.RD:
                        var rd = new Vector3RD(relativeVert.x, relativeVert.y, relativeVert.z);
                        convertedVert = CoordinateConverter.RDtoUnity(rd);
                        break;
                    default:
                        convertedVert = new Vector3((float)relativeVert.x, (float)relativeVert.z, (float)relativeVert.y);
                        break;
                }

                convertedPolygon.Add(convertedVert);
            }

            convertedPolygon.Reverse();
            return convertedPolygon;
        }
    }
}
