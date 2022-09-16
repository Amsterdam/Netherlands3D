using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshesProfileCutter : MonoBehaviour
{
    [Header("Listen to")]
    [SerializeField] private TriggerEvent onCutMeshes;
    [SerializeField] private Vector3ListEvent onReceiveCuttingLine;

    [SerializeField] private Vector3ListEvent outputProfilePolyline;

    private List<Vector3> cuttingLine;

    [Header("Settings")]
    [SerializeField, Tooltip("The meshes on these layers will be cut, and included in the profile")] 
    private LayerMask layerMask;
    private bool drawInEditorGizmos = true;

    private Plane cuttingPlane;
    private Bounds lineBounds;
    private Vector3 lineCenter;

    private List<Vector3> triangleIntersections = new List<Vector3>();
    [SerializeField] private List<Vector3> profileLines = new List<Vector3>();

    void Awake()
    {
        onReceiveCuttingLine.started.AddListener(SetLine);
        onCutMeshes.started.AddListener(CutMeshes);

        triangleIntersections.Capacity = 2;
    }

    private void SetLine(List<Vector3> cuttingLine)
    {
        this.cuttingLine = cuttingLine;
        lineCenter = Vector3.Lerp(this.cuttingLine[0], this.cuttingLine[1], 0.5f);
        var width = Mathf.Abs(this.cuttingLine[0].x - this.cuttingLine[1].x);
        var depth = Mathf.Abs(this.cuttingLine[0].z - this.cuttingLine[1].z);

        lineBounds = new Bounds(lineCenter, new Vector3(width, 300, depth));

        var planeNormal = Vector3.Cross((this.cuttingLine[0] - this.cuttingLine[1]).normalized, Vector3.up);
        cuttingPlane = new Plane(planeNormal,lineCenter);
    }

    private void CutMeshes()
    {
        if(cuttingLine == null || cuttingLine.Count < 2)
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
        foreach (var meshFilter in meshFilters)
        {
            if (IsInLayerMask(meshFilter.gameObject.layer))
            {
                var renderer = meshFilter.GetComponent<Renderer>();
                if (renderer != null && renderer.bounds.Intersects(lineBounds))
                {
                    Debug.Log($"Generating profile for {meshFilter.gameObject.name}", meshFilter.gameObject);
                    var meshProfile = GetMeshProfile(meshFilter);
                    profileLines.AddRange(meshProfile);

                    /*
                    int[] indices = new int[edgeVertices.Count]; 
                    var lineTopologyMesh = new Mesh();
                    lineTopologyMesh.SetVertices(edgeVertices);
                    lineTopologyMesh.SetIndices(indices, MeshTopology.Lines, 0);
                    return lineTopologyMesh;
                    */
                }
            }
        }
        yield return null;
    }

    private List<Vector3> GetMeshProfile(MeshFilter meshFilter)
    {
        var targetMesh = meshFilter.sharedMesh;

        var vertices = targetMesh.vertices;
        var triangles = targetMesh.triangles;

        //For each triangle, add the intersection line to our list
        List<Vector3> edgeVertices = new List<Vector3>();
        for (int i = 0; i < triangles.Length; i+=3)
        {
            triangleIntersections.Clear();

            //Get all vertex world positions ( so we support transformed objects )
            Matrix4x4 localToWorld = meshFilter.transform.localToWorldMatrix;
            var pointAWorld = localToWorld.MultiplyPoint3x4(vertices[triangles[i]]);
            var pointBWorld = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 1]]);
            var pointCWorld = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 2]]);

            Vector3 intersection;
            if(LineCrossingCuttingPlane(pointAWorld, pointBWorld, out intersection))
            {
                triangleIntersections.Add(intersection);
            }
            if (LineCrossingCuttingPlane(pointBWorld, pointCWorld, out intersection))
            {
                triangleIntersections.Add(intersection);
                if(triangleIntersections.Count == 2)
                {
                    edgeVertices.AddRange(triangleIntersections);
                    continue; //We have our line, next triangle!
                } 
            }
            if (LineCrossingCuttingPlane(pointCWorld, pointAWorld, out intersection))
                {
                    triangleIntersections.Add(intersection);
                    if (triangleIntersections.Count == 2)
                    {
                        edgeVertices.AddRange(triangleIntersections);
                        continue; //We have our line, next triangle!
                    }
                }
        }

        return edgeVertices;
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
        Vector3 q = (b - a);
        q.Normalize();
        Vector3 planeEqation;
        Vector3 pointOnPlane = plane.ClosestPointOnPlane(Vector3.zero);
        Vector3 normal = plane.normal;
        planeEqation = normal;
        var offset = Vector3.Dot(pointOnPlane, normal);
        var t = (offset - Vector3.Dot(a, planeEqation)) / Vector3.Dot(q, planeEqation);

        return a + (q * t);
    }

    private GameObject CreateGameObjectWithProfile(Mesh profileMesh)
    {
        var outputObject = new GameObject();
        outputObject.AddComponent<MeshRenderer>();
        var newMeshFilter = outputObject.AddComponent<MeshFilter>();
        newMeshFilter.mesh = profileMesh;

        return outputObject;
    }

    public bool IsInLayerMask(int layer)
    {
        return layerMask == (layerMask | (1 << layer));
    }
}
