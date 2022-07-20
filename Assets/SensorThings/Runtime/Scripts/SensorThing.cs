using Netherlands3D.Core;
using Netherlands3D.SensorThings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.SensorThings
{
    public class SensorThing : MonoBehaviour
    {
        private SensorThingsAPI sensorThingsAPI;

        private Things.Value thingData;

        private Datastreams dataStreams;
        private Dictionary<Datastreams.Value, SensorThingCombinedData> datastreamObservations = new Dictionary<Datastreams.Value, SensorThingCombinedData>();

        [SerializeField]
        private TextMesh textMesh;

        private string observedPropertyFilter;

        private DateTime fromDateTime;
        private DateTime toDateTime;

        private void Start()
        {
            textMesh.text = "Loading";
        }

        private void Update()
        {
            textMesh.transform.LookAt(Camera.main.transform);
            textMesh.transform.Rotate(0, 180, 0);
        }

        private void OnEnable()
        {
            //Get latest data
        }

        /// <summary>
        /// Set the data object for this SensorThing representation
        /// </summary>
        /// <param name="sensorThingsRIVM">SensorThingsRIVM API reference</param>
        /// <param name="thingData">The data object</param>
        public void SetData(SensorThingsAPI sensorThingsAPI, Things.Value thingData, string observedPropertyID, DateTime fromDateTime, DateTime toDateTime)
        {
            this.sensorThingsAPI = sensorThingsAPI;
            this.thingData = thingData;
            this.observedPropertyFilter = observedPropertyID;
            this.fromDateTime = fromDateTime;
            this.toDateTime = toDateTime;
            this.name = thingData.name;

            sensorThingsAPI.GetLocations(this.GotLocation, thingData.iotid);
            sensorThingsAPI.GetDatastreams(this.GotDatastreams, thingData.iotid);
        }

        /// <summary>
        /// Received the datastreams.
        /// Add the ObservedProperty and the Oberservations via seperate API requests
        /// </summary>
        private void GotDatastreams(bool success, Datastreams datastreams)
        {
            if (!success) return;

            this.dataStreams = datastreams;
            for (int i = 0; i < datastreams.value.Length; i++)
            {
                var datastream = datastreams.value[i];
                GatherDatastreamObservations(datastream);
            }
        }

        private void GatherDatastreamObservations(Datastreams.Value datastream)
        {
            sensorThingsAPI.GetObservedProperty((success, observedProperties) => { GotObservedProperty(success, observedProperties, datastream); }, datastream.iotid);
            sensorThingsAPI.GetObservations((success, observations) => { GotObservations(success, observations, datastream); }, datastream.iotid, fromDateTime, toDateTime);
        }

        private void GotObservedProperty(bool success, ObservedProperties.Value observedProperties, Datastreams.Value datastream)
        {
            if (!success) return;
            if (!datastreamObservations.ContainsKey(datastream))
            {
                datastreamObservations.Add(datastream, new SensorThingCombinedData());
            }
            else
            {
                datastreamObservations[datastream].observedProperty = observedProperties;
            }
        }


        /// <summary>
        /// Received the observations inside a datastream
        /// </summary>
        /// <param name="success"></param>
        /// <param name="observations"></param>
        /// <param name="datastream">Datastream object the observations are tied to in a dictionary</param>
        private void GotObservations(bool success, Observations observations, Datastreams.Value datastream)
        {
            if (!success) return;
            if (!datastreamObservations.ContainsKey(datastream))
            {
                datastreamObservations.Add(datastream, new SensorThingCombinedData());
            }
            else
            {
                datastreamObservations[datastream].observations = observations;
            }
        }

        /// <summary>
        /// Received the location of the thing.
        /// It can contain multiple locations so we loop though them, but this should usualy only be one.
        /// </summary>
        private void GotLocation(bool success, Locations locations)
        {
            if (!success) return;
            for (int i = 0; i < locations.value.Length; i++)
            {
                var location = locations.value[i].location.coordinates;
                var unityCoordinate = CoordConvert.WGS84toUnity(location[0], location[1]);
                this.transform.position = unityCoordinate;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(this.transform.position, this.transform.position + Vector3.up * 500);
        }
    }
}