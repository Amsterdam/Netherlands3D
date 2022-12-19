using Netherlands3D.Core;
using Netherlands3D.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class MultiPointHandler
{
    private WFSHandler owner;
    public MultiPointHandler(WFSHandler owner)
    {
        this.owner = owner;
    }
    public void ProcessMultiPoint(List<GeoJSONPoint> pointList)
    {
        GameObject template = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Vector2 pointCoords = Vector2.zero;

        for (int i = 0; i < pointList.Count; i++)
        {
            GeoJSONPoint p = pointList[i];
            Debug.Log($"[{p.x}, {p.y}]");
            pointCoords.x += (float)p.x;
            pointCoords.y += (float)p.y;
            if (i == pointList.Count - 1)
            {
                pointCoords = new Vector2(pointCoords.x / pointList.Count, pointCoords.y / pointList.Count);
                Debug.Log($"Averaged point coords at: {pointCoords}");
                float yOffset = 30f;
                Vector3 eval = pointCoords;
                Vector3 unityCoords = CoordConvert.RDtoUnity(eval);
                Object.Instantiate(template, new Vector3(unityCoords.x, yOffset, unityCoords.z), Quaternion.identity, owner.SpawnParent);
            }
        }
        Object.Destroy(template);
    }
}
