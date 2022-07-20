using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.SensorThings
{
    [RequireComponent(typeof(SensorThingsAPI))]
    public class SensorThingsFilter : MonoBehaviour
    {
        [SerializeField]
        private SensorThing thingPrefab;
        private SensorThingsAPI sensorThingsAPI;

        [SerializeField] private int municipalityID = 363;

        [Header("Listen to")]
        [SerializeField] private DateTimeEvent setFromDateTime;
        [SerializeField] private DateTimeEvent setToDateTime;

        [Header("Invoke")]
        [SerializeField] private StringEvent filterOnObservedProperty;
        [SerializeField] private StringListUnityEvent foundObservableProperty;

        private string observedProperyID = "";
        private DateTime fromDateTime = DateTime.Now.AddDays(-2);
        private DateTime toDateTime = DateTime.Now;

        private List<SensorThing> sensorThings = new List<SensorThing>();

        private void OnEnable()
        {
            setFromDateTime.started.AddListener(SetFromDateTime);
            setToDateTime.started.AddListener(SetToDateTime);

            filterOnObservedProperty.started.AddListener(FilterOnObservedProperty);
            foundObservableProperty.AddListener(FoundObservedProperty);

            //Generate
            sensorThingsAPI = GetComponent<SensorThingsAPI>();
            sensorThingsAPI.StopRequests();

            //Get all then observed properties the API offers that we can filter on
            sensorThingsAPI.GetObservedProperties(GetObservedProperties);

            //Get all the SensorThings in this municipality and gather their data
            sensorThingsAPI.GetThings(GotThings, municipalityID);
        }

        private void FilterOnObservedProperty(string observedProperyID)
        {
            this.observedProperyID = observedProperyID;
        }

        private void SetToDateTime(DateTime fromDateTime)
        {
           this.fromDateTime = fromDateTime;
        }

        private void SetFromDateTime(DateTime toDateTime)
        {
            this.toDateTime = toDateTime;
        }

        private void OnDisable()
        {
            filterOnObservedProperty.started.RemoveAllListeners();
            foundObservableProperty.RemoveAllListeners();

            sensorThings.Clear();
        }

        private void FoundObservedProperty(List<string> nameDescriptionAndID)
        {
            Debug.Log($"{nameDescriptionAndID[0]}: {nameDescriptionAndID[1]}({nameDescriptionAndID[0]})");
        }

        private void GetObservedProperties(bool success, ObservedProperties observedProperties)
        {
            if (success)
            {
                Debug.Log($"ObservedProperties :{observedProperties.value.Length}");
                foreach (var observedProperty in observedProperties.value)
                {
                    var fields = new List<string>() { observedProperty.name, observedProperty.description, observedProperty.iotid.ToString() };
                    foundObservableProperty.Invoke(fields);
                }
            }
        }

        private void GotThings(bool success, Things things)
        {
            if(success)
            {
                Debug.Log($"Things:{things.value.Length}");
                foreach(var thing in things.value)
                {
                    var sensorThing3DObject = Instantiate(thingPrefab,this.transform);
                    sensorThing3DObject.SetData(sensorThingsAPI, thing, observedProperyID,fromDateTime,toDateTime);

                    sensorThings.Add(sensorThing3DObject);
                }
            }
        }
    }
}