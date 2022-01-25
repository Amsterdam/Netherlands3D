using UnityEngine;
using System.Collections.Generic;

namespace Netherlands3D.Geoservice
{
    [System.Serializable]
    public class ImageLayerData : ImageGeoserviceLayer
    {
         public List<ImageGeoserviceStyle> styles = new List<ImageGeoserviceStyle>();
    }
}
