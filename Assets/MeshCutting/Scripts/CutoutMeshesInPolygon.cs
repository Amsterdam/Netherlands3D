using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;
using Netherlands3D.Events;

public class CutoutMeshesInPolygon : MonoBehaviour
{
    [SerializeField] private string tagForCutoutTarget = "CutoutTarget";

    private MeshFilter[] allMeshFilters;
    private GameObject targetMeshGameObject;
    private GameObject booleanMeshGameObject;

    [SerializeField] private GameObjectEvent intersectWithShape;

    private void Awake()
    {
        intersectWithShape.started.AddListener((newBooleanGameObject) => { 
            booleanMeshGameObject = newBooleanGameObject;
            Intersection();
        });
    }

    private void FindTarget()
    {
        allMeshFilters = FindObjectsOfType<MeshFilter>();
        foreach (MeshFilter filter in allMeshFilters)
        {
            if (filter.gameObject.CompareTag(tagForCutoutTarget))
            {
                targetMeshGameObject = filter.gameObject;
                Debug.Log($"Cutout target set: {this.gameObject}", this.gameObject);
            }
        }
    }

    [ContextMenu("Boolean operation")]
    void Intersection()
    {
        FindTarget();
        if (!targetMeshGameObject) { Debug.LogWarning("No targetMeshGameObject set.", this.gameObject); return; }
        if (!booleanMeshGameObject) { Debug.LogWarning("No booleanMeshGameObject set.", this.gameObject); return; }

        // Perform boolean operation
        Model result = CSG.Subtract(targetMeshGameObject, booleanMeshGameObject);

        // Create a gameObject to render the result
        var composite = new GameObject();
        composite.name = targetMeshGameObject.name;
        composite.transform.SetParent(targetMeshGameObject.transform.parent, true);
        composite.AddComponent<MeshFilter>().sharedMesh = result.mesh;
        composite.AddComponent<MeshRenderer>().sharedMaterials = result.materials.ToArray();

        Destroy(targetMeshGameObject);
    }
}
