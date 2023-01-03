using Netherlands3D.Core;
using Netherlands3D.Events;
using Netherlands3D.Utilities;
using Netherlands3D.WFSHandlers;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class WFSHandler : MonoBehaviour
{
    public WFS ActiveWFS { get; private set; }
    public Transform SpawnParent { get; private set; }
    [SerializeField] private GameObject visualizer;

    [Header("Invoked Events")]
    [SerializeField] private ObjectEvent wfsDataEvent;
    [SerializeField] private BoolEvent isWmsEvent;
    [SerializeField] private Vector3ListsEvent drawPolygonsEvent;
    [SerializeField] private Vector3ListEvent drawPolygonEvent;
    [SerializeField] private Vector3ListEvent drawLineEvent;
    [SerializeField] private Vector3ListsEvent drawLinesEvent;
    [SerializeField] private Vector3Event pointEvent;
    [SerializeField] private Vector3ListEvent multiPointEvent;

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


    private bool foundFirstFeature;
    private WFSFormatter formatter;
    private WFSFeature activeFeature;

    private void Awake()
    {
        SpawnParent = new GameObject().transform;
        SpawnParent.name = "WFS_ObjectParent";
        wfsCreationEvent.started.AddListener(CreateWFS);
        //getFeatureEvent.started.AddListener(GetFeature);
        startIndexEvent.started.AddListener(SetStartIndex);
        countEvent.started.AddListener(SetWebFeatureCount);
        setActiveFeatureEvent.started.AddListener(SetActiveFeature);

        wfsMinXEvent.started.AddListener(SetBoundingBoxMinX);
        wfsMaxXEvent.started.AddListener(SetBoundingBoxMaxX);
        wfsMinYEvent.started.AddListener(SetBoundingBoxMinY);
        wfsMaxYEvent.started.AddListener(SetBoundingBoxMaxY);

        // The addition of these two functions to the events are only for testing and debugging purposes.
        // They should be removed or commented out later.
        pointEvent.started.AddListener(TestPoint);
        multiPointEvent.started.AddListener(TestMultiPoints);
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
        Debug.Log("Getting Feature!");
        ActiveWFS.TypeName = activeFeature.FeatureName;
        Debug.Log(ActiveWFS.GetFeatures());
        StartCoroutine(GetWebString(ActiveWFS.GetFeatures(), (string s) => StartCoroutine(HandleFeatureJSON(new GeoJSON(s)))));
    }
    public void SetActiveFeature(object activatedFeature)
    {
        activeFeature = (WFSFeature)activatedFeature;
        foundFirstFeature = false;
    }
    private IEnumerator GetWebString(string url, System.Action<string> action)
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
            //Debug.Log("getting and acting on WebString!");
            StopCoroutine("GetWebString");
        }
    }
    private IEnumerator HandleFeatureJSON(GeoJSON geoJSON)
    {
        Debug.Log("Handling Feature JSON!");
        if (!foundFirstFeature)
        {
            if (geoJSON.FindFirstFeature())
            {
                Debug.Log("Hadn't found first feature! Doing it now!");
                foundFirstFeature = true;
                EvaluateGeoType(geoJSON);
                //yield return null;
            }
            else
            {
                StopCoroutine("HandleFeatureJSON");
            }
        }
        if(geoJSON.GotoNextFeature())
        {
            //if (geoJSON.GotoNextFeature())
            //{
            Debug.Log("Evaluating next feature!");
            EvaluateGeoType(geoJSON);
            yield return new WaitForSeconds(0.2f);
            StartCoroutine(HandleFeatureJSON(geoJSON));
                //yield return null;
            //}
            //else
            //{
            //    StopCoroutine("HandleFeatureJSON");
            //}
        }
    }
    private void SetStartIndex(string index)
    {
        ActiveWFS.StartIndex = int.Parse(index);
    }

    private void SetWebFeatureCount(string count)
    {
        ActiveWFS.Count = int.Parse(count);
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
                drawLineEvent.Invoke(lineStringHandler.ProcessLineString(geoJSON.GetGeometryLineString()));
                //throw new System.NotImplementedException("Geometry Type of type: 'LineString' is not currently supported");
                break;
            case GeoJSON.GeoJSONGeometryType.MultiLineString:
                MultiLineHandler multiLineHandler = new MultiLineHandler();
                drawLinesEvent.Invoke(multiLineHandler.ProcessMultiLine(geoJSON.GetMultiLine()));
                throw new System.NotImplementedException("Geometry Type of type: 'MultiLineString' is not currently supported");
                //break;
            case GeoJSON.GeoJSONGeometryType.Polygon:
                PolygonHandler polyHandler = new PolygonHandler();
                drawPolygonEvent.Invoke(polyHandler.ProcessPolygon(geoJSON.GetPolygon()));
                break;
            case GeoJSON.GeoJSONGeometryType.MultiPolygon:
                MultiPolygonHandler multiPolyHandler = new MultiPolygonHandler();
                drawPolygonsEvent.Invoke(multiPolyHandler.GetMultiPoly(geoJSON.GetMultiPolygon()));
                break;
            case GeoJSON.GeoJSONGeometryType.GeometryCollection:
                throw new System.NotImplementedException("Geometry Type of type: 'GeometryCollection' is not currently supported");
                //break;
            default: 
                throw new System.Exception("Geometry Type is either 'Undefined' or not found, cannot process like this!");

        }
    }
    private void ClearSpawnedMeshItems()
    {
        for (int i = SpawnParent.childCount - 1; i >= 0; i--)
        {
            Destroy(SpawnParent.GetChild(i));
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

    private void TestPoint(Vector3 pointCoord)
    {
        Destroy(SpawnParent);
        SpawnParent = new GameObject().transform;
        SpawnParent.name = "WFS_ObjectParent";
        Instantiate(visualizer, pointCoord + Vector3.up * 100, Quaternion.identity, SpawnParent);
    }
    private void TestMultiPoints(List<Vector3> pointCoords)
    {
        Destroy(SpawnParent);
        SpawnParent = new GameObject().transform;
        SpawnParent.name = "WFS_ObjectParent";
        foreach (Vector3 position in pointCoords)
        {
            Instantiate(visualizer, position + Vector3.up * 100, Quaternion.identity, SpawnParent);
        }
    }

}
