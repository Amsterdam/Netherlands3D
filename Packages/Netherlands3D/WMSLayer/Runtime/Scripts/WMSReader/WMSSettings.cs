using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Events;

public class WMSSettings : MonoBehaviour
{
    [SerializeField] private GameObject messagePanel;

    [Header("Invoked Events")]
    [SerializeField] private ObjectEvent imageEvent;
    [SerializeField] private StringEvent wmsLayerEvent;
    [SerializeField] private StringEvent messageTitleEvent;
    [SerializeField] private StringEvent urlDisplayEvent;

    [Header("Listen Events")]
    [SerializeField] private TriggerEvent logEvent;


    private void Start()
    {
        if(logEvent == null)
            return;
       
        logEvent.started.AddListener(() => 
            { 
                messagePanel.SetActive(true);
                messageTitleEvent.Invoke("Url Logged");
                WMSRequest.ActivatedLayers = WMSInterface.ActivatedLayers;
                urlDisplayEvent.Invoke(GetMapRequestUrl(false));
            }
        );
    }

    public void SendRequest(bool preview)
    {
        WMSRequest.ActivatedLayers = WMSInterface.ActivatedLayers;
        string url = GetMapRequestUrl(preview);
        if (preview)
        {
            StartCoroutine(DownloadImage(url));
            return;
        }
        if(wmsLayerEvent != null)
        {
            wmsLayerEvent.Invoke(url);
        }
    }

    private string GetMapRequestUrl(bool useCustomBBox)
    {
        return WMSRequest.GetMapRequest(UrlReader.Instance.ActiveWMS, useCustomBBox);
    }

    IEnumerator DownloadImage(string mediaURL)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaURL);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (imageEvent != null)
            {
                imageEvent.Invoke(((DownloadHandlerTexture)request.downloadHandler).texture);
            }
        }
    }
}
