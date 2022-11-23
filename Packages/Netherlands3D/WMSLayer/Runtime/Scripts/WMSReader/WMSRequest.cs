using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;

public static class WMSRequest
{
    public static string BaseURL = "";
    public static List<WMSLayer> ActivatedLayers = new();

    private static string version = "1.3.0";
    private static string crs;
    private static Vector2Int dimensions = new(200, 200);
    private static BoundingBox bbox = BoundingBox.Zero;

    private static bool isPreview;

    public static string GetCapabilitiesRequest()
    {
        return BaseURL + "?request=getcapabilities&service=wms";
    }
    public static string GetMapRequest(WMS wms, bool preview)
    {
        isPreview = preview;
        GetValuesFromWMS(wms);
        return StandardRequest();
    }

    public static string GetMapRequest(WMS wms, string srs, bool preview)
    {
        isPreview = preview;
        GetValuesFromWMS(wms);
        return StandardRequest() + "&" + SRSRequest(srs);
    }
    private static void GetValuesFromWMS(WMS wms)
    {
        version = wms.Version;
        dimensions = wms.Dimensions;
        bbox = wms.BBox;
        crs = wms.CRS;
    }

    private static string StandardRequest()
    {
        if (ActivatedLayers.Count == 0)
        {
            throw new System.NullReferenceException("No layers have been activated! A request can't be made like this!");
        }
        StringBuilder requestBuilder = new StringBuilder();

        requestBuilder.Append(BaseURL + "?");
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


    private static string LayerAndStyleRequest()
    {
        StringBuilder layerBuilder = new StringBuilder();
        layerBuilder.Append("layers=");

        StringBuilder styleBuilder = new StringBuilder();
        styleBuilder.Append("styles=");

        ActivatedLayers = ActivatedLayers.OrderByDescending(l => l.styles.Count).ToList();
        foreach(WMSLayer l in ActivatedLayers)
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
            if(current.activeStyle != null)
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

    private static string MapRequest() => "request=getmap";
    private static string VersionRequest() => $"version={version}";
    private static string CRSRequest() => $"crs={crs}";
    private static string DimensionRequest() => $"width={dimensions.x}&height={dimensions.y}";
    private static string BoundingBoxRequest() => isPreview ? $"bbox={bbox.MinX},{bbox.MinY},{bbox.MaxX},{bbox.MaxY}" : "bbox={Xmin},{Ymin},{Xmax},{Ymax}";
    private static string FormatRequest() => "format=image/png";
    private static string TransparencyRequest() => "transparent=true";
    private static string ServiceRequest() => "service=wms";
    private static string SRSRequest(string srs) => $"srs={srs}";

}
