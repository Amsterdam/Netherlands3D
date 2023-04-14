using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class WMSHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField minXField;
    [SerializeField] private TMP_InputField maxXField;
    [SerializeField] private TMP_InputField minYField;
    [SerializeField] private TMP_InputField maxYField;

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
    [SerializeField] private DoubleArrayEvent boundingBoxDoubleArrayEvent;
    [SerializeField] private StringEvent resolutionEvent;

    private WMS wms;

    // Start is called before the first frame update
    void Start()
    {
        legendRequestEvent.AddListenerStarted((string url) => StartCoroutine(GetLegendImage(url)));
        wmsCreationEvent.AddListenerStarted(CreateWMS);
        requestWMSData.AddListenerStarted(SendWMSData);

        boundingBoxMinXEvent.AddListenerStarted(SetBoundingBoxMinX);
        boundingBoxMaxXEvent.AddListenerStarted(SetBoundingBoxMaxX);
        boundingBoxMinYEvent.AddListenerStarted(SetBoundingBoxMinY);
        boundingBoxMaxYEvent.AddListenerStarted(SetBoundingBoxMaxY);

        if (boundingBoxDoubleArrayEvent) boundingBoxDoubleArrayEvent.AddListenerStarted(SetBoundsFromDoubleArray);

        resolutionEvent.AddListenerStarted(SetResolution);
    }

    public IEnumerator DownloadImage(string mediaURL, ObjectEvent imageEvent)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaURL, true);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            if (imageEvent != null)
            {
                imageEvent.InvokeStarted(((DownloadHandlerTexture)request.downloadHandler).texture);
            }
        }
    }
    public void SendRequest(bool preview)
    {


        wms.IsPreview(preview);
        string url = wms.GetMapRequest();
        if (preview)
        {
            if (!ParseFields())
                return;
            StartCoroutine(DownloadImage(url, imageEvent));
            return;
        }
        if (wmsLayerEvent != null)
        {
            wmsLayerEvent.InvokeStarted(url);
            foreach (WMSLayer l in wms.ActivatedLayers)
            {
                if (l.activeStyle != null)
                {
                    StartCoroutine(GetLegendImage(l.activeStyle.LegendURL));
                }
            }
        }
    }
    private bool ParseFields()
    {
        if (!double.TryParse(minXField.text, out wms.BBox.MinX))
            return false;

        if (!double.TryParse(maxXField.text, out wms.BBox.MaxX))
            return false;

        if (!double.TryParse(minYField.text, out wms.BBox.MinY))
            return false;

        if (!double.TryParse(maxYField.text, out wms.BBox.MaxY))
            return false;

        return true;
    }

    private void SetBoundsFromDoubleArray(double[] boundsArray)
    {
        minXField.text = boundsArray[0].ToString();
        minYField.text = boundsArray[1].ToString();
        maxXField.text = boundsArray[2].ToString();
        maxYField.text = boundsArray[3].ToString();
    }

    private void CreateWMS(string baseUrl)
    {
        wms = new WMS(baseUrl);
        //resetReaderEvent.Invoke();
        StartCoroutine(GetWebString(wms.GetCapabilities()));
    }
    private void SendWMSData()
    {
        wmsDataEvent.InvokeStarted(wms);
    }
    private IEnumerator GetWebString(string url)
    {
        Debug.Log(url);
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
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(legendUrl, true);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            if (legendEvent != null)
            {
                legendEvent.InvokeStarted(((DownloadHandlerTexture)request.downloadHandler).texture);
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
            WMSFormatterX formatter = new WMSFormatterX();
            formatter.ReadWMSFromXML(ref wms, xml);
            //resetReaderEvent.Invoke();
            wmsLayerBuildEvent.InvokeStarted(wms.Layers);
            isWMSEvent.InvokeStarted(true);
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
