using Netherlands3D.B3DM;
using System;
using UnityEngine;

[System.Serializable]
public class Content : MonoBehaviour, IDisposable
{
    public string uri;

    public GameObject contentGameObject;
    private Coroutine runningContentRequest;

    public enum ContentLoadState{
        NOTLOADED,
        LOADING,
        READY,
    }
    public ContentLoadState state = ContentLoadState.NOTLOADED;

    /// <summary>
    /// Load the content from an url
    /// </summary>
    public void Load()
    {
        state = ContentLoadState.LOADING;

        runningContentRequest = StartCoroutine(ImportB3DMGltf.ImportBinFromURL(uri, GotContent));
    }

    private void GotContent(GameObject contentGameObject)
    {
        state = ContentLoadState.READY;

        this.contentGameObject = contentGameObject;
    }

    /// <summary>
    /// Clean up coroutines and content gameobjects
    /// </summary>
    public void Dispose()
    {
        state = ContentLoadState.NOTLOADED;

        if (runningContentRequest != null)
            StopCoroutine(runningContentRequest);

        if (contentGameObject)
            Destroy(contentGameObject);
    }
}



