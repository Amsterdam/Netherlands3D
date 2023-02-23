using Netherlands3D.Core;
using Netherlands3D.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.WFSHandlers
{
    public class PolygonHandler
    {
        public List<Vector3> ProcessPolygon(List<List<GeoJSONPoint>> polygonList)
        {
            List<Vector3> result = new();
            foreach (List<GeoJSONPoint> points in polygonList)
            {
                foreach (GeoJSONPoint geoPoint in points)
                {
                    Vector3 unityPoint = CoordConvert.RDtoUnity(geoPoint.x, geoPoint.y, -10);
                    result.Add(unityPoint);
                }
            }
            return result;
        }
    }
}
