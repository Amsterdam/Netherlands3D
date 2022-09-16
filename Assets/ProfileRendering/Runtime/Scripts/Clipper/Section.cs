using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Section
{
    public Vector2 startpoint2D;
    public Vector2 endpoint2D;
    public float startelevation = 0;
    public float endelevation = 0;
    public SectionDirection sectionDirection = SectionDirection.horizontal;

    private List<float> distancesFromStart = new List<float>();

    public List<Intersection> Intersections = new List<Intersection>();

    private float calculateDistanceFormStart(Intersection intersection)
    {
        return (intersection.point - startpoint2D).magnitude;
    }

    public void AddIntersection(Intersection newIntersection)
    {

        float distancefromstart=0;
        switch (sectionDirection)
        {
            case SectionDirection.up:
                distancefromstart = newIntersection.elevation;
                break;
            case SectionDirection.down:
                distancefromstart = 0 - newIntersection.elevation;
                break;
            case SectionDirection.horizontal:
                distancefromstart = calculateDistanceFormStart(newIntersection);
                break;
            default:
                break;
        }
        
        if (Intersections.Count==0)
        {
            Intersections.Add(newIntersection);
            distancesFromStart.Add(distancefromstart);
            return;
        }

        for (int i = 0; i < Intersections.Count; i++)
        {
            if (distancesFromStart[i]>distancefromstart)
            {
                Intersections.Insert(i, newIntersection);
                distancesFromStart.Insert(i, distancefromstart);
                return;
            }
        }
        
        Intersections.Add(newIntersection);
        distancesFromStart.Add(distancefromstart);
    }

    public void ResetIntersections()
    {
        Intersections.Clear();
        distancesFromStart.Clear();
    }
    public void Clear()
    {
        startelevation = 0;
        endelevation = 0;
        ResetIntersections();

    }
    public enum SectionDirection
    {
        up,
        down,
        horizontal
    }
}
public class SectionPool
{
    private Queue<Section> SectionQueue = new Queue<Section>();
    public Section getSection()
    {
        Section result;
        if (SectionQueue.TryDequeue(out result))
        {
            return result;
        }
        return new Section();
    }

    public void returnSection(Section section)
    {
        section.Clear();
        SectionQueue.Enqueue(section);
    }
}
public enum SectionDirection
{
    up,
    down,
    horizontal
}

