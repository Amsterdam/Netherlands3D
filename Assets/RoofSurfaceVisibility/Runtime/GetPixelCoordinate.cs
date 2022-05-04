using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPixelCoordinate : MonoBehaviour
{
    [SerializeField]
    private RenderTexture renderTexture;

    private Rect rect;
    Texture2D readPixelTexture;

    Vector3 position = Vector3.zero;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private Camera pixelCamera;

    [SerializeField]
    private Texture2D topDownTexture;

    [SerializeField]
    private float maxViewDistance = 1000;

    [SerializeField]
    private int projectionTextureSize = 512;

    void Start()
    {
        rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
        readPixelTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

        topDownTexture = new Texture2D(projectionTextureSize, projectionTextureSize, TextureFormat.RGB24,false,true);
    }

    private void Update()
    {
        GetPixelLocation();
    }

    public void GetPixelLocation()
    {
        RenderTexture.active = renderTexture;
        pixelCamera.Render();
        readPixelTexture.ReadPixels(rect, 0, 0);

        RenderTexture.active = null;

        SetTopDownCoordinateInPixel();
    }

    public void SetTopDownCoordinateInPixel()
    {
        Color[] pixels = topDownTexture.GetPixels();

        Color[] positionPixels = readPixelTexture.GetPixels();
		for (int i = 0; i < positionPixels.Length; i++)
		{
            var rgbPosition = positionPixels[i];

            int pixelRow = (int)((rgbPosition.b) * (projectionTextureSize-1));
            int pixelColumn = (int)(rgbPosition.r * (projectionTextureSize-1));
            int pixelIndex = (pixelRow * projectionTextureSize) + pixelColumn;

            pixels[pixelIndex] = (rgbPosition.r == 0) ? Color.red : Color.green;
        }

        topDownTexture.SetPixels(pixels);
        topDownTexture.Apply();
    }

}
