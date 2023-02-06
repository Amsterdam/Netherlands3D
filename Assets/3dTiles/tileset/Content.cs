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
    private Coroutine runningContentRequest;

    private Tile parentTile;
    public Tile ParentTile { get => parentTile; set => parentTile = value; }

    public UnityEvent doneLoading = new UnityEvent();

    public enum ContentLoadState{
        NOTLOADED,
        LOADING,
        READY,
    }
    private ContentLoadState state = ContentLoadState.NOTLOADED;
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
        State = ContentLoadState.LOADING;
        runningContentRequest = StartCoroutine(ImportB3DMGltf.ImportBinFromURL(uri, GotContent));
        return runningContentRequest;
    }

    private void GotContent(GameObject contentGameObject)
    {
        State = ContentLoadState.READY;
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

        doneLoading.Invoke();
    }

    /// <summary>
    /// Clean up coroutines and content gameobjects
    /// </summary>
    public void Dispose()
    {
        doneLoading.RemoveAllListeners();

        State = ContentLoadState.NOTLOADED;

        if (runningContentRequest != null)
            StopCoroutine(runningContentRequest);

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



