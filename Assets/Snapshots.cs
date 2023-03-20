using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D
{
    public class Snapshots : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void DownloadFile(byte[] array, int byteLength, string fileName);

        private int width = 1920;
        private int height = 1080;

        private string targetPath = "";
        private string fileName = "Snapshot";
        private string fileType = "png";
        private string filePath = "";

        public UnityEvent<string> snapshotSaved;

        [Header("Optional source camera (Fallback to Camera.main)")]
        public Camera sourceCamera;

        public string FileName { get => fileName; set => fileName = value; }
        public string TargetPath { get => targetPath; set => targetPath = value; }
        public string FileType { get => fileType; set => fileType = value; }

        public void SetImageWidth(string width)
        {
            SetImageWidth(int.Parse(width));
        }
        public void SetImageWidth(int width)
        {
            this.width = width;
        }

        public void SetImageHeight(string height)
        {
            SetImageHeight(int.Parse(height));
        }
        public void SetImageHeight(int height)
        {
            this.height = height;
        }

        public void TakeSnapshot()
        {
            StartCoroutine(SnapshotWithTemporaryCamera());
        }

        IEnumerator SnapshotWithTemporaryCamera()
        {
            if (!sourceCamera)
                sourceCamera = Camera.main;

            byte[] bytes = RenderCameraToBytes(width,height, fileType, sourceCamera);

#if UNITY_EDITOR
            // Window for user to input desired path/name/filetype
            filePath = EditorUtility.SaveFilePanel("Save texture as PNG", "", fileName, fileType);
#endif

            // Default filename
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "Snapshot_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "." + FileType;
            }
            else
            {
                fileName = fileName + "." + FileType;
            }

            //Use the jslib DownloadFile to download the bytes as a file in WebGL/Browser
#if UNITY_WEBGL && !UNITY_EDITOR
            DownloadFile(bytes, bytes.Length, fileName);
#else
            File.WriteAllBytes(filePath, bytes);
#endif
            yield return null;
        }

        /// <summary>
        /// Create a snapshot based on source camera
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="fileType">Supported types are 'png', 'jpg' and 'raw'</param>
        /// <returns>Byte array of the encoded image type</returns>
        private static byte[] RenderCameraToBytes(int width, int height, string fileType = "png", Camera sourceCamera = null)
        {
            // Create temporary camera based on main
            Camera snapshotCamera = new Camera();
            if(sourceCamera)
                snapshotCamera.CopyFrom(Camera.main);

            //Create temporary textures to render to
            Texture2D screenShot = new Texture2D(width,height,TextureFormat.RGB24, false);
            RenderTexture screenshotRenderTexture = new RenderTexture(width, height, 24);
            RenderTexture.active = screenshotRenderTexture;

            // Render the camera
            snapshotCamera.targetTexture = screenshotRenderTexture;
            snapshotCamera.Render();

            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();

            // Resets variables
            snapshotCamera.targetTexture = null;
            RenderTexture.active = null;
            byte[] bytes = fileType switch
            {
                "png" => screenShot.EncodeToPNG(),
                "jpg" => screenShot.EncodeToJPG(),
                "raw" => screenShot.GetRawTextureData(),
                _ => screenShot.EncodeToPNG(),
            };

            // Cleanup temporary textures and camera
            Destroy(screenshotRenderTexture);
            Destroy(screenShot);
            Destroy(snapshotCamera);

            //Return bytes
            return bytes;
        }
    }
}
