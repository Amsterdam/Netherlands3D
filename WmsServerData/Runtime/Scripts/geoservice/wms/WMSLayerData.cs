using UnityEngine;
using System.Collections.Generic;

namespace Netherlands3D.wmsServer
{
    [System.Serializable]
    public class WMSLayerData : ImageGeoserviceLayer
    {
        [SerializeField]
        public List<string> CRS = new List<string>();
       

        public List<ImageGeoserviceStyle> styles = new List<ImageGeoserviceStyle>();
    }
}
