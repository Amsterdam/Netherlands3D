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

    [Header("Invoke")]
    private List<Vector3> cuttingLine;

    [Header("Settings")]
    [SerializeField, Tooltip("The meshes on these layers will be cut, and included in the profile")] 
    private LayerMask layerMask;

    void Awake()
    {
        onReceiveCuttingLine.started.AddListener((line) => cuttingLine = line);
        onCutMeshes.started.AddListener(CutMeshes);
    }

    private void CutMeshes()
    {
        var allMeshFilters = FindObjectsOfType<MeshFilter>();

        StartCoroutine(CutLoop(allMeshFilters));
    }

    private IEnumerator CutLoop(MeshFilter[] meshFilters)
    {
        foreach (var meshFilter in meshFilters)
        {
            if (IsInLayerMask(meshFilter.gameObject.layer))
            {
                var profileMesh = GetMeshProfile(meshFilter.mesh);
            }
        }
        yield return null;
    }

    private Mesh GetMeshProfile(Mesh mesh)
    {
        

        return mesh;
    }

    public bool IsInLayerMask(int layer)
    {
        return layerMask == (layerMask | (1 << layer));
    }
}
