using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Core;
using Netherlands3D.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.WFSHandlers
{
    public class PointGeometry
    {
        public GeoJSON.GeoJSONGeometryType GeometryType;
        public Vector3 point;

        public PointGeometry(GeoJSON.GeoJSONGeometryType geometryType, Vector3 point)
        {
            GeometryType = geometryType;
            this.point = point;
        }
    }

    public class ListPointGeometry
    {
        public GeoJSON.GeoJSONGeometryType GeometryType;
        public List<Vector3> points;

        public ListPointGeometry(GeoJSON.GeoJSONGeometryType geometryType, List<Vector3> points)
        {
            GeometryType = geometryType;
            this.points = points;
        }
    }

    public class MultiListPointGeometry
    {
        public GeoJSON.GeoJSONGeometryType GeometryType;
        public List<List<Vector3>> points;

        public MultiListPointGeometry(GeoJSON.GeoJSONGeometryType geometryType, List<List<Vector3>> points)
        {
            GeometryType = geometryType;
            this.points = points;
        }
    }

    public class GeoJSONGeometry
    {
        public GeoJSON ActiveGeoJSON { get; set; }

        public List<PointGeometry> points = new();
        public List<ListPointGeometry> listPoints = new();
        public List<MultiListPointGeometry> multiListPoints = new();

        //private UnityEvent<Vector3> pointEvent = new UnityEvent<Vector3>();
        //private UnityEvent<List<Vector3>> listPointEvent = new UnityEvent<List<Vector3>>();
        //private UnityEvent<List<List<Vector3>>> multiListPointEvent = new UnityEvent<List<List<Vector3>>>();

        //public bool AddListenerFeatureProcessed(UnityAction<Vector3> action)
        //{
        //    if (ActiveGeoJSON.GetGeometryType() == GeoJSON.GeoJSONGeometryType.Point)
        //    {
        //        pointEvent.AddListener(action);
        //        return true;
        //    }
        //    return false;
        //}

        //public bool AddListenerFeatureProcessed(UnityAction<List<Vector3>> action)
        //{
        //    if (ActiveGeoJSON.GetGeometryType() == GeoJSON.GeoJSONGeometryType.MultiPoint ||
        //        ActiveGeoJSON.GetGeometryType() == GeoJSON.GeoJSONGeometryType.LineString ||
        //        ActiveGeoJSON.GetGeometryType() == GeoJSON.GeoJSONGeometryType.Polygon)
        //    {
        //        listPointEvent.AddListener(action);
        //        return true;
        //    }
        //    return false;
        //}

        //public bool AddListenerFeatureProcessed(UnityAction<List<List<Vector3>>> action)
        //{
        //    if (ActiveGeoJSON.GetGeometryType() == GeoJSON.GeoJSONGeometryType.MultiLineString ||
        //        ActiveGeoJSON.GetGeometryType() == GeoJSON.GeoJSONGeometryType.MultiPolygon)
        //    {
        //        multiListPointEvent.AddListener(action);
        //        return true;
        //    }
        //    return false;
        //}

        public void EvaluateGeoType()
        {
            GeoJSON geoJSON = ActiveGeoJSON;

            switch (geoJSON.GetGeometryType())
            {
                case GeoJSON.GeoJSONGeometryType.Point:
                    double[] geoPointDouble = geoJSON.GetGeometryPoint2DDouble();
                    //pointEvent.Invoke(CoordConvert.RDtoUnity(geoPointDouble[0], geoPointDouble[1], -10));
                    var point = CoordConvert.RDtoUnity(geoPointDouble[0], geoPointDouble[1], -10);
                    this.points.Add(new PointGeometry(GeoJSON.GeoJSONGeometryType.Point, point));
                    break;
                case GeoJSON.GeoJSONGeometryType.MultiPoint:
                    MultiPointHandler pointHandler = new MultiPointHandler();
                    //listPointEvent.Invoke(pointHandler.ProcessMultiPoint(geoJSON.GetMultiPoint()));
                    var points = pointHandler.ProcessMultiPoint(geoJSON.GetMultiPoint());
                    listPoints.Add(new ListPointGeometry(GeoJSON.GeoJSONGeometryType.MultiPoint, points));
                    break;
                case GeoJSON.GeoJSONGeometryType.LineString:
                    LineStringHandler lineStringHandler = new LineStringHandler();
                    //ShiftLineColor();
                    //listPointEvent.Invoke(lineStringHandler.ProcessLineString(geoJSON.GetGeometryLineString()));
                    var lineString = lineStringHandler.ProcessLineString(geoJSON.GetMultiPoint());
                    listPoints.Add(new ListPointGeometry(GeoJSON.GeoJSONGeometryType.LineString, lineString));
                    break;
                case GeoJSON.GeoJSONGeometryType.MultiLineString:
                    MultiLineHandler multiLineHandler = new MultiLineHandler();
                    //multiListPointEvent.Invoke(multiLineHandler.ProcessMultiLine(geoJSON.GetMultiLine()));
                    var multiLineString = multiLineHandler.ProcessMultiLine(geoJSON.GetMultiLine());
                    multiListPoints.Add(new MultiListPointGeometry(GeoJSON.GeoJSONGeometryType.MultiLineString, multiLineString));
                    break;
                case GeoJSON.GeoJSONGeometryType.Polygon:
                    PolygonHandler polyHandler = new PolygonHandler();
                    //listPointEvent.Invoke(polyHandler.ProcessPolygon(geoJSON.GetPolygon()));
                    var polygon = polyHandler.ProcessPolygon(geoJSON.GetPolygon());
                    listPoints.Add(new ListPointGeometry(GeoJSON.GeoJSONGeometryType.Polygon, polygon));
                    break;
                case GeoJSON.GeoJSONGeometryType.MultiPolygon:
                    MultiPolygonHandler multiPolyHandler = new MultiPolygonHandler();
                    //multiListPointEvent.Invoke(multiPolyHandler.GetMultiPoly(geoJSON.GetMultiPolygon()));
                    var multiPolygon = multiPolyHandler.GetMultiPoly(geoJSON.GetMultiPolygon());
                    multiListPoints.Add(new MultiListPointGeometry(GeoJSON.GeoJSONGeometryType.MultiPolygon, multiPolygon));
                    break;
                case GeoJSON.GeoJSONGeometryType.GeometryCollection:
                    // String Event voor error.
                    throw new System.NotImplementedException("Geometry Type of type: 'GeometryCollection' is not currently supported");
                //break;
                default:
                    // String Event voor error.
                    throw new System.Exception("Geometry Type is either 'Undefined' or not found, cannot process like this!");
            }
        }

        //public void RemoveAllListenersFeatureProcessed()
        //{
        //    pointEvent.RemoveAllListeners();
        //    listPointEvent.RemoveAllListeners();
        //    multiListPointEvent.RemoveAllListeners();
        //}
    }
}
