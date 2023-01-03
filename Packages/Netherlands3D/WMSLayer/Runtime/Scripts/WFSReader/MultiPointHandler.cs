using Netherlands3D.Core;
using Netherlands3D.Utilities;
using System.Collections.Generic;
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
                result.Add(CoordConvert.RDtoUnity(geoPoint.x, geoPoint.y, -10));
            }
            return result;
        }
    }
}
