using Netherlands3D.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class WFS : IWebService
{

    public static WFS ActiveInstance { get; private set; }
    public string TypeName;
    public string Version { get; set; }
    public List<WFSFeature> features { get; set; }
    public string BaseUrl { get; private set; }


    private int startIndex = 0;
    private int count = 5;
    private WFSHandler owner;


    public WFS(string baseUrl)
    {
        BaseUrl = baseUrl;
        ActiveInstance = this;
        features = new();
    }
    public void SetHandler(WFSHandler handler)
    {
        owner = handler;
    }

    public void GetCapabilities()
    {    
        WebServiceNetworker wsn = WebServiceNetworker.Instance;
        wsn.StartCoroutine(wsn.GetWebString(BaseUrl + "?request=getcapabilities&service=wfs"));
    }

    public void GetFeature()
    {
        WebServiceNetworker wsn = WebServiceNetworker.Instance;
        wsn.WebStringEvent.started.AddListener(HandleFeatureJSON);
        Debug.Log(GetFeatureRequest());
        wsn.StartCoroutine(wsn.GetWebString(GetFeatureRequest()));
    }
    private void HandleFeatureJSON(string json)
    {
        GeoJSON geojson = new GeoJSON(json);
        //GeoJSON geoJSON = JsonUtility.FromJson<GeoJSON>(json);
        //geoJSON.geoJSONString = json;
        GameObject template = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        foreach(WFSFeature f in features)
        {
            Debug.Log($"Feature: {f.FeatureName}");
        }
        if (geojson.FindFirstFeature())
        {
            Debug.Log(geojson.FeatureString);
            //foreach (List<List<GeoJSONPoint>> pointListList in geojson.GetMultiPolygon())
            //{
            //    foreach (List<GeoJSONPoint> pointList in pointListList)
            //    {
            //        Vector2 pointCoords = Vector2.zero;

            //        for (int i = 0; i < pointList.Count; i++)
            //        {
            //            GeoJSONPoint p = pointList[i];
            //            pointCoords.x += (float)p.x;
            //            pointCoords.y += (float)p.y;
            //            if (i == pointList.Count - 1)
            //            {
            //                pointCoords = new Vector2(pointCoords.x / pointList.Count, pointCoords.y / pointList.Count);
            //                Debug.Log($"Point coords at: {pointCoords}");
            //                float yOffset = 30f;
            //                Object.Instantiate(template, new Vector3(pointCoords.x, yOffset, pointCoords.y), Quaternion.identity);
            //            }

            //        }
            //        //foreach (GeoJSONPoint p in pointList)
            //        //{
            //        //    Debug.Log($"[{p.x}, {p.y}]\n");
            //        //}
            //    }
            //}
            while (geojson.GotoNextFeature())
            {
                Debug.Log("Found new Feature!");
                foreach (List<List<GeoJSONPoint>> pointListList in geojson.GetMultiPolygon())
                {
                    foreach (List<GeoJSONPoint> pointList in pointListList)
                    {
                        Vector2 pointCoords = Vector2.zero;

                        for (int i = 0; i < pointList.Count; i++)
                        {
                            GeoJSONPoint p = pointList[i];
                            pointCoords.x += (float)p.x;
                            pointCoords.y += (float)p.y;
                            if (i == pointList.Count - 1)
                            {
                                pointCoords = new Vector2(pointCoords.x / pointList.Count, pointCoords.y / pointList.Count);
                                Debug.Log($"Point coords at: {pointCoords}");
                                float yOffset = 30f;
                                Object.Instantiate(template, new Vector3(pointCoords.x, yOffset, pointCoords.y), Quaternion.identity);
                            }

                        }
                        //foreach (GeoJSONPoint p in pointList)
                        //{
                        //    Debug.Log($"[{p.x}, {p.y}]\n");
                        //}
                    }
                }
                Object.Destroy(template);
            }
        }
        else
        {
            Debug.Log("Couldn't find any feature in the WFS!");
        }
    }

    private string GetFeatureRequest()
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
        stringBuilder.Append(countRequest);
        stringBuilder.Append("&");
        stringBuilder.Append(startIndexRequest);

        return stringBuilder.ToString();
    }
    private string featureRequest => "?request=getfeature&service=wfs";
    private string versionRequest => $"version={Version}";
    private string typeNameRequest => $"typename={TypeName}";
    private string outputFormatRequest => "outputFormat=geojson";
    private string countRequest => $"count={count}";
    private string startIndexRequest => $"startindex={startIndex}";
}
