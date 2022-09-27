using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.ProfileRendering
{
    public class MeshesProfileCutter : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private TriggerEvent onCutMeshes;
        [SerializeField] private Vector3ListEvent onReceiveCuttingLine;
        [Header("Output")]
        [SerializeField] private FloatEvent outputProgress;
        [SerializeField] private Vector3ListEvent outputProfilePolyline;

        private List<Vector3> cuttingLine;

        [Header("Settings")]
        [SerializeField, Tooltip("The meshes on these layers will be cut, and included in the profile")]
        private LayerMask layerMask;
        [SerializeField] private bool drawInEditorGizmos = true;
        [SerializeField] private int maxTrianglesPerFrame = 1000;

        private Plane cuttingPlane;
        private Bounds lineBounds;
        private Vector3 lineCenter;

        private List<Vector3> triangleIntersectionLine = new List<Vector3>();
        private List<Vector3> profileLines = new List<Vector3>();

        void Awake()
        {
            onReceiveCuttingLine.started.AddListener(SetLine);
            onCutMeshes.started.AddListener(CutMeshes);

            triangleIntersectionLine.Capacity = 2;
        }

        private void SetLine(List<Vector3> cuttingLine)
        {
            this.cuttingLine = cuttingLine;
            lineBounds = GetBoundsFromLine(cuttingLine);

            var planeNormal = Vector3.Cross((this.cuttingLine[0] - this.cuttingLine[1]).normalized, Vector3.up);
            cuttingPlane = new Plane(planeNormal, lineCenter);
        }

        private Bounds GetBoundsFromLine(List<Vector3> line)
        {
            lineCenter = Vector3.Lerp(line[0], line[1], 0.5f);
            var width = Mathf.Abs(line[0].x - line[1].x);
            var depth = Mathf.Abs(line[0].z - line[1].z);

            return new Bounds(lineCenter, new Vector3(width, 300, depth));
        }

        private void CutMeshes()
        {
            if (cuttingLine == null || cuttingLine.Count < 2)
            {
                Debug.Log("Cant cut meshes. Cutting line not set.", this.gameObject);
                return;
            }

            var allMeshFilters = FindObjectsOfType<MeshFilter>();
            StartCoroutine(CutLoop(allMeshFilters));
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawInEditorGizmos) return;

            if (lineBounds != null)
            {
                Gizmos.color = new Color(1, 1, 0, 0.5f);
                Gizmos.DrawCube(lineBounds.center, lineBounds.size);
            }

            Gizmos.color = Color.green;
            if (profileLines.Count > 1)
            {
                for (int i = 0; i < profileLines.Count; i++)
                {
                    Gizmos.DrawSphere(profileLines[i], 0.5f);
                }

                for (int i = 0; i < profileLines.Count; i += 2)
                {
                    Gizmos.DrawLine(profileLines[i], profileLines[i + 1]);
                }
            }
        }

        private IEnumerator CutLoop(MeshFilter[] meshFilters)
        {
            profileLines.Clear();
            List<Vector3> meshProfile = new List<Vector3>();

            if (outputProgress) outputProgress.Invoke(0.001f);
            yield return new WaitForEndOfFrame();

            for (int i = 0; i < meshFilters.Length; i++)
            {
                var meshFilter = meshFilters[i];
                if (outputProgress && i > 0) outputProgress.Invoke((float)i / meshFilters.Length);

                if (meshFilter && IsInLayerMask(meshFilter.gameObject.layer))
                {
                    var renderer = meshFilter.GetComponent<Renderer>();
                    if (renderer != null && renderer.bounds.Intersects(lineBounds))
                    {
                        Debug.Log($"Generating profile for {meshFilter.gameObject.name}", meshFilter.gameObject);
                        meshProfile.Clear();
                        yield return GetMeshProfile(meshFilter, meshProfile);

                        profileLines.AddRange(meshProfile);
                    }
                }
            }

            if (outputProgress) outputProgress.Invoke(0.001f);
            outputProfilePolyline.Invoke(profileLines);

            yield return null;
        }

        private IEnumerator GetMeshProfile(MeshFilter meshFilter, List<Vector3> meshProfile)
        {
            var targetMesh = meshFilter.sharedMesh;

            var vertices = targetMesh.vertices;
            var triangles = targetMesh.triangles;
            Matrix4x4 localToWorld = meshFilter.transform.localToWorldMatrix;

            //For each triangle, add the intersection line to our list
            for (int i = 0; i < triangles.Length; i += 3)
            {
                //Render a frame sometimes
                if ((i % maxTrianglesPerFrame) == 0) yield return new WaitForEndOfFrame();

                triangleIntersectionLine.Clear();

                //Get all vertex world positions ( so we support transformed objects )  
                var pointAWorld = localToWorld.MultiplyPoint3x4(vertices[triangles[i]]);
                var pointBWorld = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 1]]);
                var pointCWorld = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 2]]);

                Vector3 intersection;
                if (LineCrossingCuttingPlane(pointAWorld, pointBWorld, out intersection))
                {
                    triangleIntersectionLine.Add(intersection);
                }
                if (LineCrossingCuttingPlane(pointBWorld, pointCWorld, out intersection))
                {
                    triangleIntersectionLine.Add(intersection);
                    if (triangleIntersectionLine.Count == 2)
                    {
                        if (GetBoundsFromLine(triangleIntersectionLine).Intersects(lineBounds))
                        {
                            meshProfile.AddRange(triangleIntersectionLine);
                        }
                        continue; //We have our line, next triangle!
                    }
                }
                if (LineCrossingCuttingPlane(pointCWorld, pointAWorld, out intersection))
                {
                    triangleIntersectionLine.Add(intersection);
                    if (triangleIntersectionLine.Count == 2)
                    {
                        if (GetBoundsFromLine(triangleIntersectionLine).Intersects(lineBounds))
                        {
                            meshProfile.AddRange(triangleIntersectionLine);
                        }
                        continue; //We have our line, next triangle!
                    }
                }
            }
        }

        private bool LineCrossingCuttingPlane(Vector3 pointAWorld, Vector3 pointBWorld, out Vector3 intersection)
        {
            if (cuttingPlane.GetSide(pointAWorld) != cuttingPlane.GetSide(pointBWorld))
            {
                intersection = PointOnPlaneBetweenTwoPoints(cuttingPlane, pointAWorld, pointBWorld);
                return true;
            }
            intersection = Vector3.zero;
            return false;
        }

        private Vector3 PointOnPlaneBetweenTwoPoints(Plane plane, Vector3 a, Vector3 b)
        {
            var q = (b - a);
            q.Normalize();
            Vector3 planeEqation;
            var pointOnPlane = plane.ClosestPointOnPlane(Vector3.zero);
            var normal = plane.normal;
            planeEqation = normal;
            var offset = Vector3.Dot(pointOnPlane, normal);
            var t = (offset - Vector3.Dot(a, planeEqation)) / Vector3.Dot(q, planeEqation);

            return a + (q * t);
        }

        public bool IsInLayerMask(int layer)
        {
            return layerMask == (layerMask | (1 << layer));
        }
    }
}