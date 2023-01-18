using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using System.IO;
using B3dm.Tile;
using UnityEngine.Networking;
using System.Threading.Tasks;
using GLTFast;
using System;

namespace Netherlands3D.B3DM
{
    public class ImportB3DMGltf : MonoBehaviour
    {
        [SerializeField] StringEvent binTilePath;
        [SerializeField] StringEvent changeUrl;
        [SerializeField] TriggerEvent loadFromURL;
        [SerializeField] GameObjectEvent onCreatedGameObject;

        private string url = "";

        [Header("Develop")]
        [SerializeField] StringEvent parseTime;
        [SerializeField] private bool debug = false;
        [SerializeField] private bool writeGlbNextToB3dm;

        private void Awake()
        {
            binTilePath.started.AddListener(ImportBinFromFile);
            changeUrl.started.AddListener((newUrl) => { url = newUrl; });
            loadFromURL.started.AddListener(LoadFromURL);
        }

        public void LoadFromURL()
        {
            StartCoroutine(ImportBinFromURL(url, null));
        }

        public void LoadFromURL(string url, Action<GameObject> callback)
        {
            StartCoroutine(ImportBinFromURL(url, callback));
        }

        private IEnumerator ImportBinFromURL(string url, Action<GameObject> callback)
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
                        if (debug)
                        {
                            LogB3DMFeatures(b3dm);
                        }
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

                    yield return ParseFromBytes(bytes, url, callback);
                }
            }
        }

        public async void ImportBinFromFile(string filepath)
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

            await ParseFromBytes(bytes, filepath, null);
        }

        private async Task ParseFromBytes(byte[] glbBuffer, string sourcePath, Action<GameObject> callback)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            //Use our parser (in this case GLTFFast to read the binary data and instantiate the Unity objects in the scene)
            var gltf = new GltfImport();
            var settings = new ImportSettings();
            settings.AnimationMethod = AnimationMethod.None;

            var success = await gltf.Load(glbBuffer, new System.Uri(sourcePath), settings);

            stopwatch.Stop();
            parseTime.Invoke(stopwatch.ElapsedTicks.ToString() + " ticks");

            if (success)
            {
                var gameObject = new GameObject("glTFScenes");
                var scenes = gltf.SceneCount;

                if(debug)
                    LogGltfStats(gltf);

                for (int i = 0; i < scenes; i++)
                {
                    await gltf.InstantiateSceneAsync(gameObject.transform, i);
                }

                callback?.Invoke(gameObject);

                if(onCreatedGameObject) onCreatedGameObject.Invoke(gameObject);
            }
            else
            {
                Debug.LogError("Loading glTF failed!");
                callback?.Invoke(null);
            }
        }


        private static void LogB3DMFeatures(B3dm.Tile.B3dm b3dm)
        {
            Debug.Log("BatchTableJson:");
            Debug.Log(b3dm.BatchTableJson);
        }

        private static void LogGltfStats(GltfImport gltf)
        {
            Debug.Log("GLTF scene count: " + gltf.SceneCount);
            Debug.Log("GLTF material count: " + gltf.MaterialCount);
            Debug.Log("GLTF texture count: " + gltf.TextureCount);

            var meshes = gltf.GetMeshes();
            Debug.Log("GLTF mesh count: " + meshes.Length);
            foreach (var mesh in meshes)
            {
                Debug.Log("Mesh " + mesh.name + " with submesh count: " + mesh.subMeshCount);
            }
        }
    }
}
