using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Snapshots
{
    public class Snapshot
    {
        /// <summary>
        /// Create a snapshot based on source camera
        /// </summary>
        /// <param name="imageWidth">Width in pixels</param>
        /// <param name="imageHeight">Height in pixels</param>
        /// <param name="fileType">Supported extension/types are 'png', 'jpg' and 'raw'</param>
        /// <returns>Byte array of the encoded image type</returns>
        public static byte[] ToImageBytes(
            int imageWidth,
            int imageHeight,
            Camera sourceCamera,
            LayerMask snapshotLayers = default,
            SnapshotFileType fileType = SnapshotFileType.png
        ) {
            var screenShot = ToTexture2D(imageWidth, imageHeight, sourceCamera, snapshotLayers);

            byte[] bytes = fileType switch
            {
                SnapshotFileType.png => screenShot.EncodeToPNG(),
                SnapshotFileType.jpg => screenShot.EncodeToJPG(),
                SnapshotFileType.raw => screenShot.GetRawTextureData(),
                _ => screenShot.EncodeToPNG(),
            };
            GameObject.Destroy(screenShot);

            return bytes;
        }

        public static Texture2D ToTexture2D(
            int imageWidth,
            int imageHeight,
            Camera sourceCamera,
            LayerMask snapshotLayers
        ) {
            var screenshotRenderTexture = ToRenderTexture(imageWidth, imageHeight, sourceCamera, snapshotLayers);

            Texture2D screenShot = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
            RenderTexture.active = screenshotRenderTexture;
            screenShot.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
            screenShot.Apply();
            RenderTexture.active = null;
            GameObject.Destroy(screenshotRenderTexture);

            return screenShot;
        }

        public static RenderTexture ToRenderTexture(
            int imageWidth,
            int imageHeight,
            Camera sourceCamera,
            LayerMask snapshotLayers
        ) {
            var snapshotCamera = CreateSnapshotCamera(sourceCamera, snapshotLayers);
            var cachedCanvasRenderModes = AttachCanvasesToSnapshotCamera(snapshotCamera);

            //Create temporary textures to render to
            RenderTexture screenshotRenderTexture = new RenderTexture(imageWidth, imageHeight, 24);

            // Render the camera
            snapshotCamera.targetTexture = screenshotRenderTexture;
            snapshotCamera.Render();
            snapshotCamera.targetTexture = null;

            RestoreSettingsOnCanvases(cachedCanvasRenderModes);
            GameObject.Destroy(snapshotCamera.gameObject);

            return screenshotRenderTexture;
        }

        private static Camera CreateSnapshotCamera(Camera sourceCamera, LayerMask snapshotLayers)
        {
            var sourceCameraTransform = sourceCamera.transform;

            Camera snapshotCamera = new GameObject().AddComponent<Camera>();
            snapshotCamera.transform.SetPositionAndRotation(sourceCameraTransform.position, sourceCameraTransform.rotation);
            snapshotCamera.CopyFrom(sourceCamera);
            if (snapshotLayers != default)
            {
                snapshotCamera.cullingMask = snapshotLayers;
            }

            return snapshotCamera;
        }

        private static Dictionary<Canvas, RenderMode> AttachCanvasesToSnapshotCamera(Camera snapshotCamera)
        {
            var canvases = GameObject.FindObjectsOfType<Canvas>();
            Dictionary<Canvas, RenderMode> canvasRenderModes = new Dictionary<Canvas, RenderMode>();
            foreach (Canvas canvas in canvases)
            {
                // Cache prior render mode to restore it later
                canvasRenderModes.Add(canvas, canvas.renderMode);

                AttachCanvasToSnapshotCamera(canvas, snapshotCamera);
            }

            return canvasRenderModes;
        }

        private static void AttachCanvasToSnapshotCamera(Canvas canvas, Camera snapshotCamera)
        {
            canvas.worldCamera = snapshotCamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = snapshotCamera.nearClipPlane + 0.1f;
        }

        private static void RestoreSettingsOnCanvases(Dictionary<Canvas, RenderMode> cachedCanvasRenderModes)
        {
            //Reset canvases back to their original mode
            foreach (KeyValuePair<Canvas, RenderMode> canvasMode in cachedCanvasRenderModes)
            {
                var canvas = canvasMode.Key;
                var originalMode = canvasMode.Value;

                canvas.renderMode = originalMode;
            }
        }
    }
}
