using Netherlands3D.Core;
using Netherlands3D.SensorThings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorThingVisual : MonoBehaviour
{
    private SensorThingsRIVM sensorThingsRIVM;

    private Things.Value thing;
    private Datastreams datatStreams;

    private Dictionary<Datastreams.Value, Observations> datastreamObservations = new Dictionary<Datastreams.Value, Observations>();

    private void OnEnable()
    {
        //Get latest data
    }

    /// <summary>
    /// Set the data object for this SensorThing representation
    /// </summary>
    /// <param name="sensorThingsRIVM">SensorThingsRIVM API reference</param>
    /// <param name="thingData">The data object</param>
    public void SetData(SensorThingsRIVM sensorThingsRIVM, Things.Value thingData)
    {
        this.sensorThingsRIVM = sensorThingsRIVM;
        this.thing = thingData;

        this.name = thingData.name;

        sensorThingsRIVM.GetLocations(GotLocation, thingData.iotid);
        sensorThingsRIVM.GetDatastreams(GotDatastreams, thingData.iotid);
    }

    /// <summary>
    /// Received the datastreams.
    /// For now we simple create gameobjects for the streams to inspect them in the editor
    /// </summary>
    private void GotDatastreams(bool success, Datastreams datastreams)
    {
        if (!success) return;

        this.datatStreams = datastreams;
        for (int i = 0; i < datastreams.value.Length; i++)
        {
            var datastream  = datastreams.value[i];

            var datastreamVisual = new GameObject();
            datastreamVisual.transform.SetParent(this.transform);
            datastreamVisual.name = datastream.name + " - " + datastream.unitOfMeasurement.symbol;

            sensorThingsRIVM.GetObservations((success,observations) => { GotObservations(success, observations, datastream, datastreamVisual); });
        }
    }

    /// <summary>
    /// Received the observations inside a datastream
    /// </summary>
    /// <param name="success"></param>
    /// <param name="observations"></param>
    /// <param name="datastream">Datastream object the observations are tied to in a dictionary</param>
    private void GotObservations(bool success, Observations observations, Datastreams.Value datastream, GameObject datastreamVisual)
    {
        if (!success) return;
        if (!datastreamObservations.ContainsKey(datastream))
        {
            datastreamObservations.Add(datastream, observations);
        }
        else
        {
            datastreamObservations[datastream] = observations;
        }

        foreach (var observation in observations.value)
        {
            var observationVisual = new GameObject();
            observationVisual.transform.SetParent(datastreamVisual.transform);
            observationVisual.name = observation.phenomenonTime + ": " + observation.result + datastream.unitOfMeasurement.symbol;
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
