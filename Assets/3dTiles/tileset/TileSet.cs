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
    
    public void SetRotationMatrix()
    {
        Matrix4x4 rotationMatrix = new Matrix4x4();
        //Vector4 col0 = new Vector4(1, 0, 0, 0);
        //Vector4 col1 = new Vector4(0, 1, 0, 0);
        //Vector4 col2 = new Vector4(0, 0, 1, 0);
        //Vector4 col3 = new Vector4(3889257.5313360146f, 332923.5169855651f, 5027059.006517078f, 1.0f);
        //Matrix4x4 tm = new Matrix4x4(col0, col1, col2, col3);
    }

    public void GetNextTileToLoad(Plane[] viewFrustrum,Vector3 cameraPosition,float sseComponent)
    {

    }

}

[System.Serializable]
public class Tile
{
    public int X;
    public int Y;
    public int Z;
    public List<Tile> children = new List<Tile>();
    public double[] transform;
    public float geometricError;
    public string refine;
    public BoundingVolume boundingVolume;
    public Content content;
    TileStatus status= TileStatus.unloaded;
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

    public int getChildCount()
    {
        int childcount = 1;
        foreach (var child in children)
        {
            childcount += child.getChildCount();
        }
        return childcount;
    }

    public int getNestingDepth()
    {
        int maxDepth = 1;
        foreach (var child in children)
        {
            int depth = child.getNestingDepth()+1;
            if (depth > maxDepth) maxDepth = depth;

        }
        return maxDepth;
    }

    enum TileStatus
    {
        unloaded,
        loaded
    }

    bool isInViewFrustrum(Plane[] viewFrustrum)
    {


        return false;
    }
    void DefineBounds()
    {
       
        boundsIsDefined = true;
    }

}

[System.Serializable]
public class BoundingVolume
{
    public float[] box;
    ConvertCoordinates.Vector3RD positionRD;

    public ConvertCoordinates.Vector3RD PositionRD()
    {
        positionRD = new ConvertCoordinates.Vector3RD(box[0],box[2],box[1]);
        return positionRD;
    }

    public Vector3 Position()
    {
        return new Vector3(box[0], box[1], box[2]);
    }

    public Vector3 Size()
    {
        Vector3 scale = new Vector3();
        //    float sizeX = (float)Math.Sqrt(Math.Pow(box[3], 2) + Math.Pow(box[3], 2) + Math.Pow(box[3], 2));
        float sizeX = new Vector3(box[3], box[4], box[5]).magnitude * 2;
        scale.x = sizeX;
        float sizeY = new Vector3(box[6], box[7], box[8]).magnitude * 2;
        scale.y = sizeY;
        float sizeZ = new Vector3(box[9], box[10], box[11]).magnitude * 2;
        scale.z = sizeZ;

        return scale;
    }

    //public Bounds bounds()
    //{
    //    Bounds result = new Bounds();

    //}

}

[System.Serializable]
public class Content
{
    public string uri;
}



