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
    private Color[] linePixels;
    private Color[] outputPixels;

    void Awake()
    {
        sliceCamera = GetComponent<Camera>();
        sliceCamera.enabled = false;
    }

    private void OnValidate()
    {
        if(!sliceCamera) sliceCamera = GetComponent<Camera>();
        sliceCamera.enabled = false;

        sliceCamera.orthographicSize = 0.5f;
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

        outputTexture = new Texture2D(width, slicesHeight, TextureFormat.R8, false);
        lineTexture = new Texture2D(width, 1, TextureFormat.R8, false);

        outputPixels = new Color[width * slicesHeight];

        rawImagePreview.texture = outputTexture;
        rawImageLine.texture = lineTexture;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1,0,0,0.5f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero + (Vector3.forward* (depthFromCamera/2.0f)), new Vector3(width, 0, depthFromCamera));
    }

    private IEnumerator SliceLoop()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            sliceCamera.nearClipPlane = 0;
            sliceCamera.farClipPlane = 0;

            var startRotation = sliceCamera.transform.rotation;
            var startPosition = sliceCamera.transform.position;

            //From topdown
            for (int i = 0; i < slicesHeight; i++)
            {
                //Render line
                sliceCamera.Render();

                RenderTexture.active = lineRenderTexture;
                lineTexture.ReadPixels(rect, 0, 0, false);
                lineTexture.Apply();
                linePixels = lineTexture.GetPixels();

                float startClip = (float)i * ((float)depthFromCamera / (float)slicesHeight);
                sliceCamera.nearClipPlane = startClip;
                sliceCamera.farClipPlane = startClip + (depthFromCamera/ (float)slicesHeight); //1meter per slice for now

                //for every pixel draw one in our map
                outputTexture.SetPixels(0, (slicesHeight-1) - i, width, 1, linePixels);
                outputTexture.Apply();

                if (waitBetweenLines > 0)
                    yield return new WaitForSeconds(waitBetweenLines);
            }

            //Turn to side
            sliceCamera.transform.Translate(Vector3.forward * (depthFromCamera / 2.0f),Space.Self);
            sliceCamera.transform.Rotate(0, 90, 0, Space.Self);
            sliceCamera.transform.Translate(Vector3.back * (width / 2.0f), Space.Self);

            for (int i = 0; i < slicesHeight; i++)
            {
                //Render line
                sliceCamera.Render();

                RenderTexture.active = lineRenderTexture;
                lineTexture.ReadPixels(rect, 0, 0, false);
                lineTexture.Apply();
                linePixels = lineTexture.GetPixels();

                float startClip = (float)i * ((float)depthFromCamera / (float)slicesHeight);

                sliceCamera.nearClipPlane = startClip;
                sliceCamera.farClipPlane = startClip + (depthFromCamera / (float)slicesHeight); //1meter per slice for now

                //Keep existing pixels
                var existingPixels = outputTexture.GetPixels(i, 0, 1, width);
                for (int j = 0; j < linePixels.Length; j++)
                {
                    if (existingPixels[j].r > 0)
                        linePixels[j] = existingPixels[j];
                }

                //for every pixel draw one in our map
                outputTexture.SetPixels(i, 0, 1, width, linePixels);
                outputTexture.Apply();

                if(waitBetweenLines > 0)
                    yield return new WaitForSeconds(waitBetweenLines);
            }

            sliceCamera.transform.position = startPosition;
            sliceCamera.transform.rotation = startRotation;

            RenderTexture.active = null;
        }
    }
}
