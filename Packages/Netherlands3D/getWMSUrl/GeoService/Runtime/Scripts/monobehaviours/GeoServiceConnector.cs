using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Geoservice;
using System.Reflection;
using System.Linq;
using Netherlands3D.Events;

namespace Netherlands3D.Geoservice
{
    public class GeoServiceConnector : MonoBehaviour
    {
        // Start is called before the first frame update
        [HideInInspector]
        public ServerData serverData;

        private List<ImageGeoservice> services = new List<ImageGeoservice>();
        private ImageGeoservice activeService;

        private string _url;
        [SerializeField]
        private string _serviceType = "WMS";

        [Header("Send Events")]
        [Tooltip("message if function can't continue")]
        [SerializeField]
        private StringEvent OnError_String;

        [Tooltip("message when capabilities are read")]
        [SerializeField]
        private TriggerEvent OnCapabilitiesParsed_Trigger;

        [Tooltip("message when url is changed")]
        [SerializeField]
        private StringEvent OnURLChanged_String;
        [Tooltip("lijst met beschikbare geoservices\nwordt bijgewerkt wanneer de game gestart wordt")]
        public List<string> availableWebServices = new List<string>();


        private void Start()
        {
            getAvailableImageGeoServices();
            SetupListeners();
            if (_serviceType != "")
            {
                SetActiveService(_serviceType);
            }
        }

        private void SetupListeners()
        {
            //no listeners yet
        }


        public void setWebSericeURL(string url)
        {
            if (serverData != null)
            {
                serverData = null;
            }
            _url = url;
            if (OnURLChanged_String != null)
            {
                OnURLChanged_String.InvokeStarted(_url);
            }
        }



        public void ConnectToWebservice(string value)
        {

            setWebSericeURL(value);
            ConnectToWebservice();
        }
        public void ConnectToWebservice()
        {
            serverData = new ServerData();

            GetTextDataFromURL();
        }


        void getAvailableImageGeoServices()
        {
            services.Clear();
            availableWebServices = new List<string>();
            var types = Assembly
                        .GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => typeof(ImageGeoservice).IsAssignableFrom(t) &&
                            t != typeof(ImageGeoservice))
                        .ToArray();

            foreach (var item in types)
            {
                ImageGeoservice newService = (ImageGeoservice)System.Activator.CreateInstance(item);
                services.Add(newService);
                availableWebServices.Add(newService.getType());
            }
        }


        private void GetTextDataFromURL()
        {
            StartCoroutine(connectToServer(_url));
        }

        public bool SetActiveService(string serviceType)
        {
            for (int i = 0; i < services.Count; i++)
            {

                if (serviceType == services[i].getType())
                {
                    activeService = services[i];
                    return true;
                }
            }
            return false;
        }

        private IEnumerator connectToServer(string url)
        {
            var serverRequest = UnityWebRequest.Get(url);
            yield return serverRequest.SendWebRequest();

            if (serverRequest.result == UnityWebRequest.Result.Success)
            {
                bool succes = readXML(serverRequest.downloadHandler.text);

                if (succes)
                {
                    if (OnCapabilitiesParsed_Trigger != null)
                    {
                        OnCapabilitiesParsed_Trigger.InvokeStarted();
                    }
                }
                else
                {
                    if (OnError_String != null)
                    {
                        OnError_String.InvokeStarted($"unable to read the Capabilties at {url}");
                    }
                }


            }
            else
            {
                if (OnError_String != null)
                {
                    OnError_String.InvokeStarted($"can't connect to {url}");
                }
            }
            yield return null;
        }
        public bool readXML(string xmlData)
        {
            if (activeService.readCapabilities(serverData, xmlData))
            {
                return true;
            }
            return false;

            //
        }
    }
}