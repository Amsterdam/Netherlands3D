﻿using Netherlands3D.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile : IDisposable
{
    public int X;
    public int Y;
    public int Z;
    public bool hascontent;

    public int childrenCountDelayingDispose = 0;
    public Tile parent;

    public List<Tile> children = new List<Tile>();

    public double[] transform;
    public float geometricError;
    public float screenSpaceError;

    public string refine;
    public BoundingVolume boundingVolume;
    public string contentUri;
    public Content content;

    public int priority = 0;

    private bool boundsAvailable = false;
    private Bounds bounds = new Bounds();
    
    public bool requestedDispose = false;
    public bool requestedUpdate = false;

    public Bounds ContentBounds 
    { 
        get {
            return bounds;
        } 
        set => bounds = value; 
    }

    public Vector3 EulerRotationToVertical()
    {
        float posX = (float)(transform[12] / 1000); // measured for earth-center to prime meridian (greenwich)
        float posY = (float)(transform[13] / 1000); // measured from earth-center to 90degrees east at equator
        float posZ = (float)(transform[14] / 1000); // measured from earth-center to nothpole

        float angleX = -Mathf.Rad2Deg * Mathf.Atan(posY / posZ);
        float angleY = -Mathf.Rad2Deg * Mathf.Atan(posX / posZ);
        float angleZ = -Mathf.Rad2Deg * Mathf.Atan(posY / posX);
        Vector3 result = new Vector3(angleX, angleY, angleZ);
        return result;
    }

    public Quaternion RotationToVertical()
    { 
        float posX = (float)(transform[12]/1000000); // measured for earth-center to prime meridian (greenwich)
        float posY = (float)(transform[13]/1000000); // measured from earth-center to 90degrees east at equator
        float posZ = (float)(transform[14]/1000000); // measured from earth-center to nothpole

        Quaternion rotation= Quaternion.FromToRotation(new Vector3(posX, posY, posZ), new Vector3(0, 0, 1));
       
        return rotation;
    }

    public int GetChildCount()
    {
        int childcount = 1;
        foreach (var child in children)
        {
            childcount += child.GetChildCount();
        }
        return childcount;
    }

    public int GetNestingDepth()
    {
        int maxDepth = 1;
        foreach (var child in children)
        {
            int depth = child.GetNestingDepth()+1;
            if (depth > maxDepth) maxDepth = depth;

        }
        return maxDepth;
    }

    public enum TileStatus
    {
        unloaded,
        loaded
    }

    public bool IsInViewFrustrum(Camera ofCamera)
    {
        if (!boundsAvailable) CalculateBounds();

        return ofCamera.InView(ContentBounds);
    }

    public void CalculateBounds()
    {
        //Array order: west, south, east, north, minimum height, maximum height
        var ecefMin = CoordConvert.WGS84toECEF(new Vector3WGS((boundingVolume.values[0] * 180.0f) / Mathf.PI, (boundingVolume.values[1] * 180.0f) / Mathf.PI, boundingVolume.values[4]));
        var ecefMax = CoordConvert.WGS84toECEF(new Vector3WGS((boundingVolume.values[2] * 180.0f) / Mathf.PI, (boundingVolume.values[3] * 180.0f) / Mathf.PI, boundingVolume.values[5]));

        var unityMin = CoordConvert.ECEFToUnity(ecefMin);
        var unityMax = CoordConvert.ECEFToUnity(ecefMax);

        bounds.size = Vector3.zero;
        bounds.center = unityMin;
        bounds.Encapsulate(unityMax);

        boundsAvailable = true;
    }

    public void Dispose()
    {
        if (content != null)
        {
            content.Dispose();
            content = null;
        }
    }
}