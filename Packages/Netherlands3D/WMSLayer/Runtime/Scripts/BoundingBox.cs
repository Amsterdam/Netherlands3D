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

    public static bool operator == (BoundingBox a, BoundingBox b)
    {
        return a.MinX == b.MinX && a.MinY == b.MinY && a.MaxX == b.MaxX && a.MaxY == b.MaxY;
    }

    public static bool operator != (BoundingBox a, BoundingBox b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if(!(obj is BoundingBox))
        {
            return false;
        }
        return this == (BoundingBox)obj;
    }

    public override int GetHashCode() => (MinX, MinY, MaxX, MaxY).GetHashCode();

    public override string ToString()
    {
        return $"BBOX: min x:{MinX}, min y: {MinY}, max x: {MaxX}, max y: {MaxY}";
    }

}
