using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using System;

namespace Netherlands3D.ProfileRendering
{
    public class ProfileRenderer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float heightRange = 300;
        [SerializeField] private float heightOffset = -100;
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private Transform cuttingLine;

        [Header("Listen to")]
        [SerializeField] private Vector3ListEvent onReceiveCuttingLine;

        [Header("Invoke")]
        [SerializeField] private FloatEvent sizeWasChanged;

        private Camera renderCamera;

        private List<Vector3> linePoints;
        private float worldSliceHeight = 0;

        private void Awake()
        {
            renderCamera = this.GetComponent<Camera>();
            renderCamera.targetTexture = renderTexture;
            //We render manualy using renderCamera.Render();
            renderCamera.enabled = false;
        }

        private void OnEnable()
        {
            cuttingLine.gameObject.SetActive(false);
            onReceiveCuttingLine.AddListenerStarted(Align);
        }

        private void OnDisable()
        {
            onReceiveCuttingLine.RemoveListenerStarted(Align);
        }

        private void Align(List<Vector3> linePoints)
        {
            this.linePoints = linePoints;

            renderCamera.transform.position = Vector3.Lerp(linePoints[0], linePoints[1], 0.5f) + (Vector3.up * heightOffset);
            renderCamera.transform.right = (linePoints[0] - linePoints[1]).normalized;

            var worldSliceWidth = Vector3.Distance(linePoints[0], linePoints[1]);
            cuttingLine.localScale = new Vector3(worldSliceWidth, heightRange, 1);
            cuttingLine.gameObject.SetActive(true);

            worldSliceHeight = (worldSliceWidth / renderTexture.width) * renderTexture.height;

            sizeWasChanged.InvokeStarted(worldSliceHeight);
            renderCamera.orthographicSize = worldSliceHeight / 2.0f;
            renderCamera.Render();
        }
        private void OnDrawGizmos()
        {
            if (linePoints != null && linePoints.Count > 1)
            {
                Gizmos.DrawLine(linePoints[0] + (Vector3.up * heightOffset), linePoints[0] + (Vector3.up * worldSliceHeight));
                Gizmos.DrawLine(linePoints[1] + (Vector3.up * heightOffset), linePoints[1] + (Vector3.up * worldSliceHeight));
            }
        }
    }
}