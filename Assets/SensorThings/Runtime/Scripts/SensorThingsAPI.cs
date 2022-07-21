using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Events;
using UnityEngine.Networking;
using System;

namespace Netherlands3D.SensorThings
{
    [HelpURL("https://gost1.docs.apiary.io/")]
    public class SensorThingsAPI : MonoBehaviour
    {
        [SerializeField]
        private string baseApiURL = "https://api-samenmeten.rivm.nl/v1.0";

        private string municipalityThingsFilter = "?$filter=contains(properties/codegemeente,'municipalityID')";

        void Start()
        {
            CheckAvailabilityAPI();
        }

        /// <summary>
        /// Call root API to check if it is online
        /// </summary>
        private void CheckAvailabilityAPI()
        {
            StartCoroutine(RequestAPI(baseApiURL,(success,message) => { Debug.Log($"API Available:{success}", this.gameObject); } ));
        }

        /// <summary>
        /// Get all locations, or by thing ID
        /// </summary>
        /// <param name="callback">Returns Locations object</param>
        /// <param name="thingID">Optional ID of the Thing to request specific location</param>
        public void GetLocations(Action<bool, Locations> callback, int thingID = 0)
        {
            var specificThing = (thingID > 0) ? $"/Things({thingID})/" : "/";
            StartCoroutine(RequestAPI($"{baseApiURL}{specificThing}Locations", (success, text) => {
                if (success)
                {
                    Locations things = JSONToLocations(text);
                    callback(true, things);
                }
                else
                {
                    callback(false, null);
                }
            }));
        }

        /// <summary>
        /// Get all datastreams
        /// </summary>
        /// <param name="callback">Returns action with success and datastreams object</param>
        /// <param name="thingID">Optional specific thing ID to filter datastreams on</param>
        public void GetDatastreams(Action<bool, Datastreams> callback, int thingID = 0)
        {
            var specificThing = (thingID > 0) ? $"/Things({thingID})/" : "/";
            StartCoroutine(RequestAPI($"{baseApiURL}{specificThing}Datastreams", (success, text) => {
                if (success)
                {
                    Datastreams datastreams = JSONToDatastreams(text);
                    callback(true, datastreams);
                }
                else
                {
                    callback(false, null);
                }
            }));
        }

        /// <summary>
        /// Get filtered observations
        /// </summary>
        /// <param name="callback">Returns success and Observations object</param>
        /// <param name="dataStreamID">Optional specific datastreams to filter on</param>
        /// <param name="from">Filter results after this datetime (inclusive) using day,month and year</param>
        /// <param name="to">Filter results before this datetime using day,month and year</param>
        public void GetObservations(Action<bool, Observations> callback, int dataStreamID = 0, DateTime from = default, DateTime to = default)
        {
            var specificDatastream = (dataStreamID > 0) ? $"/Datastreams({dataStreamID})/" : "/";

            //ge is greater than or equals
            var betweenFilter = $@"?$filter=day(phenomenonTime) ge {from.Day} and month(phenomenonTime) ge {from.Month} and year(phenomenonTime) ge {from.Year} and 
day(phenomenonTime) lt {to.Day} and month(phenomenonTime) lt {to.Month} and year(phenomenonTime) le {to.Year}";

            var filter = (from != default && to != default) ? betweenFilter : "";
            
            StartCoroutine(RequestAPI($"{baseApiURL}{specificDatastream}Observations{filter}", (success, text) => {
                if (success)
                {
                    Observations datastreams = JSONToObservations(text);
                    callback(true, datastreams);
                }
                else
                {
                    callback(false, null);
                }
            }));
        }

        /// <summary>
        /// Get all things from API with optional filters
        /// </summary>
        /// <param name="callback">Returns object if data is retrieved</param>
        public void GetThings(Action<bool,Things> callback, int municipalityID = 0)
        {
            var filter = municipalityThingsFilter = municipalityThingsFilter.Replace("municipalityID", municipalityID.ToString());
            StartCoroutine(RequestAPI($"{baseApiURL}/Things{municipalityThingsFilter}", (success,text) =>
            {
                if(success)
                {
                    Things things = JSONToThings(text);
                    callback(true, things);
                }
                else
                {
                    callback(false,null);
                }
            }));
        }

        public void GetObservedProperties(Action<bool, ObservedProperties> callback)
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/ObservedProperties", (success, text) =>
            {
                if (success)
                {
                    ObservedProperties observedProperties = JSONToObservedProperties(text);
                    callback(true, observedProperties);
                }
                else
                {
                    callback(false, null);
                }
            }));
        }

        public void GetObservedProperty(Action<bool, ObservedProperties.Value> callback, int datastreamID)
        {
            StartCoroutine(RequestAPI($"{baseApiURL}/Datastreams({datastreamID})/ObservedProperty", (success, text) =>
            {
                if (success)
                {
                    ObservedProperties.Value observedProperty = JSONToObservedProperty(text);
                    callback(true, observedProperty);
                }
                else
                {
                    callback(false, null);
                }
            }));
        }

        private static ObservedProperties.Value JSONToObservedProperty(string text)
        {
            var json = JSON.Parse(text);
            var observedProperty = new ObservedProperties.Value
            {
                iotid = json["@iot.id"],
                iotselfLink = json["@iot.selfLink"],
                name = json["name"],
                description = json["description"],
                DatastreamsiotnavigationLink = "Datastreams@iot.navigationLink",

            };
            return observedProperty;
        }

        private static ObservedProperties JSONToObservedProperties(string text)
        {
            var json = JSON.Parse(text);
            var observedProperties = new ObservedProperties();

            var valuesJson = json["value"].AsArray;
            var valuesObjects = new List<ObservedProperties.Value>();
            for (int i = 0; i < valuesJson.Count; i++)
            {

                var jsonValue = valuesJson[i];
                valuesObjects.Add(new ObservedProperties.Value
                {
                    iotid = jsonValue["@iot.id"],
                    iotselfLink = jsonValue["@iot.selfLink"],
                    name = jsonValue["name"],
                    description = jsonValue["description"],
                    DatastreamsiotnavigationLink = "Datastreams@iot.navigationLink",

                });
            }
            observedProperties.value = valuesObjects.ToArray();
            return observedProperties;
        }

        private static Things JSONToThings(string text)
        {
            var json = JSON.Parse(text);
            var things = new Things();
            things.iotnextLink = json["@iot.nextLink"];

            var valuesJson = json["value"].AsArray;
            var valuesObjects = new List<Things.Value>();
            for (int i = 0; i < valuesJson.Count; i++)
            {

                var jsonValue = valuesJson[i];
                var jsonValueProperties = jsonValue["properties"];
                valuesObjects.Add(new Things.Value
                {
                    iotid = jsonValue["@iot.id"],
                    iotselfLink = jsonValue["@iot.selfLink"],
                    name = jsonValue["name"],
                    description = jsonValue["description"],
                    properties = new Things.Properties()
                    {
                        codegemeente = jsonValueProperties["codegemeente"],
                        knmicode = jsonValueProperties["knmicode"],
                        nh3closecode = jsonValueProperties["nh3closecode"],
                        nh3regiocode = jsonValueProperties["nh3regiocode"],
                        nh3stadcode = jsonValueProperties["nh3stadcode"],
                        no2closecode = jsonValueProperties["no2closecode"],
                        no2regiocode = jsonValueProperties["no2regiocode"],
                        no2stadcode = jsonValueProperties["no2stadcode"],
                        owner = jsonValueProperties["owner"],
                        pm10closecode = jsonValueProperties["pm10closecode"],
                        pm10regiocode = jsonValueProperties["pm10regiocode"],
                        pm10stadcode = jsonValueProperties["pm10stadcode"],
                        pm25closecode = jsonValueProperties["pm25closecode"],
                        pm25regiocode = jsonValueProperties["pm25regiocode"],
                        pm25stadcode = jsonValueProperties["pm25stadcode"],
                        project = jsonValueProperties["project"]
                    },
                    LocationsiotnavigationLink = "Locations@iot.navigationLink",
                    DatastreamsiotnavigationLink = "Datastreams@iot.navigationLink",
                    HistoricalLocationsiotnavigationLink = "HistoricalLocations@iot.navigationLink"
                });
            }
            things.value = valuesObjects.ToArray();
            return things;
        }

        private static Locations JSONToLocations(string text)
        {
            var json = JSON.Parse(text);
            var locations = new Locations();
            locations.iotnextLink = json["@iot.nextLink"];
            var valuesJson = json["value"].AsArray;
            var valuesObjects = new List<Locations.Value>();
            for (int i = 0; i < valuesJson.Count; i++)
            {
                var jsonValue = valuesJson[i];
                var jsonValueLocation = jsonValue["location"];
                valuesObjects.Add(new Locations.Value
                {
                    iotid = jsonValue["@iot.id"],
                    iotselfLink = jsonValue["@iot.selfLink"],
                    name = jsonValue["name"],
                    description = jsonValue["description"],
                    location = new Locations.Location()
                    {
                        coordinates = new float[2] { jsonValueLocation["coordinates"][0].AsFloat, jsonValueLocation["coordinates"][1].AsFloat },
                        type = jsonValueLocation["type"]
                    },
                    HistoricalLocationsiotnavigationLink = "HistoricalLocations@iot.navigationLink"
                });
            }
            locations.value = valuesObjects.ToArray();
            return locations;
        }
        private static Datastreams JSONToDatastreams(string text)
        {
            var json = JSON.Parse(text);
            var datastreams = new Datastreams();
            datastreams.iotnextLink = json["@iot.nextLink"];

            var valuesJson = json["value"].AsArray;
            Debug.Log(valuesJson.Count);
            var valuesObjects = new List<Datastreams.Value>();
            for (int i = 0; i < valuesJson.Count; i++)
            {
                var jsonValue = valuesJson[i];
                var jsonValueUnityOfMeasurement = jsonValue["unitOfMeasurement"];
                valuesObjects.Add(new Datastreams.Value
                {
                    iotid = jsonValue["@iot.id"],
                    iotselfLink = jsonValue["@iot.selfLink"],
                    name = jsonValue["name"],
                    description = jsonValue["description"],
                    unitOfMeasurement = new Datastreams.Unitofmeasurement()
                    {
                        symbol = jsonValueUnityOfMeasurement["symbol"]
                    },
                    ObservationsiotnavigationLink = "Observations@iot.navigationLink"
                });
            }
            datastreams.value = valuesObjects.ToArray();
            return datastreams;
        }

        private static Observations JSONToObservations(string text)
        {
            var json = JSON.Parse(text);
            var observations = new Observations();
            observations.iotnextLink = json["@iot.nextLink"];

            var valuesJson = json["value"].AsArray;
            var valuesObjects = new List<Observations.Value>();
            for (int i = 0; i < valuesJson.Count; i++)
            {
                var jsonValue = valuesJson[i];
                var jsonValueUnityOfMeasurement = jsonValue["unitOfMeasurement"];
                valuesObjects.Add(new Observations.Value
                {
                    iotid = jsonValue["@iot.id"],
                    iotselfLink = jsonValue["@iot.selfLink"],
                    phenomenonTime = jsonValue["phenomenonTime"],
                    result = jsonValue["result"].AsFloat,
                    DatastreamiotnavigationLink = "Datastream@iot.navigationLink"
                });
            }
            observations.value = valuesObjects.ToArray();
            return observations;
        }

        public void GetDatastreams()
        {
            throw new NotImplementedException();
            StartCoroutine(RequestAPI($"{baseApiURL}/Datastreams", (success, text) => { }));
        }

        public void Observations()
        {
            throw new NotImplementedException();
            StartCoroutine(RequestAPI($"{baseApiURL}/Observations", (success, text) => { }));
        }
        public void HistoricalLocations()
        {
            throw new NotImplementedException();
            StartCoroutine(RequestAPI($"{baseApiURL}/HistoricalLocations", (success, text) => { }));
        }
        public void ObservedProperties()
        {
            throw new NotImplementedException();
            StartCoroutine(RequestAPI($"{baseApiURL}/ObservedProperties", (success, text) => { }));
        }
        public void GetSensors()
        {
            throw new NotImplementedException();
            StartCoroutine(RequestAPI($"{baseApiURL}/Sensors", (success, text) => { }));
        }

        private IEnumerator RequestAPI(string uri, Action<bool,string> callback)
        {
            Debug.Log(uri);
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(uri + "\n" + webRequest.error);
                        callback(false, webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(uri + "\n" + webRequest.error);
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

