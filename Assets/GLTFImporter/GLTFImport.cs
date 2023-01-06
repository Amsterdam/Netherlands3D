using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

public class GLTFImport : MonoBehaviour
{
    [SerializeField] StringEvent gltfPath;

    private void Awake()
    {
        gltfPath.started.AddListener(ImportGLTF);
    }

    private void ImportGLTF(string filepath)
    {
        var gameObject = new GameObject();
        var gltf = gameObject.AddComponent<GLTFast.GltfAsset>();
        gltf.Url = "file:///" + Application.persistentDataPath + filepath;
        Debug.Log(gltf.FullUrl);
    }
}
