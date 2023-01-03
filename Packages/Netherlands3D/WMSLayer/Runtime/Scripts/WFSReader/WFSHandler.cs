using Netherlands3D.Events;
using Netherlands3D.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class WFSHandler : MonoBehaviour
{
    public WFS ActiveWFS { get; private set; }
    public Transform SpawnParent { get; private set; }

    [Header("Invoked Events")]
    [SerializeField] private ObjectEvent wfsDataEvent;
    [SerializeField] private BoolEvent isWmsEvent;

    [Header("Listen Events")]
    [SerializeField] private StringEvent startIndexEvent;
    [SerializeField] private StringEvent countEvent;
    [SerializeField] private TriggerEvent resetReaderEvent;
    [SerializeField] private StringEvent urlEvent;
    [SerializeField] private StringEvent wfsCreationEvent;
    //[SerializeField] private StringEvent getFeatureEvent;
    [SerializeField] private ObjectEvent setActiveFeatureEvent;

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
    }
    public void CreateWFS(string baseUrl)
    {
        ActiveWFS = new WFS(baseUrl);
        ClearSpawnedMeshItems();
        resetReaderEvent.Invoke();
        StartCoroutine(GetWebString(ActiveWFS.GetCapabilities(), ProcessWFS));
    }
    public void GetFeature()
    {
        Debug.Log("Getting Feature!");
        ActiveWFS.TypeName = activeFeature.FeatureName;
        Debug.Log(ActiveWFS.GetFeatures());
        StartCoroutine(GetWebString(ActiveWFS.GetFeatures(), (string s) => StartCoroutine(HandleFeatureJSON(s))));
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
            Debug.Log("getting and acting on WebString!");
            StopCoroutine("GetWebString");
        }
    }
    private IEnumerator HandleFeatureJSON(string json)
    {
        Debug.Log("Handling Feature JSON!");
        GeoJSON geoJSON = new GeoJSON(json);
        if (!foundFirstFeature)
        {
            if (geoJSON.FindFirstFeature())
            {
                Debug.Log("Hadn't found first feature! Doing it now!");
                foundFirstFeature = true;
                EvaluateGeoType(geoJSON);
                yield return null;
            }
        }
        else
        {
            if (geoJSON.GotoNextFeature())
            {
                Debug.Log("Evaluating next feature!");
                EvaluateGeoType(geoJSON);
                yield return null;
            }
            else
            {
                StopCoroutine("HandleFeatureJSON");
            }
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

                throw new System.NotImplementedException("Geometry Type of type: 'Point' is not currently supported");
                //break;
            case GeoJSON.GeoJSONGeometryType.MultiPoint:
                MultiPointHandler pointHandler = new MultiPointHandler(this);
                pointHandler.ProcessMultiPoint(geoJSON.GetMultiPoint());
                break;
            case GeoJSON.GeoJSONGeometryType.LineString:
                throw new System.NotImplementedException("Geometry Type of type: 'LineString' is not currently supported");
                //break;
            case GeoJSON.GeoJSONGeometryType.MultiLineString:
                throw new System.NotImplementedException("Geometry Type of type: 'MultiLineString' is not currently supported");
                //break;
            case GeoJSON.GeoJSONGeometryType.Polygon:
                PolygonHandler polyHandler = new PolygonHandler(this);
                polyHandler.ProcessPolygon(geoJSON.GetPolygon());
                break;
            case GeoJSON.GeoJSONGeometryType.MultiPolygon:
                MultiPolygonHandler multiPolyHandler = new MultiPolygonHandler(this);
                multiPolyHandler.ProcessMultiPolygon(geoJSON.GetMultiPolygon());
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
            print(serviceID.InnerText);
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
}