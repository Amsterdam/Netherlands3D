using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BoundingBox
{
    public int MinX;
    public int MinY;
    public int MaxX;
    public int MaxY;

    public static readonly BoundingBox Zero = new BoundingBox(0, 0, 0, 0);

    public BoundingBox(int minX, int minY, int maxX, int maxY)
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
