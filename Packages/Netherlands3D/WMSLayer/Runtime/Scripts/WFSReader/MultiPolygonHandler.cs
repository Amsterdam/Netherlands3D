using Netherlands3D.Utilities;
using Netherlands3D.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.WFSHandlers
{
    public class MultiPolygonHandler
    {
        public List<IList<Vector3>> GetMultiPoly(List<List<List<GeoJSONPoint>>> multiPolyList)
        {
            List<IList<Vector3>> result = new();
            foreach (List<List<GeoJSONPoint>> pointListList in multiPolyList)
            {
                foreach (List<GeoJSONPoint> pointList in pointListList)
                {
                    List<Vector3> points = new();
                    foreach (GeoJSONPoint geoPoint in pointList)
                    {
                        Vector3 unityPoint = CoordConvert.RDtoUnity(geoPoint.x, geoPoint.y, -10);
                        points.Add(unityPoint);
                    }
                    result.Add(points);
                }

            }
            return result;
        }
    }
}
