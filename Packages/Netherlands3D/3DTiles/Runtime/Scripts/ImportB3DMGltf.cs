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
#if UNITY_EDITOR
using System.IO.Compression;
#endif
namespace Netherlands3D.B3DM
{
    public class ImportB3DMGltf
    {
        private static CustomCertificateValidation customCertificateHandler = new CustomCertificateValidation();
        private static ImportSettings importSettings = new ImportSettings() { AnimationMethod = AnimationMethod.None };

        private static int exp = 1;

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
        public static IEnumerator ImportBinFromURL(string url, Action<GltfImport> callbackGltf, bool bypassCertificateValidation = false)
        {
            var webRequest = UnityWebRequest.Get(url);
            webRequest.SetRequestHeader("Accept-Encoding", "gzip");

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
                byte[] bytes;

#if UNITY_EDITOR && !UNITY_WEBGL
                string contentEncoding = webRequest.GetResponseHeader("Content-Encoding");
                bool isGzipped = !string.IsNullOrEmpty(contentEncoding) && contentEncoding.ToLower().Contains("gzip");
                if (isGzipped)
                {
                    Debug.Log("Response data is gzipped");
                    using MemoryStream compressedStream = new MemoryStream(webRequest.downloadHandler.data);

                    if(exp > 0)
                    {
                        exp--;
                        var path = Application.dataPath + Path.GetFileName(url);
                        File.WriteAllBytes(path, webRequest.downloadHandler.data);
                        Debug.Log("Check " + path);
                    }

                    using GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
                    using MemoryStream decompressedStream = new MemoryStream();

                    gzipStream.CopyTo(decompressedStream);
                    bytes = decompressedStream.ToArray();
                }
                else
                {
                    bytes = webRequest.downloadHandler.data;
                }
#else
                bytes = webRequest.downloadHandler.data;
#endif

                var memory = new ReadOnlyMemory<byte>(bytes);

                if (url.Contains(".b3dm"))
                {
                    var memoryStream = new MemoryStream(bytes);
                    bytes = B3dmReader.ReadB3dmGlbContentOnly(memoryStream);
                    if (exp > 0)
                    {
                        exp--;
                        var path = Application.dataPath + Path.GetFileName(url.Replace(".b3dm",".glb"));
                        File.WriteAllBytes(path, bytes);
                        Debug.Log("Check " + path);
                    }
                }

                yield return ParseFromBytes(bytes, url, callbackGltf);
            }

            webRequest.Dispose();
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

        /// <summary>
        /// Parse glb (or gltf) buffer bytes and do a callback when done containing a GltfImport.
        /// </summary>
        /// <param name="glbBuffer">The bytes of a glb or gtlf file</param>
        /// <param name="sourcePath">Sourcepath is required to be able to load files with external dependencies like textures etc.</param>
        /// <param name="callbackGltf">The callback containing the GltfImport result</param>
        /// <returns></returns>
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
    }
}
