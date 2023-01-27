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
    TilingMethod tilingMethod = TilingMethod.explicitTiling;

    public ImplicitTilingSettings implicitTilingSettings;

    public int tileCount;
    public int nestingDepth;

    public GameObject cubePrefab;

    private float sseComponent = -1;

    void Start()
    {
        absolutePath = tilesetUrl.Replace("tileset.json", "");
        StartCoroutine(LoadTileset());

        CoordConvert.relativeCenterChanged.AddListener(RecalculateUnityBounds);
    }

    private void RecalculateUnityBounds(Vector3 newCenter, Quaternion newRotation)
    {
        //Flag all calculated bounds to be recalculated when tile bounds is requested
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
            if(child.hascontent)
            {
                LoadChildContent(child);
            }
            yield return new WaitForEndOfFrame();
            yield return LoadContentInChildren(child);
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
        Vector3ECEF positionECEF = new Vector3ECEF(transformValues[12], transformValues[13], transformValues[14]);
        transform.position = CoordConvert.ECEFToUnity(positionECEF);
        transform.rotation = CoordConvert.ecefRotionToUp();

        //Add automatic follower
        gameObject.AddComponent<MovingOriginFollower>();

        //Vector3WGS positionWGS = CoordConvert.ECEFtoWGS84(positionECEF);

        //positionWGS = ConvertEcef.Coord.ecef_to_geo(new Vector3RD(positionECEF.X, positionECEF.Y, positionECEF.Z));
        //Vector3 position = new Vector3((float)(positionECEF.Y - CoordConvert.relativeCEnterECEF.Y), (float)(positionECEF.Z - CoordConvert.relativeCEnterECEF.Z), (float)(-positionECEF.X + CoordConvert.relativeCEnterECEF.X));
        //Vector3 position = new Vector3(-(float)(positionECEF.X - CoordConvert.relativeCEnterECEF.X), (float)(positionECEF.Z - CoordConvert.relativeCEnterECEF.Z), -(float)(positionECEF.Y - CoordConvert.relativeCEnterECEF.Y));


        //Vector3 rotation = CoordConvert.RotationToUnityUP(CoordConvert.UnitytoWGS84(Vector3.zero));

        //Vector3 newposition = Quaternion.Euler(rotation)*position;
        //transform.position = position;
        //transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
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

        while (true)
        {
            SetSSEComponent();
            LoadInViewRecursively(root);

            yield return new WaitForEndOfFrame();
        }
    }

    private void LoadInViewRecursively(Tile tile)
    {
        foreach (var child in tile.children)
        {
            var tileSSEInPixels = (sseComponent * child.geometricError) / Vector3.Distance(Camera.main.transform.position, tile.Bounds.center);
            if(true)
            //if (tileSSEInPixels > child.geometricError && child.IsInViewFrustrum())
            {
                LoadChildContent(child);
            }
            else if (child.geometricError <= sseComponent && child.content)
            {
                child.Dispose();
            }
            LoadInViewRecursively(child);
        }
    }

    public void SetSSEComponent()
    {
        sseComponent = Screen.height / (2 * Mathf.Tan(Mathf.Deg2Rad * Camera.main.fieldOfView / 2));

        // multiply with Geomettric Error and
        // divide by distance to camera
        // to get the screenspaceError in pixels;
    }
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