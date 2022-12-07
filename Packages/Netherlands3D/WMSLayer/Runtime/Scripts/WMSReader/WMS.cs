using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using Netherlands3D.Events;

public class WMS : IWebService, IWSMappable
{
    public static WMS ActiveInstance { get; private set; }
    public string Version { get; private set; }
    public string CRS { get; private set; }
    public string SRS { get; private set; }

    public Vector2Int Resolution = new Vector2Int(225, 225);
    public BoundingBox BBox = BoundingBox.Zero;
    public List<WMSLayer> Layers { get; private set; }
    public List<WMSLayer> ActivatedLayers { get; private set; }
    public string BaseUrl { get; private set; }

    private bool isPreview;
    private bool requiresSRS;
    private StringEvent capabilitiesEvent;

    public WMS(string baseUrl)
    {
        BaseUrl = baseUrl;
        Layers = new();
        ActivatedLayers = new();
        ActiveInstance = this;
    }
    public void SetVersion(string version)
    {
        Version = version;
    }
    public void SetCRS(string crs)
    {
        CRS = crs;
    }

    public void IsPreview(bool isPreview)
    {
        this.isPreview = isPreview;
    }
    public void RequiresSRS(bool required, string srs = "")
    {
        requiresSRS = required;
        SRS = srs;
    }
    public void GetCapabilities()
    {
        WebServiceNetworker wsn = WebServiceNetworker.Instance;
        wsn.StartCoroutine(wsn.GetWebString(BaseUrl + "?request=getcapabilities&service=wms"));
    }
    public string GetMapRequest()
    {
        return StandardRequest() + (requiresSRS ? SRSRequest(SRS) : "");
    }
    public void ActivateLayer(WMSLayer layerToActivate)
    {
        ActivatedLayers.Add(layerToActivate);
    }
    public void DeactivateLayer(WMSLayer layerToDeactivate)
    {
        ActivatedLayers.Remove(layerToDeactivate);
    }
    private string StandardRequest()
    {
        if (ActivatedLayers.Count == 0)
        {
            throw new System.NullReferenceException("No layers have been activated! A request can't be made like this!");
        }
        StringBuilder requestBuilder = new StringBuilder();

        requestBuilder.Append(BaseUrl + "?");
        requestBuilder.Append(MapRequest());
        requestBuilder.Append("&");
        requestBuilder.Append(LayerAndStyleRequest());
        requestBuilder.Append("&");
        requestBuilder.Append(VersionRequest());
        requestBuilder.Append("&");
        requestBuilder.Append(CRSRequest());
        requestBuilder.Append("&");
        requestBuilder.Append(DimensionRequest());
        requestBuilder.Append("&");
        requestBuilder.Append(BoundingBoxRequest());
        requestBuilder.Append("&");
        requestBuilder.Append(FormatRequest());
        requestBuilder.Append("&");
        requestBuilder.Append(TransparencyRequest());
        requestBuilder.Append("&");
        requestBuilder.Append(ServiceRequest());

        Debug.Log(requestBuilder.ToString());
        return requestBuilder.ToString();
    }


    private string LayerAndStyleRequest()
    {
        StringBuilder layerBuilder = new StringBuilder();
        layerBuilder.Append("layers=");

        StringBuilder styleBuilder = new StringBuilder();
        styleBuilder.Append("styles=");

        ActivatedLayers = ActivatedLayers.OrderByDescending(l => l.styles.Count).ToList();
        foreach (WMSLayer l in ActivatedLayers)
        {
            Debug.Log(l.Title + l.styles.Count);
        }

        for (int i = 0; i < ActivatedLayers.Count; i++)
        {
            WMSLayer current = ActivatedLayers[i];
            //if (current.activeStyle == null)
            //{
            //    throw new System.NullReferenceException($"Layer: {current.Title} has no active style selected and cannot have the request finished!");
            //}
            layerBuilder.Append(current.Name);
            if (current.activeStyle != null)
            {
                styleBuilder.Append(current.activeStyle.Name);
            }
            if (i != ActivatedLayers.Count - 1)
            {
                layerBuilder.Append(",");
                styleBuilder.Append(",");
            }
        }
        return $"{layerBuilder}&{styleBuilder}";
    }

    private string MapRequest() => "request=getmap";
    private string VersionRequest() => $"version={Version}";
    private string CRSRequest() => $"crs={CRS}";
    private string DimensionRequest() => isPreview ? $"width={Resolution.x}&height={Resolution.y}" : "width={Width}&height={Height}";
    private string BoundingBoxRequest() => isPreview ? $"bbox={BBox.MinX},{BBox.MinY},{BBox.MaxX},{BBox.MaxY}" : "bbox={Xmin},{Ymin},{Xmax},{Ymax}";
    private string FormatRequest() => "format=image/png";
    private string TransparencyRequest() => "transparent=true";
    private string ServiceRequest() => "service=wms";
    private string SRSRequest(string srs) => $"srs={srs}";

}
