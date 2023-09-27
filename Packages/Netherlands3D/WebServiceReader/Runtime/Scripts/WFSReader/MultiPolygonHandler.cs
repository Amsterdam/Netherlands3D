using System.Collections.Generic;
using Netherlands3D.Coordinates;
using Netherlands3D.GeoJSON;
using UnityEngine;

namespace Netherlands3D.WFSHandlers
{
    public class MultiPolygonHandler
    {
        public List<List<Vector3>> GetMultiPoly(List<List<List<GeoJSONPoint>>> multiPolyList)
        {
            List<List<Vector3>> result = new();
            foreach (List<List<GeoJSONPoint>> pointListList in multiPolyList)
            {
                foreach (List<GeoJSONPoint> pointList in pointListList)
                {
                    List<Vector3> points = new();
                    foreach (GeoJSONPoint geoPoint in pointList)
                    {
                        Vector3 unityPoint = CoordinateConverter.RDtoUnity(geoPoint.x, geoPoint.y, -10);
                        points.Add(unityPoint);
                    }
                    result.Add(points);
                }

            }
            return result;
        }
    }
}
