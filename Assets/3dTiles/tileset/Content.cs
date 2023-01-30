using Netherlands3D.B3DM;
using System;
using UnityEngine;

[System.Serializable]
public class Content : MonoBehaviour, IDisposable
{
    public string uri = "";

    public GameObject contentGameObject;
    private Coroutine runningContentRequest;

    private Tile parentTile;
    public Tile ParentTile { get => parentTile; set => parentTile = value; }

    public enum ContentLoadState{
        NOTLOADED,
        LOADING,
        READY,
    }
    public ContentLoadState state = ContentLoadState.NOTLOADED;

    /// <summary>
    /// Draw wire cube in editor with bounds and color coded state
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (ParentTile == null) return;

        Color color = Color.white;
        switch (state)
        {
            case ContentLoadState.NOTLOADED:
                color = Color.red;
                break;
            case ContentLoadState.LOADING:
                color = Color.yellow;
                break;
            case ContentLoadState.READY:
                color = Color.green;
                break;
            default:
                break;
        }

        Gizmos.color = color;
        var parentTileBounds = ParentTile.Bounds;
        Gizmos.DrawWireCube(parentTileBounds.center, parentTileBounds.size);
    }

    /// <summary>
    /// Load the content from an url
    /// </summary>
    public Coroutine Load()
    {
        state = ContentLoadState.LOADING;
        runningContentRequest = StartCoroutine(ImportB3DMGltf.ImportBinFromURL(uri, GotContent));
        return runningContentRequest;
    }

    private void GotContent(GameObject contentGameObject)
    {
        state = ContentLoadState.READY;
        if (contentGameObject != null)
        {
            this.contentGameObject = contentGameObject;
            this.contentGameObject.transform.SetParent(this.gameObject.transform, true);
            this.contentGameObject.transform.localRotation = Quaternion.identity;
            this.contentGameObject.transform.localPosition = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("Could not load GameObject");
        }
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

        Destroy(this);
    }
}



