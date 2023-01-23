using Netherlands3D.B3DM;
using System;
using UnityEngine;

[System.Serializable]
public class Content: MonoBehaviour, IDisposable
{
    public string uri;

    public GameObject contentGameObject;
    private Coroutine runningContentRequest;

    public void Load()
    {
        runningContentRequest = StartCoroutine(ImportB3DMGltf.ImportBinFromURL(uri, GotContent));
    }

    private void GotContent(GameObject contentGameObject)
    {
        this.contentGameObject = contentGameObject;
    }

    public void Dispose()
    {
        if (runningContentRequest != null)
            StopCoroutine(runningContentRequest);

        if (contentGameObject)
            Destroy(contentGameObject);
    }
}



