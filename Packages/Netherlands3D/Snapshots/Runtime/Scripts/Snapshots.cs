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

        [SerializeField] LayerMask snapshotLayers;

        public UnityEvent<string> snapshotSaved;

        [Header("Optional source camera (Defaults to Camera.main)")]
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

        /// <summary>
        /// Set the layermask for the snapshot camera ( if you want to exclude, include specific layers in the image )
        /// </summary>
        /// <param name="snapshotLayers">Include these layers in the snapshot</param>
        public void SetLayerMask(LayerMask snapshotLayers)
        {
            this.snapshotLayers = snapshotLayers;
        }

        public void TakeSnapshot()
        {
            if (!sourceCamera)
                sourceCamera = Camera.main;

            byte[] bytes = SnapshotToImageBytes(width, height, fileType, sourceCamera, snapshotLayers);

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
            if (!string.IsNullOrEmpty(filePath))
                File.WriteAllBytes(filePath, bytes);
#endif
        }

        /// <summary>
        /// Create a snapshot based on source camera
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="fileType">Supported extention/types are 'png', 'jpg' and 'raw'</param>
        /// <returns>Byte array of the encoded image type</returns>
        public static byte[] SnapshotToImageBytes(int width, int height, string fileType = "png", Camera sourceCamera = null, LayerMask snapshotLayers = default)
        {
            // Create temporary camera based on main
            Camera snapshotCamera = new GameObject().AddComponent<Camera>();
            if (!sourceCamera) sourceCamera = Camera.main;

            snapshotCamera.transform.SetPositionAndRotation(sourceCamera.transform.position, sourceCamera.transform.rotation);
            snapshotCamera.CopyFrom(Camera.main);
            if(snapshotLayers != default)
            {
                snapshotCamera.cullingMask = snapshotLayers;
            }

            //Create temporary textures to render to
            Texture2D screenShot = new Texture2D(width,height,TextureFormat.RGB24, false);
            RenderTexture screenshotRenderTexture = new RenderTexture(width, height, 24);
            RenderTexture.active = screenshotRenderTexture;

            //Make sure our render camera can see the canvases
            Dictionary<Canvas, RenderMode> canvasRenderModes = new Dictionary<Canvas, RenderMode>();
            var canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                canvasRenderModes.Add(canvas, canvas.renderMode);

                canvas.worldCamera = snapshotCamera;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.planeDistance = snapshotCamera.nearClipPlane + 0.1f;
            }

            // Render the camera
            snapshotCamera.targetTexture = screenshotRenderTexture;
            snapshotCamera.Render();

            //Read pixels from targetTexture
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();

            // Resets variables
            snapshotCamera.targetTexture = null;
            RenderTexture.active = null;

            //Reset canvases back to their original mode
            foreach(KeyValuePair<Canvas,RenderMode> canvasMode in canvasRenderModes)
            {
                var canvas = canvasMode.Key;
                var originalMode = canvasMode.Value;

                canvas.renderMode = originalMode;
            }

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
            Destroy(snapshotCamera.gameObject);

            //Return bytes
            return bytes;
        }
    }
}
