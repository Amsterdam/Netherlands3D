using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class MeshesProfileCutter : MonoBehaviour
{
    [Header("Listen to")]
    [SerializeField] private TriggerEvent onCutMeshes;
    [SerializeField] private Vector3ListEvent onReceiveCuttingLine;

    private List<Vector3> cuttingLine;

    [Header("Settings")]
    [SerializeField, Tooltip("The meshes on these layers will be cut, and included in the profile")] 
    private LayerMask layerMask;
    private bool drawBoundsGizmo = true;

    private Bounds lineBounds;
    private Vector3 lineCenter;

    void Awake()
    {
        onReceiveCuttingLine.started.AddListener(SetLine);
        onCutMeshes.started.AddListener(CutMeshes);
    }

    private void SetLine(List<Vector3> cuttingLine)
    {
        this.cuttingLine = cuttingLine;
        lineCenter = Vector3.Lerp(this.cuttingLine[0], this.cuttingLine[1], 0.5f);
        var width = Mathf.Abs(this.cuttingLine[0].x - this.cuttingLine[1].x);
        var depth = Mathf.Abs(this.cuttingLine[0].z - this.cuttingLine[1].z);

        lineBounds = new Bounds(lineCenter, new Vector3(width, 300, depth));
    }

    private void OnDrawGizmos()
    {
        if (drawBoundsGizmo && lineBounds != null)
        {
            Gizmos.color = new Color(1,1,0,0.5f);
            Gizmos.DrawCube(lineBounds.center, lineBounds.size);
        }
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

    private IEnumerator CutLoop(MeshFilter[] meshFilters)
    {
        foreach (var meshFilter in meshFilters)
        {
            if (IsInLayerMask(meshFilter.gameObject.layer))
            {
                var renderer = meshFilter.GetComponent<Renderer>();
                if (renderer != null && renderer.bounds.Intersects(lineBounds))
                {
                    var profileMesh = GetMeshProfile(meshFilter);
                    var profileGameObject = CreateGameObjectWithProfile(profileMesh);

                    profileGameObject.transform.SetPositionAndRotation(meshFilter.gameObject.transform.position, meshFilter.gameObject.transform.rotation);
                    profileGameObject.transform.localScale = meshFilter.gameObject.transform.localScale;
                }
            }
        }
        yield return null;
    }

    private Mesh GetMeshProfile(MeshFilter meshFilter)
    {
        var targetMesh = meshFilter.sharedMesh;

        var vertices = targetMesh.vertices;
        var triangles = targetMesh.triangles;

        //For each triangle, add the intersection line to our list
        List<Vector3> edgeVertices = new List<Vector3>();
        for (int i = 0; i < triangles.Length; i+=3)
        {
            //TODO: check all 3 lines of triangle for intersection point
            //If two points are found, add the line (two vector3) to our edgeLines list

        }

        int[] indices = new int[edgeVertices.Count]; 
        var lineTopologyMesh = new Mesh();
        lineTopologyMesh.SetVertices(edgeVertices);
        lineTopologyMesh.SetIndices(indices, MeshTopology.Lines, 0);
        return lineTopologyMesh;
    }

    private Mesh SliceMeshes(MeshFilter meshFilter)
    {
        var gameObjectToSlice = meshFilter.gameObject;
        Debug.Log($"Trying to slice gameObject mesh: {gameObjectToSlice.name}", gameObjectToSlice);

        var direction = Vector3.Cross((cuttingLine[0] = cuttingLine[1]).normalized, Vector3.up);
        SlicedHull ezySlicedHull = gameObjectToSlice.Slice(lineCenter, direction);

        return ezySlicedHull.lowerHull;
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
