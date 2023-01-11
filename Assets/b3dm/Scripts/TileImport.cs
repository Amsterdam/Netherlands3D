using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using System.IO;
using B3dm.Tile;

public class TileImport : MonoBehaviour
{
    [SerializeField] StringEvent binTilePath;
    [SerializeField] Vector3 cameraOffsetDirection = new Vector3(0,1,-1);

    private void Awake()
    {
        binTilePath.started.AddListener(ImportBin);
    }

    private async void ImportBin(string filepath)
    {
        byte[] glbBuffer = null;

        if (Path.HasExtension(".b3dm"))
        {
            //Retrieve the glb from the b3dm
            var b3dmFileStream = File.OpenRead(filepath);
            var b3dm = B3dmReader.ReadB3dm(b3dmFileStream);

            glbBuffer = new MemoryStream(b3dm.GlbData).ToArray();
        }
        else
        {
            glbBuffer = File.ReadAllBytes(filepath);
        }

        //Use our parser (in this case GLTFFast to read the binary data and instantiate the Unity objects in the scene)
        var gameObject = new GameObject();
        var gltf = gameObject.AddComponent<GLTFast.GltfBinary>();
        await gltf.LoadBinaryAndInstantiate(glbBuffer);

        //Move camera for preview
        var renderer = gameObject.GetComponentInChildren<Renderer>();
        Camera.main.transform.position = renderer.bounds.center + (cameraOffsetDirection * renderer.bounds.size.magnitude);
    }
}
