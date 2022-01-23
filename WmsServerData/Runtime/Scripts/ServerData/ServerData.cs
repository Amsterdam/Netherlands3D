using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using UnityEditor;
using UnityEngine.Events;

namespace Netherlands3D.wmsServer
{
    [CreateAssetMenu(fileName = "ServerData", menuName = "ScriptableObjects/wmsServer/ServerData", order = 0)]
    public class ServerData : ScriptableObject
    {

        public List<ImageGeoservice> services = new List<ImageGeoservice>();
        private ImageGeoservice activeService;
        [Header("ServerSelection")]
        public List<string> availableServices = new List<string>();
        [TextArea]
        [SerializeField]
        private string _getCapabilitiesURL;
        [SerializeField]
        private bool _getCapabilitiesURL_AppearsValid;

        public BoolEvent On_getCapabilitiesURL_AppearsValid_Changed;

        public BoolEvent On_AttemptedServerconnection;

        public BoolEvent On_ServerDataParsed;

        [Header("service")]
        public string ServiceName;
        public string ServiceTitle;
        [TextArea]
        public string ServiceAbstract;
        public string maxWidth;
        public string maxHeight;
        public List<string> fileFormats = new List<string>();
        public string GetMapURL;
        [Header("layers")]
        public List<string> globalCRS = new List<string>();
        public List<WMSLayerData> layer;


        [HideInInspector]
        public UnityEvent loadGetCapabilities = new UnityEvent();
        private void CreateNewCapabilitiesIsValidEvent()
        {
            Debug.Log("would be nice but don't know how to (yet)");
        }

        
        public bool getCapabilitiesURLIsValid
        {
            get { return _getCapabilitiesURL_AppearsValid; }
        }

        public string getCapabilitiesURL
        {
            get { return _getCapabilitiesURL; }
            set { 
                _getCapabilitiesURL = value;
                bool isValid = checkURLViability(_getCapabilitiesURL);
                if (_getCapabilitiesURL_AppearsValid!=isValid)
                {
                    _getCapabilitiesURL_AppearsValid = isValid;
                    if (On_getCapabilitiesURL_AppearsValid_Changed!=null)
                    {
                        On_getCapabilitiesURL_AppearsValid_Changed.started.Invoke(isValid);
                    }
                    
                }
            }
        }

        private bool checkURLViability(string url)
        {

            foreach (var item in services)
            {
                if (item.UrlIsValid(url)==true)
                {
                    activeService = item;
                    return true;
                }
            }
            return false;
            
        }

        public void ReadCapabilities()
        {
            if (_getCapabilitiesURL_AppearsValid==false)
            {
                Debug.Log("url is not valid");
            }
            Debug.Log("reading capabilities");
            loadGetCapabilities.Invoke();
        }

        public void readXML(string xmlData)
        {
           if(activeService.readCapabilities(this, xmlData))
            {
                On_ServerDataParsed.started.Invoke(true);
            }
           else
            {
                On_ServerDataParsed.started.Invoke(false);
            }

            //
        }
        public void downloadSuccesfull(bool succes)
        {
            if (On_AttemptedServerconnection!=null)
            {
                On_AttemptedServerconnection.started.Invoke(succes);
            }
        }
    }

    [System.Serializable]
    public class ImageGeoserviceLayer
    {
        public string Name;
        public string Title;
        public string Abstract;
    }
    
}