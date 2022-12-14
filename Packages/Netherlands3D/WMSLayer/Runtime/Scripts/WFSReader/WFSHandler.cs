using Netherlands3D.Events;
using Netherlands3D.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class WFSHandler : MonoBehaviour
{
    public WFS ActiveWFS { get; private set; }
    public Transform SpawnParent { get; private set; }

    [Header("Listen Events")]
    [SerializeField] private StringEvent urlEvent;

    private bool foundFirstFeature;
    private WFSFormatter formatter;
    private TriggerEvent resetReaderEvent;
    private BoolEvent isWMSEVent;

    private void Awake()
    {
        SpawnParent = new GameObject().transform;
        SpawnParent.name = "WFS_ObjectParent";
        urlEvent.started.AddListener(CreateWFS);
    }
    public void CreateWFS(string baseUrl)
    {
        ActiveWFS = new WFS(baseUrl);
        ActiveWFS.SetHandler(this);
        foundFirstFeature = false;
        for(int i = SpawnParent.childCount - 1; i >= 0; i--)
        {
            Destroy(SpawnParent.GetChild(i));
        }
        resetReaderEvent.Invoke();
        WebServiceNetworker.Instance.WebStringEvent.started.AddListener(ProcessWFS);
        ActiveWFS.GetCapabilities();
    }
    //public void GetFeature(string typeName)
    //{
    //    WFS.ActiveInstance.TypeName = typeName;
    //    WFS.ActiveInstance.GetFeature();
    //}

    private IEnumerator HandleFeatureJSON(string json)
    {
        GeoJSON geoJSON = new GeoJSON(json);
        if (!foundFirstFeature)
        {
            if (geoJSON.FindFirstFeature())
            {
                foundFirstFeature = true;
                EvaluateGeoType(geoJSON);
                yield return null;
            }
        }
        else
        {
            if (geoJSON.GotoNextFeature())
            {
                EvaluateGeoType(geoJSON);
                yield return null;
            }
            else
            {
                StopCoroutine("HandleFeatureJSON");
            }
        }
    }

    private void EvaluateGeoType(GeoJSON geoJSON)
    {
        switch (geoJSON.GetGeometryType())
        {
            case GeoJSON.GeoJSONGeometryType.Point:

                break;
            case GeoJSON.GeoJSONGeometryType.MultiPoint:

                break;
            case GeoJSON.GeoJSONGeometryType.LineString:

                break;
            case GeoJSON.GeoJSONGeometryType.MultiLineString:

                break;
            case GeoJSON.GeoJSONGeometryType.Polygon:

                break;
            case GeoJSON.GeoJSONGeometryType.MultiPolygon:
                MultiPolygonHandler multiPolyHandler = new MultiPolygonHandler(this);
                multiPolyHandler.ProcessMultiPolygon(geoJSON.GetMultiPolygon());
                break;
            case GeoJSON.GeoJSONGeometryType.GeometryCollection:

                break;
            default: 
                throw new System.Exception("Geometry Type is either 'Undefined' or not found, cannot process like this!");

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
            isWMSEVent.Invoke(false);
        }
    }
}
