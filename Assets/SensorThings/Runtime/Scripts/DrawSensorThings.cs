using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.SensorThings
{
    [RequireComponent(typeof(SensorThingsRIVM))]
    public class DrawSensorThings : MonoBehaviour
    {
        [SerializeField]
        private SensorThingVisual thingPrefab;
        private SensorThingsRIVM sensorThingsRIVM;

        [SerializeField] private int municipalityID = 363;
        private void OnEnable()
        {
            //Generate
            sensorThingsRIVM = GetComponent<SensorThingsRIVM>();
            sensorThingsRIVM.GetThings(GotThings, municipalityID);
        }

        private void GotThings(bool success, Things things)
        {
            if(success)
            {
                Debug.Log($"Things:{things.value.Length}");
                foreach(var thing in things.value)
                {
                    var thingVisual = Instantiate(thingPrefab,this.transform);
                    thingVisual.SetData(sensorThingsRIVM, thing);
                }
            }
        }

        private void OnDisable()
        {
            //Clean up
        }
    }
}