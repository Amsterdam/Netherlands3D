using Netherlands3D.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiPolygonHandler
{
    private WFSHandler owner;
    public MultiPolygonHandler(WFSHandler owner)
    {
        this.owner = owner;
    }
    public void ProcessMultiPolygon(List<List<List<GeoJSONPoint>>> multiPolyList)
    {
        Debug.Log("Processing MultiPoly!");
        GameObject template = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        foreach (List<List<GeoJSONPoint>> pointListList in multiPolyList)
        {
            foreach (List<GeoJSONPoint> pointList in pointListList)
            {
                Vector2 pointCoords = Vector2.zero;

                for (int i = 0; i < pointList.Count; i++)
                {
                    GeoJSONPoint p = pointList[i];
                    pointCoords.x += (float)p.x;
                    pointCoords.y += (float)p.y;
                    if (i == pointList.Count - 1)
                    {
                        pointCoords = new Vector2(pointCoords.x / pointList.Count, pointCoords.y / pointList.Count);
                        Debug.Log($"Point coords at: {pointCoords}");
                        float yOffset = 30f;
                        Object.Instantiate(template, new Vector3(pointCoords.x, yOffset, pointCoords.y), Quaternion.identity, owner.SpawnParent);
                    }
                }
            }
        }
        Object.Destroy(template);
    }

}
