using UnityEngine;
using Netherlands3D.Events;
using TMPro;
using System.Xml;
using UnityEngine.Networking;
using System.Collections;
using System;

public enum WebServiceType { NONE, WMS, WFS };

public class UrlReader : MonoBehaviour
{
    //public static UrlReader Instance { get; private set; }

    public WMS ActiveWMS { get; private set; } 
    public WFS ActiveWFS { get; private set; }

    [SerializeField] private TMP_InputField wfsInputField;
    [SerializeField] private TMP_InputField wmsInputField;


    [Header("Invoked Events")]
    [SerializeField] private TriggerEvent resetReaderEvent; // reset UI
    [SerializeField] private BoolEvent requestUrlButtonEvent; // editor only
    //[SerializeField] private BoolEvent isWMSEvent;

    [SerializeField] private StringEvent wmsCreationEvent;
    [SerializeField] private StringEvent wfsCreationEvent;

    //private string xmlResult;

    //private void Awake()
    //{
    //    if(Instance != null)
    //    {
    //        Debug.LogWarning("Instance has already been set, duplicate reader found!");
    //        Destroy(gameObject);
    //        return;
    //    }
    //    Instance = this;
    //}

    public void ReadAsWMS()
    {
        //var uri = new System.Uri(wmsInputField.text);
        var baseURL = GetBaseURL(wmsInputField.text);
        print(baseURL);
        wmsCreationEvent.InvokeStarted(baseURL);
    }
    public void ReadAsWFS()
    {
        var baseURL = GetBaseURL(wfsInputField.text);
        print(baseURL);
        wfsCreationEvent.InvokeStarted(baseURL);
    }

    public static string GetBaseURL(string url)
    {
        url = url.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new System.InvalidOperationException("You must input a valid URL to read");
        }

        var urlParts = url.Split('?');
        return urlParts[0];
    }
}
