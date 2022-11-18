using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Events;

public class WMSSettings : MonoBehaviour
{
    [SerializeField] private ObjectEvent imageEvent;
    public void SendRequest()
    {
        WMSRequest.ActivatedLayers = WMSInterface.ActivatedLayers;
        StartCoroutine(DownloadImage(WMSRequest.GetMapRequest(UrlReader.Instance.ActiveWMS)));
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
