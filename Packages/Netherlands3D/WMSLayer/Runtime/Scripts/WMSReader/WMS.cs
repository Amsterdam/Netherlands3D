using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WMS
{
    public string Version { get; private set; }
    public string CRS = "epsg:28992";
    public Vector2Int Dimensions = new Vector2Int(225, 225);
    public BoundingBox BBox = BoundingBox.Zero;
    public List<WMSLayer> layers { get; private set; }

    public WMS(string version)
    {
        Version = version;
        layers = new();
    }

}
