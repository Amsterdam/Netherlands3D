using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;
using GLTFast;
using System;
using SimpleJSON;
#if UNITY_EDITOR
using System.IO.Compression;
#endif
namespace Netherlands3D.B3DM
{
    public class ImportB3DMGltf
    {
        private static CustomCertificateValidation customCertificateHandler = new CustomCertificateValidation();
        private static ImportSettings importSettings = new ImportSettings() { AnimationMethod = AnimationMethod.None };

        /// <summary>
        /// Helps bypassing expired certificate warnings.
        /// Use with caution, and only with servers you trust.
        /// </summary>
        public class CustomCertificateValidation : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }

        /// <summary>
        /// Rerturns IENumerator for a webrequest for a .b3dm, .glb or .gltf and does a GltfImport callback on success
        /// </summary>
        /// <param name="url">Full url to .b3dm, .glb or .gltf file</param>
        /// <param name="callbackGltf">The callback to receive the GltfImport on success</param>
        /// <param name="webRequest">Provide </param>
        /// <param name="bypassCertificateValidation"></param>
        /// <returns></returns>
        public static IEnumerator ImportBinFromURL(string url, Action<ParsedGltf> callbackGltf, bool bypassCertificateValidation = false)
        {
            var webRequest = UnityWebRequest.Get(url);

            if (bypassCertificateValidation)
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
                double[] rtcCenter = null;

                if (url.Contains(".b3dm"))
                {
                    var memoryStream = new MemoryStream(bytes);

                    var b3dm = B3dmReader.ReadB3dm(memoryStream);
                    bytes = b3dm.GlbData;

                    //Optional RTC_CENTER from b3DM header
                    rtcCenter = GetRTCCenter(rtcCenter, b3dm);
                }

                yield return ParseFromBytes(bytes, url, callbackGltf, rtcCenter);
            }

            webRequest.Dispose();
        }

        private static double[] GetRTCCenter(double[] rtcCenter, B3dm b3dm)
        {
            if (b3dm.FeatureTableJson.Length > 0)
            {
                JSONNode rootnode = JSON.Parse(b3dm.FeatureTableJson);
                var rtcCenterValues = rootnode["RTC_CENTER"];
                rtcCenter = new double[3];
                if (rtcCenterValues != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        rtcCenter[i] = rtcCenterValues[i].AsDouble;
                    }
                }
            }

            return rtcCenter;
        }

        /// <summary>
        /// Import binary .glb,.gltf data or get it from a .b3dm
        /// </summary>
        /// <param name="filepath">Path to local .glb,.gltf or .b3dm file</param>
        /// <param name="writeGlbNextToB3dm">Extract/copy .glb file from .b3dm and place it next to it.</param>
        public async void ImportBinFromFile(string filepath, bool writeGlbNextToB3dm = false)
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
                bytes = b3dm.GlbData;

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

        /// <summary>
        /// Parse glb (or gltf) buffer bytes and do a callback when done containing a GltfImport.
        /// </summary>
        /// <param name="glbBuffer">The bytes of a glb or gtlf file</param>
        /// <param name="sourcePath">Sourcepath is required to be able to load files with external dependencies like textures etc.</param>
        /// <param name="callbackGltf">The callback containing the GltfImport result</param>
        /// <returns></returns>
        private static async Task ParseFromBytes(byte[] glbBuffer, string sourcePath, Action<ParsedGltf> callbackGltf, double[] rtcCenter = null)
        {
            //Use our parser (in this case GLTFFast to read the binary data and instantiate the Unity objects in the scene)
            var gltf = new GltfImport();
            var success = await gltf.Load(glbBuffer, new Uri(sourcePath), importSettings);

            if (success)
            {
                var parsedGltf = new ParsedGltf()
                {
                    gltfImport = gltf,
                    rtcCenter = rtcCenter
                };
                callbackGltf?.Invoke(parsedGltf);
            }
            else
            {
                Debug.LogError($"Loading glTF failed! -> {sourcePath}");
                callbackGltf?.Invoke(null);
                gltf.Dispose();
            }
        }
    }
}

[Serializable]
public class ParsedGltf
{
    public GltfImport gltfImport;
    public double[] rtcCenter = null;
}
