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


    public int StartIndex = 0;
    public int Count = 5;

    public WFS(string baseUrl)
    {
        BaseUrl = baseUrl;
        ActiveInstance = this;
        features = new();
    }

    public string GetCapabilities()
    {    
        return BaseUrl + "?request=getcapabilities&service=wfs";
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
        stringBuilder.Append(countRequest);
        stringBuilder.Append("&");
        stringBuilder.Append(startIndexRequest);

        return stringBuilder.ToString();

    }
    private string featureRequest => "?request=getfeature&service=wfs";
    private string versionRequest => $"version={Version}";
    private string typeNameRequest => $"typename={TypeName}";
    private string outputFormatRequest => "outputFormat=geojson";
    private string countRequest => $"count={Count}";
    private string startIndexRequest => $"startindex={StartIndex}";
}
