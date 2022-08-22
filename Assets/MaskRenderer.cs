using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MaskRenderer : MonoBehaviour
{
    private Vector3 lastRenderedPosition;
    private Camera mainCamera;
    private Camera renderCamera;

    private Vector2 maskTiling = new Vector2();
    private Vector2 maskOffset = new Vector2();

    private Extent cameraExtent;
    private Extent lastCameraExtent;

    [SerializeField] private float maskingMaxDistance = 1000;
    private int textureSize = 1024;

    [SerializeField] private float offset = 500;
    [SerializeField] private float correction = 0;
    [SerializeField] private Material[] targetMaterials;

    void Start()
    {
        renderCamera = GetComponent<Camera>();
        renderCamera.enabled = false;

        GetMainCamera();
        CameraChanged();
    }

    public void GetMainCamera()
    {
        mainCamera = Camera.main;
        lastRenderedPosition = mainCamera.transform.position;
    }

    void LateUpdate()
    {
        cameraExtent = Camera.main.GetExtent(maskingMaxDistance);

        if (!cameraExtent.Equals(lastCameraExtent))
        {
            CameraChanged();
        }
#if UNITY_EDITOR
        else
        {
            CameraChanged();
        }
#endif
    }

    private void CameraChanged()
    {
        lastCameraExtent = cameraExtent;

        renderCamera.orthographicSize = (float)cameraExtent.Width / 2.0f;

        this.transform.position = new Vector3((float)lastCameraExtent.CenterX,offset, (float)lastCameraExtent.CenterY);
        renderCamera.Render();
        UpdateMaterialBounds();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(new Vector3((float)cameraExtent.CenterX, offset/2.0f, (float)cameraExtent.CenterY), new Vector3((float)cameraExtent.Width,offset, (float)cameraExtent.Height));
    }

    private void UpdateMaterialBounds()
    {
        //We do this square so we do not need dynamic rendertexture
        var scale = (float)cameraExtent.Width * (1.0f + (2.0f / textureSize));
        maskTiling.Set(1.0f/(float)cameraExtent.Width, 1.0f/(float)cameraExtent.Width);
        maskOffset.Set((-(float)cameraExtent.CenterX / scale) + 0.5f, (-(float)cameraExtent.CenterY / scale) + 0.5f);

        foreach(var material in targetMaterials)
        {
            material.SetVector("MaskOffset", maskOffset);
            material.SetVector("MaskTiling", maskTiling);
        }
    }
}
