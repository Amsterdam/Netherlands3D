using System;
using System.Collections.Generic;
using Netherlands3D.Events;
using Netherlands3D.GeoJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class WFSHandler : MonoBehaviour
{
    public WFS ActiveWFS { get; private set; }
    public Transform SpawnParent { get; private set; }
    [SerializeField] private GameObject visualizer;

    [SerializeField] private TMP_InputField minXField;
    [SerializeField] private TMP_InputField maxXField;
    [SerializeField] private TMP_InputField minYField;
    [SerializeField] private TMP_InputField maxYField;

    [Header("Invoked Events")]
    [SerializeField] private ObjectEvent wfsDataEvent;
    [SerializeField] private BoolEvent isWmsEvent;
    [SerializeField] private ObjectEvent propertyFeatureEvent;
    [SerializeField] private Vector3ListsEvent drawPolygonsEvent;
    [SerializeField] private Vector3ListEvent drawPolygonEvent;
    [SerializeField] private Vector3ListEvent drawLineEvent;
    [SerializeField] private Vector3ListsEvent drawLinesEvent;
    [SerializeField] private Vector3Event pointEvent;
    [SerializeField] private Vector3ListEvent multiPointEvent;
    [SerializeField] private GameObjectEvent wfsParentEvent;
    [SerializeField] private ColorEvent lineColorEvent;

    [SerializeField] private UnityEvent<List<WFSFeature>> wfsFeatureListEvent;

    [Header("Listen Events")]
    [SerializeField] private StringEvent startIndexEvent;
    [SerializeField] private StringEvent countEvent;
    [SerializeField] private TriggerEvent resetReaderEvent;
    [SerializeField] private StringEvent urlEvent;
    [SerializeField] private StringEvent wfsCreationEvent;
    //[SerializeField] private StringEvent getFeatureEvent;
    [SerializeField] private ObjectEvent setActiveFeatureEvent;
    [SerializeField] private StringEvent wfsMinXEvent;
    [SerializeField] private StringEvent wfsMaxXEvent;
    [SerializeField] private StringEvent wfsMinYEvent;
    [SerializeField] private StringEvent wfsMaxYEvent;

    [SerializeField] private DoubleArrayEvent boundingBoxDoubleArrayEvent;

    //private WFSFeature activeFeature;
    //private WFSFeatureData featureData;
    //private float coroutineRunTime = 200f;
    private float hue = 0.9f;
    private float saturation = 0.5f;
    private float brightness = 0.5f;

    private void Awake()
    {
        wfsCreationEvent.AddListenerStarted(CreateWFS);
        //getFeatureEvent.started.AddListener(GetFeature);
        startIndexEvent.AddListenerStarted(SetStartIndex);
        countEvent.AddListenerStarted(SetWebFeatureCount);
        setActiveFeatureEvent.AddListenerStarted(SetActiveFeature);

        wfsMinXEvent.AddListenerStarted(SetBoundingBoxMinX);
        wfsMaxXEvent.AddListenerStarted(SetBoundingBoxMaxX);
        wfsMinYEvent.AddListenerStarted(SetBoundingBoxMinY);
        wfsMaxYEvent.AddListenerStarted(SetBoundingBoxMaxY);

        if (boundingBoxDoubleArrayEvent) boundingBoxDoubleArrayEvent.AddListenerStarted(SetBoundsFromDoubleArray);

        // The addition of these functions to the events are only for testing and debugging purposes.
        // They should be removed or commented out later.
        //pointEvent.AddListenerStarted(TestPoint);
        //multiPointEvent.AddListenerStarted(TestMultiPoints);
        //drawLinesEvent.AddListenerStarted(TestMultiLine);
    }
    private bool ParseFields()
    {
        if (!double.TryParse(minXField.text, out ActiveWFS.BBox.MinX))
            return false;

        if (!double.TryParse(maxXField.text, out ActiveWFS.BBox.MaxX))
            return false;

        if (!double.TryParse(minYField.text, out ActiveWFS.BBox.MinY))
            return false;

        if (!double.TryParse(maxYField.text, out ActiveWFS.BBox.MaxY))
            return false;

        return true;
    }

    private void SetBoundsFromDoubleArray(double[] boundsArray)
    {
        minXField.text = boundsArray[0].ToString();
        minYField.text = boundsArray[1].ToString();
        maxXField.text = boundsArray[2].ToString();
        maxYField.text = boundsArray[3].ToString();
    }

    private void CreateWFS(string baseUrl)
    {
        if (ActiveWFS != null)
            //ActiveWFS.
            //ActiveWFS.wfsGetCapabilitiesProcessedEvent.RemoveAllListeners();

        ActiveWFS = new WFS(baseUrl);
        //ActiveWFS = wfs;

        ClearSpawnedMeshItems();
        resetReaderEvent.InvokeStarted();

        //ActiveWFS.getCapabilitiesReceived.AddListener((object o, List<WFSFeature> l) => wfsFeatureListEvent.Invoke(l));
        //ActiveWFS.RequestWFSGetCapabilities(this);

        //ActiveWFS.wfsGetCapabilitiesProcessedEvent.AddListener(OnWFSDataProcessed);
    }

    private void OnWFSDataProcessed()
    {
        wfsDataEvent.InvokeStarted(ActiveWFS);
    }

    //private void GetWFSCapabilities(WFS wfs)
    //{
    //    var url = wfs.GetCapabilities();

    //    Debug.Log("getting wfs capabilities at " + wfs.GetCapabilities());

    //    //StartCoroutine(UrlReader.GetWebString(url, callback));
    //    WebRequest.CreateWebRequest(url, ProcessWFS);
    //}


    //called by button in inspector
    public void GetFeature()
    {
        if (!ParseFields() || (ActiveWFS == null))
            return;
        //ActiveWFS.featureDataReceived.RemoveAllListeners();
        //ActiveWFS.featureDataReceived.AddListener(FeatureCallback);
        ActiveWFS.wfsFeatureDataReceivedEvent.RemoveAllListeners(); //todo: also do this when selecting a new feature
        ActiveWFS.wfsFeatureDataReceivedEvent.AddListener(FeatureCallback);
        ActiveWFS.GetFeature();

        //ActiveWFS.TypeName = activeFeature.FeatureName;
        //Debug.Log(ActiveWFS.GetFeatures());
        //WebRequest.CreateWebRequest(ActiveWFS.GetFeatures(), FeatureCallback);
        //StartCoroutine(UrlReader.GetWebString(ActiveWFS.GetFeatures(), (string s) => StartCoroutine(HandleFeatureJSON(new GeoJSON(s)))));
    }
    public void SetActiveFeature(object activatedFeatureObject)
    {

        var activatedFeature = (WFSFeature)activatedFeatureObject;
        ActiveWFS.SetActiveFeature(activatedFeature.FeatureName);
    }

    private void FeatureCallback(List<WFSFeatureData> featureDataList)
    {
        foreach (var featureData in featureDataList)
        {
            SpawnParent = new GameObject().transform;
            SpawnParent.name = "WFS_ObjectParent";
            wfsParentEvent.InvokeStarted(SpawnParent.gameObject);

            LinkEventsForGeoJSONProcessing(featureData);
            //    var geoJSON = new GeoJSON(geoJSONString);
            //    ProcessFeatureJSONPerFrame(geoJSON);

        }
        if (propertyFeatureEvent)
            propertyFeatureEvent.InvokeStarted(ActiveWFS.ActiveFeature);
    }

    private void LinkEventsForGeoJSONProcessing(WFSFeatureData featureData)
    {
        //ActiveWFS.fea
        ActiveWFS.RemoveAllListenersFeatureProcessed();

        switch (featureData.GeometryType)
        {
            case GeoJSONStreamReader.GeoJSONGeometryType.Point:
                ActiveWFS.AddListenerFeatureProcessed(pointEvent.InvokeStarted);
                break;
            case GeoJSONStreamReader.GeoJSONGeometryType.MultiPoint:
                ActiveWFS.AddListenerFeatureProcessed(multiPointEvent.InvokeStarted);
                break;
            case GeoJSONStreamReader.GeoJSONGeometryType.LineString:
                ActiveWFS.AddListenerFeatureProcessed(drawLineEvent.InvokeStarted);
                break;
            case GeoJSONStreamReader.GeoJSONGeometryType.MultiLineString:
                ActiveWFS.AddListenerFeatureProcessed(drawLinesEvent.InvokeStarted);
                break;
            case GeoJSONStreamReader.GeoJSONGeometryType.Polygon:
                ActiveWFS.AddListenerFeatureProcessed(drawPolygonEvent.InvokeStarted);
                break;
            case GeoJSONStreamReader.GeoJSONGeometryType.MultiPolygon:
                ActiveWFS.AddListenerFeatureProcessed(drawPolygonsEvent.InvokeStarted);
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

    //private IEnumerator HandleFeatureJSON(GeoJSON geoJSON) // ???
    //{
    //    ShiftLineColor();
    //    SpawnParent = new GameObject().transform;
    //    SpawnParent.name = "WFS_ObjectParent";
    //    wfsParentEvent.InvokeStarted(SpawnParent.gameObject);

    //    DateTime dateTime = DateTime.UtcNow;

    //    Debug.Log("Handling Feature JSON!");
    //    if (geoJSON.FindFirstFeature())
    //    {
    //        Debug.Log("Hadn't found first feature! Doing it now!");

    //        var featureData = new WFSFeatureData();
    //        featureData.TransferDictionary(geoJSON.GetProperties());
    //        activeFeature.AddNewFeatureData(featureData);

    //        EvaluateGeoType(geoJSON);
    //    }
    //    while (geoJSON.GotoNextFeature())
    //    {
    //        Debug.Log("Evaluating next feature!");

    //        var featureData = new WFSFeatureData();
    //        featureData.TransferDictionary(geoJSON.GetProperties());
    //        activeFeature.AddNewFeatureData(featureData);

    //        EvaluateGeoType(geoJSON);

    //        if ((DateTime.UtcNow - dateTime).Milliseconds > coroutineRunTime)
    //        {
    //            yield return null;
    //            dateTime = DateTime.UtcNow;
    //        }
    //    }
    //    if (propertyFeatureEvent)
    //        propertyFeatureEvent.InvokeStarted(activeFeature);
    //}

    private void SetStartIndex(string index)
    {
        int.TryParse(index, out ActiveWFS.StartIndex);
    }

    private void SetWebFeatureCount(string count)
    {
        int.TryParse(count, out ActiveWFS.Count);
    }
    //private void EvaluateGeoType(GeoJSON geoJSON)
    //{
    //    switch (geoJSON.GetGeometryType())
    //    {
    //        case GeoJSON.GeoJSONGeometryType.Point:
    //            double[] geoPointDouble = geoJSON.GetGeometryPoint2DDouble();
    //            pointEvent.InvokeStarted(CoordConvert.RDtoUnity(geoPointDouble[0], geoPointDouble[1], -10));
    //            break;
    //        case GeoJSON.GeoJSONGeometryType.MultiPoint:
    //            MultiPointHandler pointHandler = new MultiPointHandler();
    //            multiPointEvent.InvokeStarted(pointHandler.ProcessMultiPoint(geoJSON.GetMultiPoint()));
    //            break;
    //        case GeoJSON.GeoJSONGeometryType.LineString:
    //            LineStringHandler lineStringHandler = new LineStringHandler();
    //            //ShiftLineColor();
    //            drawLineEvent.InvokeStarted(lineStringHandler.ProcessLineString(geoJSON.GetGeometryLineString()));
    //            break;
    //        case GeoJSON.GeoJSONGeometryType.MultiLineString:
    //            MultiLineHandler multiLineHandler = new MultiLineHandler();
    //            drawLinesEvent.InvokeStarted(multiLineHandler.ProcessMultiLine(geoJSON.GetMultiLine()));
    //            break;
    //        case GeoJSON.GeoJSONGeometryType.Polygon:
    //            PolygonHandler polyHandler = new PolygonHandler();
    //            drawPolygonEvent.InvokeStarted(polyHandler.ProcessPolygon(geoJSON.GetPolygon()));
    //            break;
    //        case GeoJSON.GeoJSONGeometryType.MultiPolygon:
    //            MultiPolygonHandler multiPolyHandler = new MultiPolygonHandler();
    //            drawPolygonsEvent.InvokeStarted(multiPolyHandler.GetMultiPoly(geoJSON.GetMultiPolygon()));
    //            break;
    //        case GeoJSON.GeoJSONGeometryType.GeometryCollection:
    //            // String Event voor error.
    //            throw new System.NotImplementedException("Geometry Type of type: 'GeometryCollection' is not currently supported");
    //        //break;
    //        default:
    //            // String Event voor error.
    //            throw new System.Exception("Geometry Type is either 'Undefined' or not found, cannot process like this!");

    //    }
    //}

    private void ClearSpawnedMeshItems()
    {
        if (SpawnParent == null)
            return;

        for (int i = SpawnParent.childCount - 1; i >= 0; i--)
        {
            Destroy(SpawnParent.GetChild(i).gameObject);
        }
    }

    private void SetBoundingBoxMinX(string value)
    {
        ActiveWFS.BBox.MinX = float.Parse(value);
    }
    private void SetBoundingBoxMaxX(string value)
    {
        ActiveWFS.BBox.MaxX = float.Parse(value);
    }
    private void SetBoundingBoxMinY(string value)
    {
        ActiveWFS.BBox.MinY = float.Parse(value);
    }
    private void SetBoundingBoxMaxY(string value)
    {
        ActiveWFS.BBox.MaxY = float.Parse(value);
    }
    private void ShiftLineColor()
    {
        hue += 0.1f;
        if (hue > 1)
        {
            hue = 0;
            saturation += 0.1f;
            if (saturation > 1)
            {
                saturation = 0;
                brightness += 0.1f;
                if (brightness > 1)
                {
                    brightness = 0;
                }
            }
        }
        lineColorEvent.InvokeStarted(Color.HSVToRGB(hue, saturation, brightness));
    }

    //private void TestMultiLine(List<List<Vector3>> multiLine)
    //{
    //    //ShiftLineColor();
    //    foreach (List<Vector3> lines in multiLine)
    //    {
    //        drawLineEvent.InvokeStarted(lines);
    //    }
    //}
    //private void TestPoint(Vector3 pointCoord)
    //{
    //    //Destroy(SpawnParent.gameObject);
    //    //SpawnParent = new GameObject().transform;
    //    //SpawnParent.name = "WFS_ObjectParent";
    //    GameObject wfsPointObject = Instantiate(visualizer, pointCoord + Vector3.up * 100, Quaternion.identity, SpawnParent);
    //}
    //private void TestMultiPoints(List<Vector3> pointCoords)
    //{
    //    //Destroy(SpawnParent.gameObject);
    //    //SpawnParent = new GameObject().transform;
    //    //SpawnParent.name = "WFS_ObjectParent";
    //    foreach (Vector3 position in pointCoords)
    //    {
    //        GameObject wfsPointObject = Instantiate(visualizer, position + Vector3.up * 100, Quaternion.identity, SpawnParent);
    //    }
    //}

}
