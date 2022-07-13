using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Events;
using UnityEngine.Networking;
using System;

namespace Netherlands3D.SensorThings
{
    public class SensorThingsRIVM : MonoBehaviour
    {
        [SerializeField]
        private string baseApiURL = "https://api-samenmeten.rivm.nl/v1.0";

        void Start()
        {
            CheckAvailabilityAPI();
        }

        private void CheckAvailabilityAPI()
        {
            StartCoroutine(RequestAPI(baseApiURL,(success,message) => { Debug.Log($"API Available:{success}", this.gameObject); } ));
        }

        public void GetThings(Action<bool,Things> callback)
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/Things", (success,text) =>
            {
                if(success)
                {
                    var json = JSON.Parse(text);
                    var things = new Things();
                    things.iotnextLink = json["@iot.nextLink"];
                    //things.value = json["@iot.nextLink"];


                    callback(true,things);
                }
                else
                {
                    callback(false,null);
                }
            }));
        }
        public void GetDatastreams()
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/Datastreams", ReceivedSensors));
        }
        public void GetLocations()
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/GetLocations", ReceivedSensors));
        }
        public void Observations()
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/Observations", ReceivedSensors));
        }
        public void HistoricalLocations()
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/HistoricalLocations", ReceivedSensors));
        }
        public void ObservedProperties()
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/ObservedProperties", ReceivedSensors));
        }
        public void GetSensors()
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/Sensors", ReceivedSensors));
        }

        /*
         {
   "value": [
      {
         "name": "Things",
         "url": "https://api-samenmeten.rivm.nl/v1.0/Things"
      },
      {
         "name": "Datastreams",
         "url": "https://api-samenmeten.rivm.nl/v1.0/Datastreams"
      },
      {
         "name": "Locations",
         "url": "https://api-samenmeten.rivm.nl/v1.0/Locations"
      },
      {
         "name": "FeaturesOfInterest",
         "url": "https://api-samenmeten.rivm.nl/v1.0/FeaturesOfInterest"
      },
      {
         "name": "Observations",
         "url": "https://api-samenmeten.rivm.nl/v1.0/Observations"
      },
      {
         "name": "HistoricalLocations",
         "url": "https://api-samenmeten.rivm.nl/v1.0/HistoricalLocations"
      },
      {
         "name": "ObservedProperties",
         "url": "https://api-samenmeten.rivm.nl/v1.0/ObservedProperties"
      },
      {
         "name": "Sensors",
         "url": "https://api-samenmeten.rivm.nl/v1.0/Sensors"
      }
   ]
}
        */

        private IEnumerator RequestAPI(string uri, Action<bool,string> callback)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(webRequest.error);
                        callback(false, webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(webRequest.error);
                        callback(false, webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        callback(true,webRequest.downloadHandler.text);
                        break;
                }
            }
        }

        public void StopRequests()
        {
            StopAllCoroutines();
        }
    }
}

