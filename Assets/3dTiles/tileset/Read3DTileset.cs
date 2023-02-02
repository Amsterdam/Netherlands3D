using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using System;
using Netherlands3D.Core;

[RequireComponent(typeof(ReadSubtree))]
public class Read3DTileset : MonoBehaviour
{
    public string tilesetUrl = "https://storage.googleapis.com/ahp-research/maquette/kadaster/3dbasisvoorziening/test/landuse_1_1/tileset.json";
    private string absolutePath = "";

    public Tile root;
    public double[] transformValues;
    private Vector3ECEF positionECEF;

    TilingMethod tilingMethod = TilingMethod.explicitTiling;

    public ImplicitTilingSettings implicitTilingSettings;

    public int tileCount;
    public int nestingDepth;

    public GameObject cubePrefab;

    public int maxPixelError = 5;
    private float sseComponent = -1;

    void Start()
    {
        absolutePath = tilesetUrl.Replace("tileset.json", "");
        StartCoroutine(LoadTileset());

        CoordConvert.relativeOriginChanged.AddListener(RelativeCenterChanged);
    }

    private void RelativeCenterChanged(Vector3 cameraOffset)
    {
        //Point set up from new origin
        AlignWithUnityWorld();

        //Flag all calculated bounds to be recalculated when tile bounds is requested
        RecalculateAllTileBounds(root);
    }

    /// <summary>
    /// Recursive recalculation of tile bounds
    /// </summary>
    /// <param name="tile">Starting tile</param>
    private void RecalculateAllTileBounds(Tile tile)
    {
        if (tile == null) return;

        tile.CalculateBounds();

        foreach (var child in tile.children)
        {
            RecalculateAllTileBounds(child);
        } 
    }

    IEnumerator LoadTileset()
    {
        UnityWebRequest www = UnityWebRequest.Get(tilesetUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            string jsonstring = www.downloadHandler.text;

            JSONNode rootnode = JSON.Parse(jsonstring)["root"];
            ReadTileset(rootnode);
        }
        
    }

    private void LoadChildContent(Tile child)
    {
        if(!child.content)
        {
            child.content = gameObject.AddComponent<Content>();
            child.content.ParentTile = child;
            child.content.uri = absolutePath + implicitTilingSettings.contentUri.Replace("{level}", child.X.ToString()).Replace("{x}", child.Y.ToString()).Replace("{y}", child.Z.ToString());
            child.content.Load();
        }
    }

    private void ReadTileset(JSONNode rootnode)
    {
        JSONNode transformNode = rootnode["transform"];
        transformValues = new double[16];
        for (int i = 0; i < 16; i++)
        {
            transformValues[i] = transformNode[i].AsDouble;
        }
        JSONNode implicitTilingNode = rootnode["implicitTiling"];
        if (implicitTilingNode != null)
        {
            ReadImplicitTiling(rootnode);
        }

        //setup location and rotation
        positionECEF = new Vector3ECEF(transformValues[12], transformValues[13], transformValues[14]);
        AlignWithUnityWorld();
    }

    private void AlignWithUnityWorld()
    {
        transform.position = CoordConvert.ECEFToUnity(positionECEF);
        transform.rotation = CoordConvert.ecefRotionToUp();
    }

    private void ReadImplicitTiling(JSONNode rootnode)
    {
        tilingMethod = TilingMethod.implicitTiling;
        implicitTilingSettings = new ImplicitTilingSettings();
        string refine = rootnode["refine"].Value;
        switch (refine)
        {
            case "REPLACE":
                implicitTilingSettings.refinementtype = refinementType.Replace;
                break;
            case "ADD":
                implicitTilingSettings.refinementtype = refinementType.Add;
                break;
            default:
                break;
        }
        implicitTilingSettings.geometricError = rootnode["geometricError"].AsFloat;
        implicitTilingSettings.boundingRegion = new double[6];
        for (int i = 0; i < 6; i++)
        {
            implicitTilingSettings.boundingRegion[i] = rootnode["boundingVolume"]["region"][i].AsDouble;
        }
        implicitTilingSettings.contentUri = rootnode["content"]["uri"].Value;
        JSONNode implicitTilingNode = rootnode["implicitTiling"];
        string subdivisionScheme = implicitTilingNode["subsivisionScheme"].Value;
        switch (subdivisionScheme)
        {
            case "QUADTREE":
                implicitTilingSettings.subdivisionScheme = Subdivisionscheme.Quadtree;
                break;
            default:
                implicitTilingSettings.subdivisionScheme = Subdivisionscheme.Octree;
                break;
        }
        implicitTilingSettings.subtreeLevels = implicitTilingNode["subtreeLevels"];
        implicitTilingSettings.subtreeUri = implicitTilingNode["subtrees"]["uri"].Value;

        ReadSubtree subtreeReader = GetComponent<ReadSubtree>();
        string subtreeURL = tilesetUrl.Replace("tileset.json", implicitTilingSettings.subtreeUri)
                            .Replace("{level}", "0")
                            .Replace("{x}", "0")
                            .Replace("{y}", "0");

        Debug.Log("Load subtree: " + subtreeURL);
        subtreeReader.DownloadSubtree(subtreeURL, implicitTilingSettings, ReturnTiles);
    }

    public void ReturnTiles(Tile rootTile)
    {
        root = rootTile;
        StartCoroutine(LoadInView());
    }

    /// <summary>
    /// Check what tiles should be loaded/unloaded based on view
    /// </summary>
    private IEnumerator LoadInView()
    {
        yield return new WaitForEndOfFrame();
        var currentMainCamera = Camera.main;

        while (true)
        {
            SetSSEComponent(currentMainCamera);
            LoadInViewRecursively(root, currentMainCamera);

            yield return new WaitForEndOfFrame();
        }
    }

    private void LoadInViewRecursively(Tile tile, Camera currentCamera)
    {
        foreach (var child in tile.children)
        {
            var closestPointOnBounds = child.Bounds.ClosestPoint(currentCamera.transform.position); //Returns original point when inside the bounds
            var pixelError = (sseComponent * child.geometricError) / Vector3.Distance(currentCamera.transform.position, closestPointOnBounds);

            if (pixelError > maxPixelError && child.IsInViewFrustrum(currentCamera))
            {
                LoadChildContent(child);
            }
            else if (child.geometricError <= sseComponent && child.content)
            {
                child.Dispose();
            }
            LoadInViewRecursively(child, currentCamera);
        }
    }

    /// <summary>
    /// Screen-space error component calculation
    /// </summary>
    public void SetSSEComponent(Camera currentCamera)
    {
        sseComponent = Screen.height / (2 * Mathf.Tan(Mathf.Deg2Rad * currentCamera.fieldOfView / 2));
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor only methods for loading all tiles from context menu
    /// </summary>
    [ContextMenu("Load all content")]
    private void LoadAll()
    {
        StartCoroutine(LoadAllTileContent());
    }
    private IEnumerator LoadAllTileContent()
    {
        yield return new WaitForEndOfFrame();
        yield return LoadContentInChildren(root);
    }
    private IEnumerator LoadContentInChildren(Tile tile)
    {
        foreach (var child in tile.children)
        {
            if (child.hascontent)
            {
                LoadChildContent(child);
            }
            yield return new WaitForEndOfFrame();
            yield return LoadContentInChildren(child);
        }
    }
#endif 
}

public enum TilingMethod
{
    explicitTiling,
    implicitTiling
}

public enum refinementType
{
    Replace,
    Add
}
public enum Subdivisionscheme
{
    Quadtree,
    Octree
}

[System.Serializable]
public class ImplicitTilingSettings
{
    public refinementType refinementtype;
    public Subdivisionscheme subdivisionScheme;
    public int subtreeLevels;
    public string subtreeUri;
    public string contentUri;
    public float geometricError;
    public double[] boundingRegion;
}