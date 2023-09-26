using System.Collections.Generic;
using Netherlands3D.Coordinates;
using Netherlands3D.GeoJSON;
using UnityEngine;

namespace Netherlands3D.WFSHandlers
{
    public class MultiPointHandler
    {
        public List<Vector3> ProcessMultiPoint(List<GeoJSONPoint> pointList)
        {
            List<Vector3> result = new();
            foreach (GeoJSONPoint geoPoint in pointList)
            {
                result.Add(CoordinateConverter.RDtoUnity(geoPoint.x, geoPoint.y, -10));
            }
            return result;
        }
    }
}
