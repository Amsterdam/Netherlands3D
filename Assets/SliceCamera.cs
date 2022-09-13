using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliceCamera : MonoBehaviour
{
    [SerializeField] int slicesHeight = 512;
    [SerializeField] int depthFromCamera = 200;
    [SerializeField] int width = 512;
    [SerializeField] float waitBetweenLines = 0.2f;
    private RenderTexture lineRenderTexture;

    private Camera sliceCamera;

    [SerializeField] private Texture2D lineTexture;

    private Texture2D outputTexture;
    public RawImage rawImagePreview;
    public RawImage rawImageLine;
    private Rect rect;
    void Awake()
    {
        sliceCamera = GetComponent<Camera>();
        sliceCamera.enabled = false;
    }

    [ContextMenu("Slice")]
    public void RenderClices()
    {
        CreateTextures();

        rect = new Rect(0, 0, width, 1);

        StopAllCoroutines();
        StartCoroutine(SliceLoop());
    }

    private void CreateTextures()
    {
        if (lineRenderTexture) Destroy(lineRenderTexture);
        if (outputTexture) Destroy(outputTexture);
        if (lineTexture) Destroy(lineTexture);

        lineRenderTexture = new RenderTexture(width, 1, 0);
        sliceCamera.targetTexture = lineRenderTexture;

        outputTexture = new Texture2D(width, slicesHeight, TextureFormat.ARGB32, false);
        lineTexture = new Texture2D(width, 1, TextureFormat.ARGB32, false);

        rawImagePreview.texture = outputTexture;
        rawImageLine.texture = lineTexture;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(this.transform.position, this.transform.forward * depthFromCamera);
    }

    private IEnumerator SliceLoop()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            sliceCamera.nearClipPlane = 0;
            sliceCamera.farClipPlane = 0;

            for (int i = 0; i < slicesHeight; i++)
            {
                //Render line
                sliceCamera.Render();

                RenderTexture.active = lineRenderTexture;
                lineTexture.ReadPixels(rect, 0, 0, false);
                lineTexture.Apply();

                var linePixels = lineTexture.GetPixels();

                float startClip = (float)i * ((float)depthFromCamera / (float)slicesHeight);

                sliceCamera.nearClipPlane = startClip;
                sliceCamera.farClipPlane = startClip + (depthFromCamera/ (float)slicesHeight); //1meter per slice for now

                //for every pixel draw one in our map
                outputTexture.SetPixels(0, (slicesHeight-1) - i, width, 1, linePixels);
                outputTexture.Apply();

                //yield return new WaitForSeconds(waitBetweenLines);
            }
            RenderTexture.active = null;
        }
    }
}
