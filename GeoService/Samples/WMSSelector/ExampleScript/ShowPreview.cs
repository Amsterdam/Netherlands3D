using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ShowPreview : MonoBehaviour
{

    public StringEvent wmsURL;
    public StringEvent legendaURL;
    public Image previewImage;
    public Image legendaImage;

    // Start is called before the first frame update
    void Start()
    {
        if (wmsURL != null)
        {
            wmsURL.started.AddListener(LoadPreviewImage);
        }
        if (legendaURL!=null)
        {
            legendaURL.started.AddListener(LoadLegendImage);
        }
    }

    private void LoadLegendImage(string url)
    {
        Debug.Log($"loading legend {url}");
        legendaImage.sprite = null;
        StartCoroutine(DownloadImage(url, legendaImage));
    }

    private void LoadPreviewImage(string url)
    {
        Debug.Log($"loading preview rawURL {url}");
        string newURL = url.Replace("{Xmin}", "120000");
        newURL = newURL.Replace("{width}","1024");
        newURL = newURL.Replace("{height}", "1024");
        newURL = newURL.Replace("{Xmax}", "121000");
        newURL = newURL.Replace("{Ymin}", "487000");
        newURL = newURL.Replace("{Ymax}", "488000");
        Debug.Log($"loading preview URL {newURL}");
        previewImage.sprite = null;
        StartCoroutine(DownloadImage(newURL,previewImage));
    }

    private IEnumerator DownloadImage(string url, Image container)
    {

            using (var www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    if (www.isDone)
                    {
                    var texture = DownloadHandlerTexture.GetContent(www);
                    var rect = new Rect(0, 0, texture.width, texture.height);
                    var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                    container.sprite = sprite;
                    container.preserveAspect = true;   
                    }
                }
            }
        }
 

    
}
