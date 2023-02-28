using Netherlands3D.Core;
using Netherlands3D.Events;
using Netherlands3D.Utilities;
using Netherlands3D.WFSHandlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

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


    private WFSFormatter formatter;
    private WFSFeature activeFeature;
    private WFSFeatureData featureData;
    private float coroutineRunTime = 200f;
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
        pointEvent.AddListenerStarted(TestPoint);
        multiPointEvent.AddListenerStarted(TestMultiPoints);
        drawLinesEvent.AddListenerStarted(TestMultiLine);
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

    public void CreateWFS(string baseUrl)
    {
        ActiveWFS = new WFS(baseUrl);
        ClearSpawnedMeshItems();
        resetReaderEvent.Invoke();
        Debug.Log(ActiveWFS.GetCapabilities());
        StartCoroutine(GetWebString(ActiveWFS.GetCapabilities(), ProcessWFS));
    }
    public void GetFeature()
    {
        if (!ParseFields())
            return;

        ActiveWFS.TypeName = activeFeature.FeatureName;
        Debug.Log(ActiveWFS.GetFeatures());
        StartCoroutine(GetWebString(ActiveWFS.GetFeatures(), (string s) => StartCoroutine(HandleFeatureJSON(new GeoJSON(s)))));
    }
    public void SetActiveFeature(object activatedFeature)
    {
        activeFeature = (WFSFeature)activatedFeature;
    }
    private IEnumerator GetWebString(string url, Action<string> action)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            string result = request.downloadHandler.text;
            action.Invoke(result); 
        }
    }
    private IEnumerator HandleFeatureJSON(GeoJSON geoJSON)
    {
        ShiftLineColor();
        SpawnParent = new GameObject().transform;
        SpawnParent.name = "WFS_ObjectParent";
        wfsParentEvent.Invoke(SpawnParent.gameObject);

        DateTime dateTime = DateTime.UtcNow;

        Debug.Log("Handling Feature JSON!");
        if (geoJSON.FindFirstFeature())
        {
            Debug.Log("Hadn't found first feature! Doing it now!");

            featureData = new WFSFeatureData();
            featureData.TransferDictionary(geoJSON.GetProperties());
            activeFeature.AddNewFeatureData(featureData);

            EvaluateGeoType(geoJSON);


        }
        while (geoJSON.GotoNextFeature())
        {
            Debug.Log("Evaluating next feature!");

            featureData = new WFSFeatureData();
            featureData.TransferDictionary(geoJSON.GetProperties());
            activeFeature.AddNewFeatureData(featureData);

            EvaluateGeoType(geoJSON);

            if ((DateTime.UtcNow - dateTime).Milliseconds > coroutineRunTime)
            {
                yield return null;
                dateTime = DateTime.UtcNow;
            }
        }
        if (propertyFeatureEvent)
            propertyFeatureEvent.Invoke(activeFeature);
    }
    private void SetStartIndex(string index)
    {
        int.TryParse(index, out ActiveWFS.StartIndex);
    }

    private void SetWebFeatureCount(string count)
    {
        int.TryParse(count, out ActiveWFS.Count);
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
                multiPointEvent.Invoke(pointHandler.ProcessMultiPoint(geoJSON.GetMultiPoint()));
                break;
            case GeoJSON.GeoJSONGeometryType.LineString:
                LineStringHandler lineStringHandler = new LineStringHandler();
                //ShiftLineColor();
                drawLineEvent.Invoke(lineStringHandler.ProcessLineString(geoJSON.GetGeometryLineString()));
                break;
            case GeoJSON.GeoJSONGeometryType.MultiLineString:
                MultiLineHandler multiLineHandler = new MultiLineHandler();
                drawLinesEvent.Invoke(multiLineHandler.ProcessMultiLine(geoJSON.GetMultiLine()));
                break;
            case GeoJSON.GeoJSONGeometryType.Polygon:
                PolygonHandler polyHandler = new PolygonHandler();
                drawPolygonEvent.Invoke(polyHandler.ProcessPolygon(geoJSON.GetPolygon()));
                break;
            case GeoJSON.GeoJSONGeometryType.MultiPolygon:
                MultiPolygonHandler multiPolyHandler = new MultiPolygonHandler();
                drawPolygonsEvent.Invoke(multiPolyHandler.GetMultiPoly(geoJSON.GetMultiPolygon()));
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
    private void ClearSpawnedMeshItems()
    {
        if (SpawnParent == null)
            return;

        for (int i = SpawnParent.childCount - 1; i >= 0; i--)
        {
            Destroy(SpawnParent.GetChild(i).gameObject);
        }
    }
    private void ProcessWFS(string xmlData)
    {
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlData);

        XmlElement serviceID = xml.DocumentElement["ows:ServiceIdentification"]["ows:ServiceType"];
        if (serviceID != null && serviceID.InnerText.Contains("WFS"))
        {
            //print(serviceID.InnerText);
            if (formatter == null)
            {
                formatter = new WFSFormatter();
            }
            ActiveWFS = formatter.ReadFromWFS(ActiveWFS, xml);
            resetReaderEvent.Invoke();
            isWmsEvent.Invoke(false);
            wfsDataEvent.Invoke(ActiveWFS);
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
        if(hue > 1)
        {
            hue = 0;
            saturation += 0.1f;
            if(saturation > 1)
            {
                saturation = 0;
                brightness += 0.1f;
                if(brightness > 1)
                {
                    brightness = 0;
                }
            }
        }
        lineColorEvent.Invoke(Color.HSVToRGB(hue, saturation, brightness));
    }

    private void TestMultiLine(List<IList<Vector3>> multiLine)
    {
        //ShiftLineColor();
        foreach(List<Vector3> lines in multiLine)
        {
            drawLineEvent.Invoke(lines);
        }
    }
    private void TestPoint(Vector3 pointCoord)
    {
        //Destroy(SpawnParent.gameObject);
        //SpawnParent = new GameObject().transform;
        //SpawnParent.name = "WFS_ObjectParent";
        GameObject wfsPointObject = Instantiate(visualizer, pointCoord + Vector3.up * 100, Quaternion.identity, SpawnParent);
    }
    private void TestMultiPoints(List<Vector3> pointCoords)
    {
        //Destroy(SpawnParent.gameObject);
        //SpawnParent = new GameObject().transform;
        //SpawnParent.name = "WFS_ObjectParent";
        foreach (Vector3 position in pointCoords)
        {
            GameObject wfsPointObject = Instantiate(visualizer, position + Vector3.up * 100, Quaternion.identity, SpawnParent);
        }
    }

}
