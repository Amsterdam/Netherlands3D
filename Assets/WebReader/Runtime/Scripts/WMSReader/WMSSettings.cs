using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Events;

public class WMSSettings : MonoBehaviour
{
    private List<WMSLayer> activatedLayers = new();

    [SerializeField] private ObjectEvent imageEvent;

    public void ActivateLayer(object layerToActivate)
    {
        activatedLayers.Add((WMSLayer)layerToActivate);
    }

    public void DeactivateLayer(object layerToDeactivate)
    {
        if (activatedLayers.Contains((WMSLayer)layerToDeactivate))
        {
            activatedLayers.Remove((WMSLayer)layerToDeactivate);
        }
    }

    public void SendRequest()
    {
        WMSRequest.ActivatedLayers = activatedLayers;
        StartCoroutine(DownloadImage(WMSRequest.GetMapRequest(WMSFormatter.Instance.CurrentWMS, "https://service.pdok.nl/rvo/indgebfunderingsproblematiek/wms/v1_0")));
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
