using Netherlands3D.Coordinates;
using Netherlands3D.WFSHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Netherlands3D.GeoJSON;
using Netherlands3D.Web;
using UnityEngine;
using UnityEngine.Events;

public enum FeatureType
{
    Integer,
    String,
    Double,
    Long,
    Custom
}

public struct WFSFeatureDescriptor
{
    public string Name;
    public FeatureType Type;
    public string TypeString;

    public WFSFeatureDescriptor(string name, FeatureType type)
    {
        Name = name;
        Type = type;
        TypeString = type.ToString();
    }

    public WFSFeatureDescriptor(string name, string customType)
    {
        Name = name;
        Type = FeatureType.Custom;
        TypeString = customType;
    }
}

public class WFS : IWebService
{
    public static WFS ActiveInstance { get; private set; }
    public string TypeName { get; set; }
    public string Version { get; set; } = "2.0.0";
    //public List<string> PropertyNames { get; set; }
    public List<WFSFeature> features { get; set; }
    public List<WFSFeatureDescriptor> ActiveFeatureDescriptors { get; private set; }
    public string BaseUrl { get; private set; }
    public Dictionary<string, string> RequestHeaders { get; set; }

    public BoundingBox BBox;

    public int StartIndex = 0;
    public int Count = 0;

    public WFSFeature ActiveFeature { get; private set; }
    //public WFSFeatureData featureData { get; private set; }
    public GeoJSONStreamReader ActiveGeoJsonStreamReader { get; private set; }
    //public WFSFeature activeFeature;

    public UnityEvent wfsGetCapabilitiesProcessedEvent = new UnityEvent();
    public UnityEvent<List<WFSFeatureDescriptor>> activeFeatureDescriptorsReceived = new UnityEvent<List<WFSFeatureDescriptor>>();
    public UnityEvent<List<WFSFeatureData>> wfsFeatureDataReceivedEvent = new UnityEvent<List<WFSFeatureData>>();

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

    public string GetFeatures(List<string> propertyFilters)
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

        stringBuilder.Append("&PropertyName=");
        foreach (var propertyFilter in propertyFilters)
        {
            stringBuilder.Append(propertyFilter);
            stringBuilder.Append(",");
        }
        stringBuilder.Remove(stringBuilder.Length - 1, 1); //remove trailing comma

        return stringBuilder.ToString();

    }
    private string featureRequest => "?REQUEST=GetFeature&SERVICE=WFS";
    private string versionRequest => $"VERSION={Version}";
    private string typeNameRequest => $"TypeName={TypeName}";
    //private string propertyNameRequest => $"PropertyName={PropertyName}";
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
        ActiveFeatureDescriptors = new();
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
            string typeString = element.Attributes["type"].Value;
            //Debug.Log($"Name: {name}, Type: {typeString}");
            if (Enum.TryParse(typeof(FeatureType), typeString, true, out var type))
            {
                ActiveFeatureDescriptors.Add(new WFSFeatureDescriptor(name, (FeatureType)type));
            }
            else
            {
                Debug.Log(typeString + " is not an available FeatureType, setting it to custom: " + name);
                ActiveFeatureDescriptors.Add(new WFSFeatureDescriptor(name, typeString));
            }
        }

        activeFeatureDescriptorsReceived.Invoke(ActiveFeatureDescriptors);
    }

    public void GetFeature(List<string> propertyNames = null)
    {
        //if (!ParseFields()) //todo: set bounding box requirement?
        //    return;

        //PropertyNames = propertyNames;
        TypeName = ActiveFeature.FeatureName;

        var url = GetFeatures(propertyNames);
        Debug.Log("getting wfs features at " + url);
        WebRequest.CreateWebRequest(url, RequestHeaders, FeatureCallback);
        //StartCoroutine(UrlReader.GetWebString(ActiveWFS.GetFeatures(), (string s) => StartCoroutine(HandleFeatureJSON(new GeoJSON(s)))));
    }

    private void FeatureCallback(string geoJSONString)
    {
        ActiveGeoJsonStreamReader = new GeoJSONStreamReader(geoJSONString);
        ProcessFeatureJSON(ActiveGeoJsonStreamReader);
    }

    public void SetActiveFeature(string activatedFeatureName)
    {
        //Debug.Log("setting active feature " + activatedFeature.FeatureName);
        ActiveFeature = features.FirstOrDefault(feature => feature.FeatureName == activatedFeatureName);

        if (ActiveFeature == null)
        {
            var newFeature = new WFSFeature(activatedFeatureName);
            features.Add(newFeature);
            ActiveFeature = newFeature;
        }

        Debug.Log(ActiveFeature.FeatureName);
    }

    private void ProcessFeatureJSON(GeoJSONStreamReader geoJsonStreamReader)
    {
        Debug.Log("Handling Feature JSON!");

        var list = new List<WFSFeatureData>();
        while (geoJsonStreamReader.GotoNextFeature())
        {
            var featureData = new WFSFeatureData();
            featureData.GeometryType = geoJsonStreamReader.GetGeometryType();
            featureData.TransferDictionary(geoJsonStreamReader.GetProperties());
            ActiveFeature.AddNewFeatureData(featureData); //list of all requested features in this session
            list.Add(featureData); //list of all features for this request only
            EvaluateGeoType(geoJsonStreamReader);
        }
        //wfsFeatureDataReceivedEvent.Invoke(ActiveFeature.GetFeatureDataList);
        //Debug.Log("listcount: " + list.Count);
        wfsFeatureDataReceivedEvent.Invoke(list);
    }

    public bool AddListenerFeatureProcessed(UnityAction<Vector3> action)
    {
        if (ActiveGeoJsonStreamReader.GetGeometryType() == GeoJSONStreamReader.GeoJSONGeometryType.Point)
        {
            pointEvent.AddListener(action);
            return true;
        }
        return false;
    }

    public bool AddListenerFeatureProcessed(UnityAction<List<Vector3>> action)
    {
        if (ActiveGeoJsonStreamReader.GetGeometryType() == GeoJSONStreamReader.GeoJSONGeometryType.MultiPoint ||
            ActiveGeoJsonStreamReader.GetGeometryType() == GeoJSONStreamReader.GeoJSONGeometryType.LineString ||
            ActiveGeoJsonStreamReader.GetGeometryType() == GeoJSONStreamReader.GeoJSONGeometryType.Polygon)
        {
            listPointEvent.AddListener(action);
            return true;
        }
        return false;
    }

    public bool AddListenerFeatureProcessed(UnityAction<List<List<Vector3>>> action)
    {
        if (ActiveGeoJsonStreamReader.GetGeometryType() == GeoJSONStreamReader.GeoJSONGeometryType.MultiLineString ||
            ActiveGeoJsonStreamReader.GetGeometryType() == GeoJSONStreamReader.GeoJSONGeometryType.MultiPolygon)
        {
            multiListPointEvent.AddListener(action);
            return true;
        }
        return false;
    }

    private void EvaluateGeoType(GeoJSONStreamReader geoJsonStreamReader)
    {
        switch (geoJsonStreamReader.GetGeometryType())
        {
            case GeoJSONStreamReader.GeoJSONGeometryType.Point:
                double[] geoPointDouble = geoJsonStreamReader.GetGeometryPoint2DDouble();
                var coord = CoordinateConverter.ConvertTo(new Coordinate(CoordinateSystem.RD, geoPointDouble[0], geoPointDouble[1], -10), CoordinateSystem.Unity);
                pointEvent.Invoke(coord.ToVector3());
                break;
            case GeoJSONStreamReader.GeoJSONGeometryType.MultiPoint:
                MultiPointHandler pointHandler = new MultiPointHandler();
                listPointEvent.Invoke(pointHandler.ProcessMultiPoint(geoJsonStreamReader.GetMultiPoint()));
                break;
            case GeoJSONStreamReader.GeoJSONGeometryType.LineString:
                LineStringHandler lineStringHandler = new LineStringHandler();
                //ShiftLineColor();
                listPointEvent.Invoke(lineStringHandler.ProcessLineString(geoJsonStreamReader.GetGeometryLineString()));
                break;
            case GeoJSONStreamReader.GeoJSONGeometryType.MultiLineString:
                MultiLineHandler multiLineHandler = new MultiLineHandler();
                multiListPointEvent.Invoke(multiLineHandler.ProcessMultiLine(geoJsonStreamReader.GetMultiLine()));
                break;
            case GeoJSONStreamReader.GeoJSONGeometryType.Polygon:
                PolygonHandler polyHandler = new PolygonHandler();
                listPointEvent.Invoke(polyHandler.ProcessPolygon(geoJsonStreamReader.GetPolygon()));
                break;
            case GeoJSONStreamReader.GeoJSONGeometryType.MultiPolygon:
                MultiPolygonHandler multiPolyHandler = new MultiPolygonHandler();
                multiListPointEvent.Invoke(multiPolyHandler.GetMultiPoly(geoJsonStreamReader.GetMultiPolygon()));
                break;
            case GeoJSONStreamReader.GeoJSONGeometryType.GeometryCollection:
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
