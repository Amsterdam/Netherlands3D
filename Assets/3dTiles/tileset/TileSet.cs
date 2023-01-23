using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileSet 
{
    public Tile root;
    public float geometricError;
    public string filepath;
 }

[System.Serializable]
public class Tile
{
    public int X;
    public int Y;
    public int Z;
    public bool hascontent;
    public List<Tile> children = new List<Tile>();
    
    public double[] transform;
    public float geometricError;
    public string refine;
    public BoundingVolume boundingVolume;
    public Content content;
    TileStatus status = TileStatus.unloaded;
    //public importLosseB3dm b3dmImporter;
    Bounds bounds;
    bool boundsIsDefined = false;


    public Vector3 EulerRotationToVertical()
    {
        float posX = (float)(transform[12] / 1000); // measured for earth-center to prime meridian (greenwich)
        float posY = (float)(transform[13] / 1000); // measured from earth-center to 90degrees east at equator
        float posZ = (float)(transform[14] / 1000); // measured from earth-center to nothpole

        Vector3 basedirection = new Vector3(posX, posY, posZ);

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

    enum TileStatus
    {
        unloaded,
        loaded
    }

    bool IsInViewFrustrum(Plane[] viewFrustrum)
    {
        //Camera.main.GetRDExtent
    }
    void DefineBounds()
    {
       
        boundsIsDefined = true;
    }

}

[System.Serializable]
public class BoundingVolume
{
    public BoundingVolumeType boundingVolumeType;
    public double[] values;
    
}

public enum BoundingVolumeType
{
    Box,
    Sphere,
    Region
}

[System.Serializable]
public class Content
{
    public string uri;

    public GameObject loadedGameObject;
}



