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


        private void OnEnable()
        {
            //Generate
            sensorThingsRIVM.GetComponent<SensorThingsRIVM>();

            //Get all things
            //Append properties

        }
        private void OnDisable()
        {
            //Clean up
        }
    }
}