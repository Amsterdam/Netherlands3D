using System.Collections.Generic;
using Netherlands3D.Coordinates;
using Netherlands3D.GeoJSON;
using UnityEngine;

namespace Netherlands3D.WFSHandlers
{
    public class MultiLineHandler
    {

        public List<List<Vector3>> ProcessMultiLine(List<List<GeoJSONPoint>> pointList)
        {
            List<List<Vector3>> result = new();
            foreach(List<GeoJSONPoint> list in pointList)
            {
                List<Vector3> linePoints = new();
                foreach(GeoJSONPoint point in list)
                {
                    linePoints.Add(CoordinateConverter.RDtoUnity(point.x, point.y, -10));
                }
                result.Add(linePoints);
            }
            return result;
        }


    }
}
