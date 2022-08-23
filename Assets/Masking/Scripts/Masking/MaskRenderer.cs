using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Rendering
{
    [RequireComponent(typeof(Camera))]
    public class MaskRenderer : MonoBehaviour
    {
        private Vector3 lastRenderedPosition;
        private Camera mainCamera;
        private Camera renderCamera;
        private RenderTexture renderTexture;

        private Vector2 maskTiling = new Vector2();
        private Vector2 maskOffset = new Vector2();
        private Extent cameraExtent;
        private Extent lastCameraExtent;

        [Header("Mask camera settings")]
        [SerializeField] private float maxDistance = 1000;
        [SerializeField] private float offset = 500;
        
        [Header("Material references")]
        [SerializeField] private Material maskingObjectsMaterial;
        [SerializeField] private Material[] targetMaterials;

        [Header("Mask texture settings")]
        [SerializeField] private int textureSize = 1024;
        [SerializeField] private bool invertedMask = false;
        [SerializeField] private bool onlyUpdateOnCameraChange = true;
        [SerializeField] private RenderTextureFormat renderTextureFormat = RenderTextureFormat.R8;
        [SerializeField] private AnimationCurve lookDirectionResolution;

        void Awake()
        {
            renderCamera = GetComponent<Camera>();
            renderCamera.enabled = false;
        }

        private void OnEnable()
        {
            renderTexture = new RenderTexture(textureSize, textureSize, 0, renderTextureFormat);
            renderCamera.targetTexture = renderTexture;
            GetMainCamera();
            CameraChanged();
            MaskDirection(invertedMask);
        }
        private void OnDisable()
        {
            if (renderTexture) Destroy(renderTexture);
        }

        private void OnValidate()
        {
            if (renderCamera)
                MaskDirection(invertedMask);
        }

        /// <summary>
        /// Invert the black and white in the mask texture
        /// </summary>
        /// <param name="inverted">On True the objects will be black in the mask instead of white, and the background white instead of black.</param>
        public void MaskDirection(bool inverted)
        {
            maskingObjectsMaterial.color = inverted ? Color.black : Color.white;
            renderCamera.backgroundColor = inverted ? Color.white : Color.black;
        }

        /// <summary>
        /// Target the current camera tagged as main
        /// </summary>
        public void GetMainCamera()
        {
            mainCamera = Camera.main;
            lastRenderedPosition = mainCamera.transform.position;
        }

        void LateUpdate()
        {
            var lookingForward = 1-Math.Abs(Vector3.Dot(Vector3.down, mainCamera.transform.forward));
            var sampleMaxDistance = lookDirectionResolution.Evaluate(lookingForward) * maxDistance;
            cameraExtent = mainCamera.GetExtent(sampleMaxDistance);
            if (!onlyUpdateOnCameraChange || !cameraExtent.Equals(lastCameraExtent))
            {
                CameraChanged();
            }
        }

        /// <summary>
        /// The camera was changed so we move and scale our renderCamera and texture
        /// </summary>
        private void CameraChanged()
        {
            lastCameraExtent = cameraExtent;
            renderCamera.orthographicSize = (float)cameraExtent.Width / 2.0f;
            this.transform.position = new Vector3((float)lastCameraExtent.CenterX, offset, (float)lastCameraExtent.CenterY);
            renderCamera.Render();
            UpdateMaterialBounds();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(new Vector3((float)cameraExtent.CenterX, offset / 2.0f, (float)cameraExtent.CenterY), new Vector3((float)cameraExtent.Width, offset, (float)cameraExtent.Height));
        }

        /// <summary>
        /// Update our list of target materials with the new mask location and scale
        /// </summary>
        private void UpdateMaterialBounds()
        {
            //We do this square so we do not need dynamic rendertexture (slower)
            var scale = (float)cameraExtent.Width * (1.0f + (2.0f / textureSize));
            maskTiling.Set(1.0f / (float)cameraExtent.Width, 1.0f / (float)cameraExtent.Width);
            maskOffset.Set((-(float)cameraExtent.CenterX / scale) + 0.5f, (-(float)cameraExtent.CenterY / scale) + 0.5f);

            foreach (var material in targetMaterials)
            {
                material.mainTexture = renderTexture;
                material.SetVector("MaskOffset", maskOffset);
                material.SetVector("MaskTiling", maskTiling);
            }
        }
    }
}