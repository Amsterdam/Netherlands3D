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
        [Header("Listen to")]
        [SerializeField] StringEvent binTilePath;
        [SerializeField] StringEvent changeUrl;
        [SerializeField] TriggerEvent loadFromURL;

        [Header("Invoke")]
        [SerializeField] GameObjectEvent onCreatedGameObject;
        [SerializeField] StringEvent logStat;

        private string url = "";
        private string stats = "";

        [Header("Develop")]
        [SerializeField] StringEvent parseTime;
        [SerializeField] private bool debug = false;
        [SerializeField] private bool writeGlbNextToB3dm;

        private static CustomCertificateValidation customCertificateHandler;
        private static ImportSettings importSettings;


        private void Awake()
        {
            customCertificateHandler = new CustomCertificateValidation();
            importSettings = new ImportSettings() { AnimationMethod = AnimationMethod.None };

            if (binTilePath) binTilePath.AddListenerStarted(ImportBinFromFile);
            if (changeUrl) changeUrl.AddListenerStarted((newUrl) => { url = newUrl; });
            if (loadFromURL) loadFromURL.AddListenerStarted(LoadFromURL);
        }

        private void LoadFromURL()
        {
            StartCoroutine(ImportBinFromURL(url, null, new UnityWebRequest()));
        }

        public static IEnumerator ImportBinFromURL(string url, Action<GltfImport> callbackGltf, UnityWebRequest webRequest)
        {
            webRequest = UnityWebRequest.Get(url);
            webRequest.certificateHandler = customCertificateHandler; //Not safe; but solves breaking curl error

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(url + " -> " +webRequest.error);
                callbackGltf.Invoke(null);
            }
            else
            {
                byte[] bytes = webRequest.downloadHandler.data;
                var memory = new ReadOnlyMemory<byte>(bytes);

                if (Path.GetExtension(url).Equals(".b3dm"))
                {
                    var memoryStream = new MemoryStream(bytes);
                    bytes = B3dmReader.ReadB3dmGlbContentOnly(memoryStream);
                }

                yield return ParseFromBytes(bytes, url, callbackGltf);
            }

            webRequest.Dispose();
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

        private static async Task ParseFromBytes(byte[] glbBuffer, string sourcePath, Action<GltfImport> callbackGltf)
        {
            //Use our parser (in this case GLTFFast to read the binary data and instantiate the Unity objects in the scene)
            var gltf = new GltfImport();
            var success = await gltf.Load(glbBuffer, new Uri(sourcePath), importSettings);

            if (success)
            {
                callbackGltf?.Invoke(gltf);
            }
            else
            {
                Debug.LogError("Loading glTF failed!");
                callbackGltf?.Invoke(null);
                gltf.Dispose();
            }
        }


        private void LogB3DMFeatures(B3dm.Tile.B3dm b3dm)
        {
            LogStat("B3DM Version:");
            LogStat(b3dm.B3dmHeader.Version.ToString());

            LogStat("FeatureTableJson:");
            LogStat(b3dm.FeatureTableJson);

            LogStat("BatchTableJson:");
            LogStat(b3dm.BatchTableJson);
        }

        private void LogGltfStats(GltfImport gltf)
        {
            LogStat("GLTF scene count: " + gltf.SceneCount);
            LogStat("GLTF material count: " + gltf.MaterialCount);
            LogStat("GLTF texture count: " + gltf.TextureCount);

            var meshes = gltf.GetMeshes();
            LogStat("GLTF mesh count: " + meshes.Length);
            foreach (var mesh in meshes)
            {
                LogStat("Mesh " + mesh.name + " with submesh count: " + mesh.subMeshCount);
            }
        }

        private void LogStat(string stat)
        {
            Debug.Log(stat);

            stats += stat + "\n\n";
            if (logStat) logStat.Invoke(stats);
        }
    }
}

public class CustomCertificateValidation : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}