using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using System.IO;
using B3dm.Tile;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class TileImport : MonoBehaviour
{
    [SerializeField] StringEvent binTilePath;
    [SerializeField] StringEvent changeUrl;
    [SerializeField] TriggerEvent loadFromURL;
    [SerializeField] StringEvent parseTime;

    [SerializeField] Vector3 cameraOffsetDirection = new Vector3(0,1,-1);

    private string url = "";

    [SerializeField]
    private bool writeGlbNextToB3dm;

    private void Awake()
    {
        binTilePath.started.AddListener(ImportBinFromFile);
        changeUrl.started.AddListener((newUrl) => { url = newUrl; });
        loadFromURL.started.AddListener(LoadFromURL);
    }

    private void LoadFromURL()
    {
        StartCoroutine(ImportBinFromURL(url));
    }

    private IEnumerator ImportBinFromURL(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                byte[] bytes = webRequest.downloadHandler.data;

                if (Path.GetExtension(url).Equals(".b3dm"))
                {
                    var memoryStream = new MemoryStream(bytes);
                    var b3dm = B3dmReader.ReadB3dm(memoryStream);

                    bytes = new MemoryStream(b3dm.GlbData).ToArray();

#if UNITY_EDITOR
                    if (writeGlbNextToB3dm)
                    {
                        var localGlbPath = Application.persistentDataPath + "/" + Path.GetFileName(url).Replace(".b3dm", ".glb");
                        Debug.Log("Writing local file: " + localGlbPath);
                        File.WriteAllBytes(localGlbPath, bytes);
                    }
#endif
                }

                yield return ParseFromBytes(bytes);
            }
        }
    }

    private async void ImportBinFromFile(string filepath)
    {
        byte[] bytes = null;

#if UNITY_WEBGL && !UNITY_EDITOR
        filepath = Application.persistentDataPath + "/" + filepath;
#endif

        if (Path.GetExtension(filepath).Equals(".b3dm"))
        {
            //Retrieve the glb from the b3dm
            var b3dmFileStream = File.OpenRead(filepath);
            var b3dm = B3dmReader.ReadB3dm(b3dmFileStream);

            bytes = new MemoryStream(b3dm.GlbData).ToArray();

#if UNITY_EDITOR
            if (writeGlbNextToB3dm)
            {
                var localGlbPath = filepath.Replace(".b3dm", ".glb");
                Debug.Log("Writing local file: " + localGlbPath);
                File.WriteAllBytes(localGlbPath, bytes);
            }
#endif
        }
        else
        {
            bytes = File.ReadAllBytes(filepath);
        }

        await ParseFromBytes(bytes);
    }

    private async Task ParseFromBytes(byte[] glbBuffer)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        //Use our parser (in this case GLTFFast to read the binary data and instantiate the Unity objects in the scene)
        var gameObject = new GameObject();
        var gltf = gameObject.AddComponent<GLTFast.GltfBinary>();
        await gltf.LoadBinaryAndInstantiate(glbBuffer);

        stopwatch.Stop();
        parseTime.Invoke(stopwatch.ElapsedTicks.ToString() + " ticks");

        FocusCamera(gameObject);
    }

    private void FocusCamera(GameObject gameObject)
    {
        //Move camera for preview
        var renderer = gameObject.GetComponentInChildren<Renderer>();
        Camera.main.transform.position = renderer.bounds.center + (cameraOffsetDirection * renderer.bounds.size.magnitude);
        Camera.main.transform.LookAt(renderer.bounds.center);
    }
}
