using System.Collections.Generic;

namespace Netherlands3D.Geoservice
{
    [System.Serializable]
    public class ImageGeoserviceLayer
    {
        public string Name;
        public string Title;
        public string Abstract;
        public List<string> CRS = new List<string>();
    }
    
}