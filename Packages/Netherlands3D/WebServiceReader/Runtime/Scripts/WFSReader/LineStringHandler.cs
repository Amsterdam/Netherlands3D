using Netherlands3D.Utilities;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;

namespace Netherlands3D.WFSHandlers
{
    public class LineStringHandler
    {
        public List<Vector3> ProcessLineString(List<GeoJSONPoint> points)
        {
            List<Vector3> result = new();
            foreach (GeoJSONPoint point in points)
            {
                result.Add(CoordConvert.RDtoUnity(point.x, point.y, -10));
            }
            return result;
        }

    }
}