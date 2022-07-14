using Netherlands3D.SensorThings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorThingVisual : MonoBehaviour
{
    private SensorThingsRIVM sensorThingsRIVM;
    private Things.Value thingData;

    private void OnEnable()
    {
        //Get latest data
    }

    public void SetData(SensorThingsRIVM sensorThingsRIVM, Things.Value thingData)
    {
        this.sensorThingsRIVM = sensorThingsRIVM;
        this.thingData = thingData;

        this.name = thingData.name;

        //Get location from API

    }


}
