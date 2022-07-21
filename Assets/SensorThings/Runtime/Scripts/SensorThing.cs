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
        private Dictionary<int, SensorThingCombinedData> datastreamObservations = new Dictionary<int, SensorThingCombinedData>();

        [SerializeField]
        private TextMesh textMesh;

        [SerializeField]
        private MeshRenderer meshRenderer;

        private string observedPropertyFilter;

        private DateTime fromDateTime;
        private DateTime toDateTime;

        private void Start()
        {
            textMesh.text = "";
            meshRenderer.enabled = false;
        }

        private void Update()
        {
            textMesh.transform.rotation = Camera.main.transform.rotation;
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
        public void SetData(SensorThingsAPI sensorThingsAPI, Things.Value thingData, string observedPropertyFilter, DateTime fromDateTime, DateTime toDateTime)
        {
            this.sensorThingsAPI = sensorThingsAPI;
            this.thingData = thingData;
            this.observedPropertyFilter = observedPropertyFilter;
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
            var newCombinedData = new SensorThingCombinedData() { datastream = datastream };

            datastreamObservations.Add(datastream.iotid, newCombinedData);
            sensorThingsAPI.GetObservedProperty((success, observedProperties) => { GotObservedProperty(success, observedProperties, datastream, newCombinedData); }, datastream.iotid);
        }

        private void GotObservedProperty(bool success, ObservedProperties.Value observedProperties, Datastreams.Value datastream, SensorThingCombinedData combinedData)
        {
            if (!success) return;
            combinedData.observedProperty = observedProperties;
            sensorThingsAPI.GetObservations((success, observations) => { GotObservations(success, observations, datastream, combinedData); }, datastream.iotid);
        }

        /// <summary>
        /// Received the observations inside a datastream
        /// </summary>
        /// <param name="success"></param>
        /// <param name="observations"></param>
        /// <param name="datastream">Datastream object the observations are tied to in a dictionary</param>
        private void GotObservations(bool success, Observations observations, Datastreams.Value datastream, SensorThingCombinedData combinedData)
        {
            if (!success) return;

            combinedData.observations = observations;
            
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            this.name = $"{datastreamObservations.Count}";

            if (datastreamObservations.Count > 0 )
            {
                foreach (KeyValuePair<int, SensorThingCombinedData> dataCombination in datastreamObservations)
                {
                        var dataStream = dataCombination.Value.datastream;
                        var observedProperty = dataCombination.Value.observedProperty;
                        var observations = dataCombination.Value.observations;


                        if (observedProperty != null)
                        {
                            if (observedProperty.iotid.ToString() == observedPropertyFilter && observations != null && observations.value.Length > 0) {
                                meshRenderer.enabled = true;
                                

                                var observation = ObservationClosestToTimeRange(observations.value);
                                textMesh.text = $"{observation.result} {dataStream.unitOfMeasurement.symbol}";
                                meshRenderer.material.color = Color.Lerp(Color.green, Color.red, observation.result);
                            }
                        }

                }
            }
        }

        private Observations.Value ObservationClosestToTimeRange(Observations.Value[] observations)
        {
            if (observations.Length == 0) return null;
            return observations[0];
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