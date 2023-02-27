using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Events;
using Netherlands3D.T3DPipeline;
using UnityEngine;

[RequireComponent(typeof(CityJSON))]
public class TestCityJSONInput : MonoBehaviour
{
    [SerializeField]
    private TextAsset testJson;
    [SerializeField]
    private StringEvent cityJSONReceived;

    protected void Start()
    {
        print(testJson.text);
        cityJSONReceived.InvokeStarted(testJson.text);
    }
}
