using System;
using System.Collections.Generic;
using Netherlands3D.Coordinates;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    public class MultiPointVisualizer : CityObjectVisualizer
    {
        [SerializeField]
        private GameObject visualizationObject;

        protected override List<BoundaryMesh> BoundariesToMeshes(CityBoundary boundary, CoordinateSystem coordinateSystem)
        {
            if (!(boundary is CityMultiPoint))
                throw new NotSupportedException("Boundary is not of Type MultiPoint, use CityObjectVisualiser instead.");

            return PointsToMeshes(boundary as CityMultiPoint, coordinateSystem, visualizationObject);
        }

        private List<BoundaryMesh> PointsToMeshes(CityMultiPoint boundary, CoordinateSystem coordinateSystem, GameObject visualizationObject)
        {
            var meshes = new List<BoundaryMesh>();
            var verts = GetConvertedPolygonVertices(boundary.Points, coordinateSystem);
            for (int i = 0; i < boundary.VertexCount; i++)
            {
                CityGeometrySemanticsObject semantics = null;
                if (boundary.SemanticsObjects.Count > 0)
                    semantics = boundary.SemanticsObjects[i];

                var mesh = InstantiateObjectAtPoint(verts[i], coordinateSystem, visualizationObject);
                meshes.Add(new BoundaryMesh(mesh, semantics));
            }
            return meshes;
        }

        private Mesh InstantiateObjectAtPoint(Vector3 point, CoordinateSystem coordinateSystem, GameObject visualizationObject)
        {
            var obj = Instantiate(visualizationObject, point, Quaternion.identity, transform);
            var meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter)
                return meshFilter.mesh;

            return null;
        }
    }
}
