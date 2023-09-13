using System.Collections.Generic;
using Netherlands3D.Coordinates;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshToCityObject : CityObject
    {
        [SerializeField]
        private string id = "CustomMeshObject";
        [SerializeField]
        private CityObjectType type = CityObjectType.Building;
        [SerializeField]
        private CoordinateSystem coordinateSystem = CoordinateSystem.Unity;
        [SerializeField]
        private int lod;

        private MeshFilter meshFilter;

#if UNITY_EDITOR
        private void OnValidate()
        {
            Id = id;
            Type = type;
            CoordinateSystem = coordinateSystem;

            meshFilter = GetComponent<MeshFilter>();
            CreateGeometryFromMesh(meshFilter.sharedMesh);
        }
#endif
        protected virtual void Awake()
        {
            Id = id;
            Type = type;
            CoordinateSystem = coordinateSystem;

            meshFilter = GetComponent<MeshFilter>();
            CreateGeometryFromMesh(meshFilter.mesh);
            //Attributes = CityObjectAttribute.ParseAttributesNode(this, cityObjectNode["attributes"]);
        }

        public void CreateGeometryFromMesh(Mesh mesh)
        {
            Geometries = new List<CityGeometry>();
            var geometry = CreateMultiSurfaceGeometryFromMesh(mesh, lod);
            Geometries.Add(geometry);
            RecalculateExtents();
        }

        public static CityGeometry CreateMultiSurfaceGeometryFromMesh(Mesh mesh, int lod)
        {
            var type = GeometryType.MultiSurface;

            var includeSemantics = false; //todo: add semantics support
            var includeMaterials = false; //todo: add material support
            var includeTextures = false; //todo: add texture support

            var geometry = new CityGeometry(type, lod, includeSemantics, includeMaterials, includeTextures);
            geometry.CreateBoundaryObjectFromMesh(mesh);
            //if (includeSemantics)
            //    CityGeometrySemantics.FromJSONNode(semanticsNode, geometry.BoundaryObject);

            return geometry;
        }
    }
}
