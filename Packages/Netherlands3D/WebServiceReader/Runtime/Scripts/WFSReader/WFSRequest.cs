using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WFSRequest
{
    public static string BaseURL = string.Empty;

    private static string version = "2.0.2";

    public static string GetCapabilitiesRequest()
    {
        return BaseURL + "?request=getcapabilities&service=wfs";
    }
    private static void GetValuesFromWFS(WFS wfs)
    {
        version = wfs.Version;
    }

}
