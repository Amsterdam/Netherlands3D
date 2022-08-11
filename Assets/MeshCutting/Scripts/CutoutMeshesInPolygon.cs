using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;

public class CutoutMeshesInPolygon : MonoBehaviour
{
    private MeshFilter[] allMeshFilters;
    [SerializeField] private GameObject targetMeshGameObject;
    [SerializeField] private GameObject intersectionMeshGameObject;

    void Start()
    {
        allMeshFilters = FindObjectsOfType<MeshFilter>();
    }

    [ContextMenu("Boolean operation")]
    void Intersection()
    {
       // Include the library

        // Initialize two new meshes in the scene
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = Vector3.one * 1.3f;

        // Perform boolean operation
        Model result = CSG.Subtract(targetMeshGameObject, intersectionMeshGameObject);

        // Create a gameObject to render the result
        var composite = new GameObject();
        composite.AddComponent<MeshFilter>().sharedMesh = result.mesh;
        composite.AddComponent<MeshRenderer>().sharedMaterials = result.materials.ToArray();
    }
}
