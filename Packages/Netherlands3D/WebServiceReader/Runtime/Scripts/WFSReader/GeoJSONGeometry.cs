using System;
using System.Collections.Generic;
using Netherlands3D.Coordinates;
using Netherlands3D.GeoJSON;
using UnityEngine;

namespace Netherlands3D.WFSHandlers
{
    public class PointGeometry
    {
        public GeoJSONStreamReader.GeoJSONGeometryType GeometryType;
        public Vector3 point;

        public PointGeometry(GeoJSONStreamReader.GeoJSONGeometryType geometryType, Vector3 point)
        {
            GeometryType = geometryType;
            this.point = point;
        }
    }

    public class ListPointGeometry
    {
        public GeoJSONStreamReader.GeoJSONGeometryType GeometryType;
        public List<Vector3> points;

        public ListPointGeometry(GeoJSONStreamReader.GeoJSONGeometryType geometryType, List<Vector3> points)
        {
            GeometryType = geometryType;
            this.points = points;
        }
    }

    public class MultiListPointGeometry
    {
        public GeoJSONStreamReader.GeoJSONGeometryType GeometryType;
        public List<List<Vector3>> points;

        public MultiListPointGeometry(GeoJSONStreamReader.GeoJSONGeometryType geometryType, List<List<Vector3>> points)
        {
            GeometryType = geometryType;
            this.points = points;
        }
    }

    public class GeoJSONGeometry
    {
        public GeoJSONStreamReader ActiveGeoJsonStreamReader { get; set; }

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
            GeoJSONStreamReader geoJsonStreamReader = ActiveGeoJsonStreamReader;

            switch (geoJsonStreamReader.GetGeometryType())
            {
                case GeoJSONStreamReader.GeoJSONGeometryType.Point:
                    double[] geoPointDouble = geoJsonStreamReader.GetGeometryPoint2DDouble();
                    //pointEvent.Invoke(CoordConvert.RDtoUnity(geoPointDouble[0], geoPointDouble[1], -10));
                    var coord = CoordinateConverter.ConvertTo(new Coordinate(CoordinateSystem.RD, geoPointDouble[0], geoPointDouble[1], -10), CoordinateSystem.Unity);
                    var point = coord.ToVector3();
                    this.points.Add(new PointGeometry(GeoJSONStreamReader.GeoJSONGeometryType.Point, point));
                    break;
                case GeoJSONStreamReader.GeoJSONGeometryType.MultiPoint:
                    MultiPointHandler pointHandler = new MultiPointHandler();
                    //listPointEvent.Invoke(pointHandler.ProcessMultiPoint(geoJSON.GetMultiPoint()));
                    var points = pointHandler.ProcessMultiPoint(geoJsonStreamReader.GetMultiPoint());
                    listPoints.Add(new ListPointGeometry(GeoJSONStreamReader.GeoJSONGeometryType.MultiPoint, points));
                    break;
                case GeoJSONStreamReader.GeoJSONGeometryType.LineString:
                    LineStringHandler lineStringHandler = new LineStringHandler();
                    //ShiftLineColor();
                    //listPointEvent.Invoke(lineStringHandler.ProcessLineString(geoJSON.GetGeometryLineString()));
                    var lineString = lineStringHandler.ProcessLineString(geoJsonStreamReader.GetMultiPoint());
                    listPoints.Add(new ListPointGeometry(GeoJSONStreamReader.GeoJSONGeometryType.LineString, lineString));
                    break;
                case GeoJSONStreamReader.GeoJSONGeometryType.MultiLineString:
                    MultiLineHandler multiLineHandler = new MultiLineHandler();
                    //multiListPointEvent.Invoke(multiLineHandler.ProcessMultiLine(geoJSON.GetMultiLine()));
                    var multiLineString = multiLineHandler.ProcessMultiLine(geoJsonStreamReader.GetMultiLine());
                    multiListPoints.Add(new MultiListPointGeometry(GeoJSONStreamReader.GeoJSONGeometryType.MultiLineString, multiLineString));
                    break;
                case GeoJSONStreamReader.GeoJSONGeometryType.Polygon:
                    PolygonHandler polyHandler = new PolygonHandler();
                    //listPointEvent.Invoke(polyHandler.ProcessPolygon(geoJSON.GetPolygon()));
                    var polygon = polyHandler.ProcessPolygon(geoJsonStreamReader.GetPolygon());
                    listPoints.Add(new ListPointGeometry(GeoJSONStreamReader.GeoJSONGeometryType.Polygon, polygon));
                    break;
                case GeoJSONStreamReader.GeoJSONGeometryType.MultiPolygon:
                    MultiPolygonHandler multiPolyHandler = new MultiPolygonHandler();
                    //multiListPointEvent.Invoke(multiPolyHandler.GetMultiPoly(geoJSON.GetMultiPolygon()));
                    var multiPolygon = multiPolyHandler.GetMultiPoly(geoJsonStreamReader.GetMultiPolygon());
                    multiListPoints.Add(new MultiListPointGeometry(GeoJSONStreamReader.GeoJSONGeometryType.MultiPolygon, multiPolygon));
                    break;
                case GeoJSONStreamReader.GeoJSONGeometryType.GeometryCollection:
                    // String Event voor error.
                    throw new NotImplementedException("Geometry Type of type: 'GeometryCollection' is not currently supported");
                //break;
                default:
                    // String Event voor error.
                    throw new Exception("Geometry Type is either 'Undefined' or not found, cannot process like this!");
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
