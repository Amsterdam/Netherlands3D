using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class WMSHandler : MonoBehaviour
{
    [Header("Invoked Events")]
    //[SerializeField] private TriggerEvent resetReaderEvent;
    [SerializeField] private BoolEvent isWMSEvent;
    [SerializeField] private StringEvent wmsLayerEvent;
    [SerializeField] private ObjectEvent wmsLayerBuildEvent;
    [SerializeField] private ObjectEvent legendEvent;
    [SerializeField] private ObjectEvent wmsDataEvent;
    [SerializeField] private ObjectEvent imageEvent;

    [Header("Listen Events")]
    [SerializeField] private StringEvent legendRequestEvent;
    [SerializeField] private StringEvent wmsCreationEvent;
    [SerializeField] private TriggerEvent requestWMSData;

    [SerializeField] private StringEvent boundingBoxMinXEvent;
    [SerializeField] private StringEvent boundingBoxMinYEvent;
    [SerializeField] private StringEvent boundingBoxMaxXEvent;
    [SerializeField] private StringEvent boundingBoxMaxYEvent;
    [SerializeField] private StringEvent resolutionEvent;

    private WMS wms;
    private WMSFormatter formatter;

    // Start is called before the first frame update
    void Start()
    {
        legendRequestEvent.started.AddListener((string url) => StartCoroutine(GetLegendImage(url)));
        wmsCreationEvent.started.AddListener(CreateWMS);
        requestWMSData.started.AddListener(SendWMSData);

        boundingBoxMinXEvent.started.AddListener(SetBoundingBoxMinX);
        boundingBoxMaxXEvent.started.AddListener(SetBoundingBoxMaxX);
        boundingBoxMinYEvent.started.AddListener(SetBoundingBoxMinY);
        boundingBoxMaxYEvent.started.AddListener(SetBoundingBoxMaxY);
        resolutionEvent.started.AddListener(SetResolution);
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
                StopCoroutine("DownloadImage");
            }
        }
    }
    public void SendRequest(bool preview)
    {
        wms.IsPreview(preview);
        string url = wms.GetMapRequest();
        if (preview)
        {
            StartCoroutine(DownloadImage(url, imageEvent));
            return;
        }
        if (wmsLayerEvent != null)
        {
            wmsLayerEvent.Invoke(url);
            foreach (WMSLayer l in wms.ActivatedLayers)
            {
                if (l.activeStyle != null)
                {
                    StartCoroutine(GetLegendImage(l.activeStyle.LegendURL));
                }
            }
        }
    }
    private void CreateWMS(string baseUrl)
    {
        wms = new WMS(baseUrl);
        //resetReaderEvent.Invoke();
        StartCoroutine(GetWebString(wms.GetCapabilities()));
    }
    private void SendWMSData()
    {
        wmsDataEvent.Invoke(wms);
    }
    private IEnumerator GetWebString(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            string result = request.downloadHandler.text;
            ProcessWMS(result);
            StopCoroutine("GetWebString");
        }
    }
    private IEnumerator GetLegendImage(string legendUrl)
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
                StopCoroutine("GetLegendImage");
            }
        }
    }
    private void ProcessWMS(string xmlData)
    {
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlData);

        XmlElement service = xml.DocumentElement["Service"]["Name"];
        if (service != null && service.InnerText.Contains("WMS"))
        {
            if (formatter == null)
            {
                formatter = new WMSFormatter();
            }
            wms = formatter.ReadWMSFromXML(wms, xml);
            //resetReaderEvent.Invoke();
            wmsLayerBuildEvent.Invoke(wms.Layers);
            isWMSEvent.Invoke(true);
        }
    }
    private void SetResolution(string resolution)
    {
        int res = int.Parse(resolution);
        wms.Resolution = new Vector2Int(res, res);
    }

    private void SetBoundingBoxMinX(string value)
    {
        wms.BBox.MinX = float.Parse(value);
    }
    private void SetBoundingBoxMaxX(string value)
    {
        wms.BBox.MaxX = float.Parse(value);
    }
    private void SetBoundingBoxMinY(string value)
    {
        wms.BBox.MinY = float.Parse(value);
    }
    private void SetBoundingBoxMaxY(string value)
    {
        wms.BBox.MaxY = float.Parse(value);
    }
}
