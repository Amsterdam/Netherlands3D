using Netherlands3D.Core;
using Netherlands3D.Utilities;
using Netherlands3D.WFSHandlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;

public class WFS : IWebService
{
    public static WFS ActiveInstance { get; private set; }
    public string TypeName { get; set; }
    public string Version { get; set; }
    public List<WFSFeature> features { get; set; }
    public string BaseUrl { get; private set; }
    public Dictionary<string, string> RequestHeaders { get; set; }

    public BoundingBox BBox;

    public int StartIndex = 0;
    public int Count = 0;

    public WFSFeature ActiveFeature { get; private set; }
    //public WFSFeatureData featureData { get; private set; }
    public GeoJSON ActiveGeoJSON { get; private set; }
    //public WFSFeature activeFeature;

    public UnityEvent wfsGetCapabilitiesProcessedEvent = new UnityEvent();
    public UnityEvent<WFSFeatureData> wfsFeatureDataReceivedEvent = new UnityEvent<WFSFeatureData>();

    private UnityEvent<Vector3> pointEvent = new UnityEvent<Vector3>();
    private UnityEvent<List<Vector3>> listPointEvent = new UnityEvent<List<Vector3>>();
    private UnityEvent<List<List<Vector3>>> multiListPointEvent = new UnityEvent<List<List<Vector3>>>();

    private bool tileHandled;
    private float coroutineRunTime = 200f;

    public WFS(string baseUrl)
    {
        BaseUrl = baseUrl;
        ActiveInstance = this;
        features = new();
    }

    public string GetCapabilities()
    {
        return BaseUrl + "?request=GetCapabilities&service=WFS";
    }

    public string GetFeatures()
    {
        StringBuilder stringBuilder = new StringBuilder(BaseUrl);
        stringBuilder.Append(featureRequest);
        stringBuilder.Append("&");
        stringBuilder.Append(versionRequest);
        stringBuilder.Append("&");
        stringBuilder.Append(typeNameRequest);
        stringBuilder.Append("&");
        stringBuilder.Append(outputFormatRequest);
        stringBuilder.Append("&");
        if (Count > 0)
        {
            stringBuilder.Append(countRequest);
            stringBuilder.Append("&");
        }
        stringBuilder.Append(startIndexRequest);
        stringBuilder.Append("&");
        stringBuilder.Append(boundingBoxRequest);

        return stringBuilder.ToString();

    }
    private string featureRequest => "?REQUEST=GetFeature&SERVICE=WFS";
    private string versionRequest => $"VERSION={Version}";
    private string typeNameRequest => $"TypeName={TypeName}";
    private string outputFormatRequest => "OutputFormat=geojson";
    private string countRequest => $"count={Count}";
    private string startIndexRequest => $"startindex={StartIndex}";
    private string boundingBoxRequest => tileHandled ? "bbox={Xmin},{Ymin},{Xmax},{Ymax}" : $"bbox={BBox.MinX},{BBox.MinY},{BBox.MaxX},{BBox.MaxY}";

    public void RequestWFSGetCapabilities()
    {
        var url = GetCapabilities();
        Debug.Log("getting wfs capabilities at " + url);
        //StartCoroutine(UrlReader.GetWebString(url, callback));
        WebRequest.CreateWebRequest(url, RequestHeaders, ProcessGetCapabilitesXML);
    }

    private void ProcessGetCapabilitesXML(string xmlData)
    {
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlData);

        XmlElement serviceID = xml.DocumentElement["ows:ServiceIdentification"]["ows:ServiceType"];
        if (serviceID != null && serviceID.InnerText.Contains("WFS"))
        {
            //WFSFormatter formatter = new WFSFormatter();
            this.ReadFromWFS(xml);
            //resetReaderEvent.InvokeStarted();
            //isWmsEvent.InvokeStarted(false);
            wfsGetCapabilitiesProcessedEvent.Invoke();
        }
    }

    public void GetDescribeFeatureType()
    {
        var url = BaseUrl + "?REQUEST=DescribeFeatureType&service=WFS&" + versionRequest;
        Debug.Log(url);
        WebRequest.CreateWebRequest(url, RequestHeaders, ProcessGetFeatureInfo);
    }

    private void ProcessGetFeatureInfo(string featureInfoXML)
    {
        Debug.Log(featureInfoXML);

        XmlDocument xml = new XmlDocument();
        xml.LoadXml(featureInfoXML);

        XmlNamespaceManager nsManager = new XmlNamespaceManager(xml.NameTable);
        nsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
        XmlNodeList elements = xml.SelectNodes("//xsd:element", nsManager);

        foreach (XmlNode element in elements)
        {
            string name = element.Attributes["name"].Value;
            string type = element.Attributes["type"].Value;
            Debug.Log($"Name: {name}, Type: {type}");
        }
        //XmlElement serviceID = xml.DocumentElement["ows:ServiceIdentification"]["ows:ServiceType"];
        //if (serviceID != null && serviceID.InnerText.Contains("WFS"))
        //{
        //    //WFSFormatter formatter = new WFSFormatter();
        //    this.ReadFromWFS(xml);
        //    //resetReaderEvent.InvokeStarted();
        //    //isWmsEvent.InvokeStarted(false);
        //    wfsGetCapabilitiesProcessedEvent.Invoke();
        //}
    }

    public void GetFeature()
    {
        //if (!ParseFields()) //todo: set bounding box requirement?
        //    return;

        TypeName = ActiveFeature.FeatureName;

        var url = GetFeatures();
        Debug.Log("getting wfs features at " + url);
        WebRequest.CreateWebRequest(GetFeatures(), RequestHeaders, FeatureCallback);
        //StartCoroutine(UrlReader.GetWebString(ActiveWFS.GetFeatures(), (string s) => StartCoroutine(HandleFeatureJSON(new GeoJSON(s)))));
    }

    private void FeatureCallback(string geoJSONString)
    {
        ActiveGeoJSON = new GeoJSON(geoJSONString);
        ProcessFeatureJSONPerTimeInterval(ActiveGeoJSON);
    }

    public void SetActiveFeature(string activatedFeatureName)
    {
        //Debug.Log("setting active feature " + activatedFeature.FeatureName);
        ActiveFeature = features.FirstOrDefault(feature => feature.FeatureName == activatedFeatureName);
        Debug.Log(ActiveFeature);
    }

    private void ProcessFeatureJSONPerTimeInterval(GeoJSON geoJSON)
    {
        Debug.Log("Handling Feature JSON!");

        while (geoJSON.GotoNextFeature())
        {
            var featureData = new WFSFeatureData();
            featureData.GeometryType = geoJSON.GetGeometryType();
            featureData.TransferDictionary(geoJSON.GetProperties());
            ActiveFeature.AddNewFeatureData(featureData);
            wfsFeatureDataReceivedEvent.Invoke(featureData);

            EvaluateGeoType(geoJSON);
        }
    }

    public bool AddListenerFeatureProcessed(UnityAction<Vector3> action)
    {
        if (ActiveGeoJSON.GetGeometryType() == GeoJSON.GeoJSONGeometryType.Point)
        {
            pointEvent.AddListener(action);
            return true;
        }
        return false;
    }

    public bool AddListenerFeatureProcessed(UnityAction<List<Vector3>> action)
    {
        if (ActiveGeoJSON.GetGeometryType() == GeoJSON.GeoJSONGeometryType.MultiPoint ||
            ActiveGeoJSON.GetGeometryType() == GeoJSON.GeoJSONGeometryType.LineString ||
            ActiveGeoJSON.GetGeometryType() == GeoJSON.GeoJSONGeometryType.Polygon)
        {
            listPointEvent.AddListener(action);
            return true;
        }
        return false;
    }

    public bool AddListenerFeatureProcessed(UnityAction<List<List<Vector3>>> action)
    {
        if (ActiveGeoJSON.GetGeometryType() == GeoJSON.GeoJSONGeometryType.MultiLineString ||
            ActiveGeoJSON.GetGeometryType() == GeoJSON.GeoJSONGeometryType.MultiPolygon)
        {
            multiListPointEvent.AddListener(action);
            return true;
        }
        return false;
    }

    private void EvaluateGeoType(GeoJSON geoJSON)
    {
        switch (geoJSON.GetGeometryType())
        {
            case GeoJSON.GeoJSONGeometryType.Point:
                double[] geoPointDouble = geoJSON.GetGeometryPoint2DDouble();
                pointEvent.Invoke(CoordConvert.RDtoUnity(geoPointDouble[0], geoPointDouble[1], -10));
                break;
            case GeoJSON.GeoJSONGeometryType.MultiPoint:
                MultiPointHandler pointHandler = new MultiPointHandler();
                listPointEvent.Invoke(pointHandler.ProcessMultiPoint(geoJSON.GetMultiPoint()));
                break;
            case GeoJSON.GeoJSONGeometryType.LineString:
                LineStringHandler lineStringHandler = new LineStringHandler();
                //ShiftLineColor();
                listPointEvent.Invoke(lineStringHandler.ProcessLineString(geoJSON.GetGeometryLineString()));
                break;
            case GeoJSON.GeoJSONGeometryType.MultiLineString:
                MultiLineHandler multiLineHandler = new MultiLineHandler();
                multiListPointEvent.Invoke(multiLineHandler.ProcessMultiLine(geoJSON.GetMultiLine()));
                break;
            case GeoJSON.GeoJSONGeometryType.Polygon:
                PolygonHandler polyHandler = new PolygonHandler();
                listPointEvent.Invoke(polyHandler.ProcessPolygon(geoJSON.GetPolygon()));
                break;
            case GeoJSON.GeoJSONGeometryType.MultiPolygon:
                MultiPolygonHandler multiPolyHandler = new MultiPolygonHandler();
                multiListPointEvent.Invoke(multiPolyHandler.GetMultiPoly(geoJSON.GetMultiPolygon()));
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

    public void RemoveAllListenersFeatureProcessed()
    {
        pointEvent.RemoveAllListeners();
        listPointEvent.RemoveAllListeners();
        multiListPointEvent.RemoveAllListeners();
    }
}
