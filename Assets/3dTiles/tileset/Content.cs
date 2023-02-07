using Netherlands3D.B3DM;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

[System.Serializable]
public class Content : MonoBehaviour, IDisposable
{
    public string uri = "";

    public GameObject contentGameObject;

    private UnityWebRequest runningWebRequest;
    private Coroutine runningContentRequest;

    private Tile parentTile;
    public Tile ParentTile { get => parentTile; set => parentTile = value; }

    public UnityEvent doneDownloading = new UnityEvent();

    public enum ContentLoadState{
        NOTLOADING,
        DOWNLOADING,
        DOWNLOADED,
    }
    private ContentLoadState state = ContentLoadState.NOTLOADING;
    public ContentLoadState State { 
        get => state;
        set
        {
            state = value;
        }
    }

    /// <summary>
    /// Draw wire cube in editor with bounds and color coded state
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (ParentTile == null) return;

        Color color = Color.white;
        switch (State)
        {
            case ContentLoadState.NOTLOADING:
                color = Color.red;
                break;
            case ContentLoadState.DOWNLOADING:
                color = Color.yellow;
                break;
            case ContentLoadState.DOWNLOADED:
                color = Color.green;
                break;
            default:
                break;
        }

        Gizmos.color = color;
        var parentTileBounds = ParentTile.Bounds;
        Gizmos.DrawWireCube(parentTileBounds.center, parentTileBounds.size);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(parentTileBounds.center, parentTileBounds.center+(ParentTile.priority * Vector3.up));
    }

    /// <summary>
    /// Load the content from an url
    /// </summary>
    public void Load()
    {
        State = ContentLoadState.DOWNLOADING;
        runningWebRequest = new UnityWebRequest();
        runningContentRequest = StartCoroutine(
            ImportB3DMGltf.ImportBinFromURL(uri, GotContent, runningWebRequest)
        );
    }

    private void GotContent(GameObject contentGameObject)
    {
        State = ContentLoadState.DOWNLOADED;
        if (contentGameObject != null)
        {
            this.contentGameObject = contentGameObject;
            this.contentGameObject.transform.SetParent(this.gameObject.transform, true);
            this.contentGameObject.transform.localRotation = Quaternion.identity;
            this.contentGameObject.transform.localPosition = Vector3.zero;
        }

        doneDownloading.Invoke();
    }

    /// <summary>
    /// Clean up coroutines and content gameobjects
    /// </summary>
    public void Dispose()
    {
        doneDownloading.RemoveAllListeners();

        //Direct abort of downloads
        if(State == ContentLoadState.DOWNLOADING)
        {
            runningWebRequest.Abort();
            StopCoroutine(runningContentRequest);
        }
        State = ContentLoadState.NOTLOADING;


        if (contentGameObject)
        {
            //TODO: Make sure GLTFast cleans up its internal stuff like textures, animations etc. It has a Dispose but might be shared lists
            //For now we do mats and meshes manualy.

            //Materials
            var renderers = contentGameObject.GetComponentsInChildren<Renderer>();
            foreach(var renderer in renderers)
            {
                var materials = renderer.sharedMaterials;
                foreach (var mat in materials)
                    Destroy(mat);
            }

            //Meshes
            var meshFilters = contentGameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
                Destroy(meshFilter.sharedMesh);

            Destroy(contentGameObject);
        }
        Destroy(this);
    }
}



