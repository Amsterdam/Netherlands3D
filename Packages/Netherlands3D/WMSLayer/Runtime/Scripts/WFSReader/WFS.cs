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

    public BoundingBox BBox;

    public int StartIndex = 0;
    public int Count = 100;

    private bool tileHandled;

    public WFS(string baseUrl)
    {
        BaseUrl = baseUrl;
        ActiveInstance = this;
        features = new();
    }

    public string GetCapabilities()
    {    
        return BaseUrl + "?REQUEST=GetCapabilities&SERVICE=WFS";
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
}
