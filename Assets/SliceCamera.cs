using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliceCamera : MonoBehaviour
{
    [SerializeField] int slices = 512;
    [SerializeField] RenderTexture lineRenderTexture;

    private Camera sliceCamera;

    private Texture2D lineTexture;

    private Texture2D outputTexture;
    public RawImage rawImagePreview;
    private Rect rect = new Rect(0,0,512,1);
    void Start()
    {
        sliceCamera = GetComponent<Camera>();
        sliceCamera.enabled = false;

        outputTexture = new Texture2D(512,512, TextureFormat.ARGB32, false);
        lineTexture = new Texture2D(512,1, TextureFormat.ARGB32, false);

        rawImagePreview.texture = lineTexture;
    }

    [ContextMenu("Slice")]
    public void RenderClices()
    {
        StartCoroutine(SliceLoop());
    }

    private IEnumerator SliceLoop()
    {
        yield return new WaitForEndOfFrame();
        sliceCamera.nearClipPlane = 0;
        sliceCamera.farClipPlane = 0;

        for (int i = 0; i < slices; i++)
        {
            RenderTexture.active = lineRenderTexture;
            //Render line
            sliceCamera.Render();
            yield return new WaitForEndOfFrame();
            lineTexture.ReadPixels(rect,0,0);
            lineTexture.Apply();

            var linePixels = lineTexture.GetPixels();

            sliceCamera.nearClipPlane = i;
            sliceCamera.farClipPlane = i + 1; //1meter per slice for now

            //for every pixel draw one in our map
            outputTexture.SetPixels(0, i, 512, 1, linePixels);

            outputTexture.Apply();

            RenderTexture.active = null;
        }
    }

    void Update()
    {
        
    }
}
