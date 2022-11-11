using System.Collections;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DebugReader : MonoBehaviour
{
    public string Url;
    public UrlReader urlReader;

    [SerializeField] private string requestURL;
    [SerializeField] private RawImage rawImage;

    public void ReadURLInEditor()
    {
        urlReader.GetFromURL(Url);
    }

    public void GetPreview()
    {
        StartCoroutine(DownloadImage(requestURL));
    }

    IEnumerator DownloadImage(string mediaURL)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaURL);
        yield return request.SendWebRequest();
        if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            rawImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }
}
