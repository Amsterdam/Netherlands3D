using System.Collections.Generic;
using Netherlands3D.Coordinates;
using Netherlands3D.GeoJSON;
using UnityEngine;

namespace Netherlands3D.WFSHandlers
{
    public class LineStringHandler
    {
        public List<Vector3> ProcessLineString(List<GeoJSONPoint> points)
        {
            List<Vector3> result = new();
            foreach (GeoJSONPoint point in points)
            {
                result.Add(CoordinateConverter.RDtoUnity(point.x, point.y, -10));
            }
            return result;
        }

    }
}
