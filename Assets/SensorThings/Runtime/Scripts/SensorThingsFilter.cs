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

        [SerializeField] private StringEvent filterOnObservedProperty;
        [SerializeField] private StringListUnityEvent foundObservableProperty;

        private string observedProperyID = ""; 

        private void OnEnable()
        {
            filterOnObservedProperty.started.AddListener(FilterOnObservedProperty);
            foundObservableProperty.AddListener(FoundObservedProperty);

            //Generate
            sensorThingsAPI = GetComponent<SensorThingsAPI>();
            sensorThingsAPI.StopRequests();

            //sensorThingsRIVM.GetThings(GotThings, municipalityID);
            sensorThingsAPI.GetObservedProperties(GetObservedProperties);
        }

        private void OnDisable()
        {
            filterOnObservedProperty.started.RemoveAllListeners();
            foundObservableProperty.RemoveAllListeners();
        }

        private void FilterOnObservedProperty(string observedProperyID)
        {
            this.observedProperyID = observedProperyID;
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
                    sensorThing3DObject.SetData(sensorThingsAPI, thing, observedProperyID);
                }
            }
        }
    }
}