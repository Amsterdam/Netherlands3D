using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Events;

public class WebServiceNetworker : MonoBehaviour
{
    public static WebServiceNetworker Instance { get; private set; }

    [SerializeField] private GameObject messagePanel;

    [field:SerializeField] public StringEvent WebStringEvent { get; private set; }
    [Header("Invoked Events")]
    
    [SerializeField] private ObjectEvent imageEvent;
    [SerializeField] private ObjectEvent legendEvent;
    [SerializeField] private StringEvent wmsLayerEvent;
    [SerializeField] private StringEvent messageTitleEvent;
    [SerializeField] private StringEvent urlDisplayEvent;

    [Header("Listen Events")]
    [SerializeField] private TriggerEvent logEvent;


    private void Start()
    {
        if(logEvent == null || Instance != null)
            return;

        Instance = this;
        logEvent.started.AddListener(() => 
            { 
                messagePanel.SetActive(true);
                messageTitleEvent.Invoke("Url Logged");
                WMS.ActiveInstance.IsPreview(false);
                urlDisplayEvent.Invoke(WMS.ActiveInstance.GetMapRequest());
            }
        );
    }

    public void SendRequest(bool preview)
    {
        //WMSRequest.ActivatedLayers = WMSInterface.ActivatedLayers;

        WMS.ActiveInstance.IsPreview(preview);
        string url = WMS.ActiveInstance.GetMapRequest();
        if (preview)
        {
            StartCoroutine(DownloadImage(url, imageEvent));
            return;
        }
        if(wmsLayerEvent != null)
        {
            wmsLayerEvent.Invoke(url);
            foreach(WMSLayer l in WMS.ActiveInstance.ActivatedLayers)
            {
                if(l.activeStyle != null)
                {
                    StartCoroutine(GetLegendImage(l.activeStyle.LegendURL));
                }
            }
            //StartCoroutine(GetLegendImage(WMSRequest.ActivatedLayers[0].activeStyle.LegendURL));
        }
    }

    //private string GetMapRequestUrl(bool useCustomBBox)
    //{
    //    return WMS.ActiveInstance.GetMapRequest();
    //}
    public IEnumerator GetWebString(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            if (WebStringEvent != null)
            {
                string result = request.downloadHandler.text;
                WebStringEvent.Invoke(result);
                WebStringEvent.started.RemoveAllListeners();
            }
        }
    }
    public IEnumerator DownloadImage(string mediaURL, ObjectEvent imageEvent)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaURL);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            if (imageEvent != null)
            {
                imageEvent.Invoke(((DownloadHandlerTexture)request.downloadHandler).texture);
            }
        }
    }

    IEnumerator GetLegendImage(string legendUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(legendUrl);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            if (legendEvent != null)
            {
                legendEvent.Invoke(((DownloadHandlerTexture)request.downloadHandler).texture);
            }
        }
    }

}
