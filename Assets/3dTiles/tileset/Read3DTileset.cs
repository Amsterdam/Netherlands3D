﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using System;
using Netherlands3D.Core;

public class Read3DTileset : MonoBehaviour
{
    public string tilesetUrl = "https://storage.googleapis.com/ahp-research/maquette/kadaster/3dbasisvoorziening/test/landuse_1_1/tileset.json";

    public Tile root;
    public double[] transformValues;
    TilingMethod tilingMethod = TilingMethod.explicitTiling;

    public ImplicitTilingSettings implicitTilingSettings;

    public int tileCount;
    public int nestingDepth;

    public GameObject cubePrefab;

    void Start()
    {
        StartCoroutine(LoadTileset());

        CoordConvert.relativeCenterChanged.AddListener(CenterChanged);
    }

    private void CenterChanged(Vector3 positionOffset, Quaternion rotationOffset)
    {
        //
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
        var absolutePath = tilesetUrl.Replace("tileset.json","");

        foreach (var child in tile.children)
        {
            if(child.hascontent)
            {
                child.content = gameObject.AddComponent<Content>();
                child.content.uri = absolutePath + implicitTilingSettings.contentUri.Replace("{level}", child.X.ToString()).Replace("{x}", child.Y.ToString()).Replace("{y}", child.Z.ToString());
                child.content.Load();
            }
            yield return new WaitForEndOfFrame();
            yield return LoadContentInChildren(child);
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
        Vector3WGS positionWGS = CoordConvert.ECEFtoWGS84(positionECEF);
        Vector3 position = CoordConvert.WGS84toUnity(positionWGS);
        transform.position = position;
        Vector3 rotation = CoordConvert.RotationToUnityUP(positionWGS);
        transform.rotation = Quaternion.Euler(rotation.x,rotation.y,rotation.z);
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
        
        subtreeReader.DownloadSubtree(subtreeURL, implicitTilingSettings, ReturnTiles);
    }

    public void ReturnTiles(Tile rootTile)
    {
        root = rootTile;
    }

    public void SetSSEComponent()
    {
        float sseComponent = Screen.height / (2 * Mathf.Tan(Mathf.Deg2Rad * Camera.main.fieldOfView / 2));
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