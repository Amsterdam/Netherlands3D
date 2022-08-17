using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Intersection
{
    public Vector2 point;
    public float elevation;
    public intersectionDirection direction;
    public int pointIndexOnTriangle;
    public int pointIndexOnBoundary;

    
}

public class IntersectionPool
{
    private Queue<Intersection> IntersectionQueue = new Queue<Intersection>();
    public Intersection getIntersection()
    {
        Intersection result;
        if (IntersectionQueue.TryDequeue(out result))
        {
            return result;
        }
        return new Intersection();
    }

    public void returnIntersection(Intersection intersection)
    {

        IntersectionQueue.Enqueue(intersection);
    }
    
}
