using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BoundingBox
{
    public float MinX;
    public float MinY;
    public float MaxX;
    public float MaxY;

    public static readonly BoundingBox Zero = new BoundingBox(0, 0, 0, 0);

    public BoundingBox(float minX, float minY, float maxX, float maxY)
    {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
    }

    public override string ToString()
    {
        return $"BBOX: min x:{MinX}, min y: {MinY}, max x: {MaxX}, max y: {MaxY}";
    }

}
