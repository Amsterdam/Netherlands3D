using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using UnityEditor;
using UnityEngine.Events;
using System.Reflection;
using System.Linq;

namespace Netherlands3D.Geoservice
{
    [System.Serializable]
    public class ServerData 
    {

        public List<ImageGeoservice> services = new List<ImageGeoservice>();
        public ImageGeoservice activeService;
        [Header("ServerSelection")]
        public List<string> availableServiceTypes = new List<string>();
        [SerializeField]
        

        [Header("service")]
        public string ServiceName;
        public string ServiceTitle;
        [TextArea]
        public string ServiceAbstract;
        public string maxWidth;
        public string maxHeight;
        public List<string> fileFormats = new List<string>();
        public string TemplateURL;
        [Header("layers")]
        public List<string> globalCRS = new List<string>();
        public List<ImageLayerData> layer = new List<ImageLayerData>();





        

        

        
       
    }
    
}