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
        StartCoroutine(DownloadImage(url, legendaImage));
    }

    private void LoadPreviewImage(string url)
    {
        Debug.Log($"loading preview {url}");
        StartCoroutine(DownloadImage(url,previewImage));
    }

    private IEnumerator DownloadImage(string url, Image container)
    {

            using (var www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
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
