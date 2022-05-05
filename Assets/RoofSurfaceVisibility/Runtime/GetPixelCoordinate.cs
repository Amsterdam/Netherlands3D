using System;
using System.Collections;
using UnityEngine;

public class GetPixelCoordinate : MonoBehaviour
{
    [SerializeField]
    private RenderTexture renderTexture;

    private Rect rect;
    Texture2D readPixelTexture;

    [SerializeField]
    private Camera pixelCamera;

    private Texture2D topDownTexture;

    [SerializeField]
    private float maxViewDistance = 1000;

    [SerializeField]
    private int projectionTextureSize = 512;

    [SerializeField]
    private Material projectorMaterial;

    [SerializeField]
    private Color visibleColor;

    [SerializeField]
    private Color notVisibleColor;

    [SerializeField]
    Transform projector;

    [SerializeField]
    private int maxPixelsPerFrame = 100;

    [SerializeField]
    private bool splitDirectionsOverFrames = false;

    void Start()
    {
        rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
        readPixelTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        topDownTexture = new Texture2D(projectionTextureSize, projectionTextureSize, TextureFormat.RGB24,false,true);

        projectorMaterial.mainTexture = topDownTexture;

        StartCoroutine(DrawLoop());
    }

    IEnumerator DrawLoop()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            this.transform.rotation = Quaternion.identity;

            Color[] topDownPixels = topDownTexture.GetPixels();
            ApplyBaseColors(topDownPixels);

            for (int i = 0; i < 5; i++)
            {
                RenderTexture.active = renderTexture;
                pixelCamera.Render();
                readPixelTexture.ReadPixels(rect, 0, 0);
                RenderTexture.active = null;

                yield return DrawTopDownVisibilityPixels(topDownPixels);

                switch (i)
                {
                    case 0:
                        this.transform.forward = Vector3.back;
                        break;
                    case 1:
                        this.transform.forward = Vector3.down;
                        break;
                    case 2:
                        this.transform.forward = Vector3.left;
                        break;
                    case 3:
                        this.transform.forward = Vector3.right;
                        break;
                    case 4:
                        this.transform.forward = Vector3.forward;
                        break;
                }        
                if(splitDirectionsOverFrames) yield return new WaitForEndOfFrame();
            }

            topDownTexture.Apply();
            yield return new WaitForEndOfFrame();
        }
    }

    private void LateUpdate()
    {
        projector.rotation = Quaternion.Euler(90,0,0);
    }

    private IEnumerator DrawTopDownVisibilityPixels(Color[] topDownPixels)
    {
        yield return new WaitForEndOfFrame();

        //Draw visibile pixels
        Color[] positionPixels = readPixelTexture.GetPixels();
		for (int i = 0; i < positionPixels.Length; i++)
		{
            var rgbPosition = positionPixels[i];

            int pixelRow = (int)((rgbPosition.b) * (projectionTextureSize-1));
            int pixelColumn = (int)(rgbPosition.r * (projectionTextureSize-1));
            int pixelIndex = (pixelRow * projectionTextureSize) + pixelColumn;

            topDownPixels[pixelIndex] = visibleColor;
        }
        topDownTexture.SetPixels(topDownPixels);
    }

    private void ApplyBaseColors(Color[] pixels)
    {
        //Set base color
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = notVisibleColor;
        }
    }
}
