using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using System;

public class ProfileRenderer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float heightRange = 300;
    [SerializeField] private float heightOffset = -100;

    [Header("Listen to")]
    [SerializeField] private Vector3ListEvent onReceiveCuttingLine;

    [SerializeField] private FloatEvent onReveiceHeightRange;
    [SerializeField] private FloatEvent onReveiceHeightOffset;

    private Camera renderCamera;
    [SerializeField] private Transform cuttingLine;

    private List<Vector3> linePoints;
    private float worldSliceHeight = 0;

    private void Awake()
    {
        renderCamera = this.GetComponent<Camera>();
        onReceiveCuttingLine.started.AddListener(Align);

        if(onReveiceHeightRange)
            onReveiceHeightRange.started.AddListener((value) => heightRange = value);

        if(onReveiceHeightRange)
            onReveiceHeightRange.started.AddListener((value) => heightOffset = value);
    }

    private void OnDrawGizmos()
    {
        if(linePoints!=null && linePoints.Count>1)
        {
            Gizmos.DrawLine(linePoints[0] + (Vector3.up * heightOffset), linePoints[0] + (Vector3.up * worldSliceHeight));
            Gizmos.DrawLine(linePoints[1] + (Vector3.up * heightOffset), linePoints[1] + (Vector3.up * worldSliceHeight));
        }
    }

    private void Align(List<Vector3> linePoints)
    {
        this.linePoints = linePoints;

        renderCamera.transform.position = Vector3.Lerp(linePoints[0], linePoints[1], 0.5f) + (Vector3.up * heightOffset);
        renderCamera.transform.right = (linePoints[0] - linePoints[1]).normalized;

        var worldSliceWidth = Vector3.Distance(linePoints[0], linePoints[1]);
        cuttingLine.localScale = new Vector3(worldSliceWidth, heightRange, 1);

        worldSliceHeight = (worldSliceWidth / renderCamera.activeTexture.width) * renderCamera.activeTexture.height;

        renderCamera.orthographicSize = worldSliceHeight / 2.0f;
    }
}
