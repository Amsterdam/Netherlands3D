using UnityEngine;
using System.Xml;
using Netherlands3D.Events;
using UnityEngine.UI;

public class UrlReader : MonoBehaviour
{
    public static UrlReader Instance { get; private set; }

    public WMS ActiveWMS { get; private set; } 
    public WFS ActiveWFS { get; private set; }

    [SerializeField] private InputField urlField;

    [Header("Invoked Events")]
    [SerializeField] private TriggerEvent resetReaderEvent;
    [SerializeField] private ObjectEvent wmsLayerEvent;
    [SerializeField] private BoolEvent requestUrlButtonEvent;
    [SerializeField] private BoolEvent isWMSEVent;

    [Header("Listen Events")]
    [SerializeField] private StringEvent boundingBoxMinXEvent;
    [SerializeField] private StringEvent boundingBoxMinYEvent;
    [SerializeField] private StringEvent boundingBoxMaxXEvent;
    [SerializeField] private StringEvent boundingBoxMaxYEvent;
    [SerializeField] private StringEvent resolutionEvent;

    //private IWebService activeService;
    private WMSFormatter wmsFormatter;
    private WFSFormatter wfsFormatter;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("Instance has already been set, duplicate reader found!");
            return;
        }
        Instance = this;

        boundingBoxMinXEvent.started.AddListener(SetBoundingBoxMinX);
        boundingBoxMaxXEvent.started.AddListener(SetBoundingBoxMaxX);
        boundingBoxMinYEvent.started.AddListener(SetBoundingBoxMinY);
        boundingBoxMaxYEvent.started.AddListener(SetBoundingBoxMaxY);
        resolutionEvent.started.AddListener(SetResolution);
    }

    private void Start()
    {
        if(requestUrlButtonEvent != null)
        {
            requestUrlButtonEvent.Invoke(Application.isEditor);
        }
    }
    public void GetFromURL()
    {
        if (resetReaderEvent == null || wmsLayerEvent == null)
        {
            Debug.LogError("Events aren't properly set up! Please resolve this!");
        }
        
        string url = urlField.text.ToLower();
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new System.InvalidOperationException("You must input a valid URL to read");

        }

        string validatedURL = string.Empty;
        foreach(char c in url)
        {
            if(c == char.Parse("?"))
            {
                break;
            }
            validatedURL += c;
        }

        XmlDocument xml = new XmlDocument();
        if (url.Contains("wms"))
        {
            ActiveWMS = new WMS(validatedURL);
            WebServiceNetworker.Instance.WebStringEvent.started.AddListener(ProcessWMS);
            ActiveWMS.GetCapabilities();
            //Debug.Log(xmlData);
            //xml.LoadXml(xmlData);

            //XmlElement service = xml.DocumentElement["Service"]["Name"];
            //if (service != null && service.InnerText.Contains("WMS"))
            //{
            //    if (wmsFormatter is null)
            //    {
            //        wmsFormatter = new WMSFormatter();
            //    }
            //    ActiveWMS = wmsFormatter.ReadWMSFromXML(ActiveWMS, xml);
            //    resetReaderEvent.Invoke();
            //    wmsLayerEvent.Invoke(ActiveWMS.Layers);
            //    isWMSEVent.Invoke(true);
            //    return;
            //}

        }
        else if (url.Contains("wfs"))
        {
            ActiveWFS = new WFS(validatedURL);
            WebServiceNetworker.Instance.WebStringEvent.started.AddListener(ProcessWFS);
            ActiveWFS.GetCapabilities();
            //xml.LoadXml(xmlData);

            //XmlElement serviceID = xml.DocumentElement["ows:ServiceIdentification"]["ows:ServiceType"];
            //if (serviceID != null && serviceID.InnerText.Contains("WFS"))
            //{
            //    print(serviceID.InnerText);
            //    if (wfsFormatter is null)
            //    {
            //        wfsFormatter = new WFSFormatter();
            //    }
            //    ActiveWFS = wfsFormatter.ReadFromWFS(xml);
            //    isWMSEVent.Invoke(false);
            //    return;
            //}

        }
    }
    private void ProcessWMS(string xmlData)
    {
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlData);

        XmlElement service = xml.DocumentElement["Service"]["Name"];
        if (service != null && service.InnerText.Contains("WMS"))
        {
            if (wmsFormatter is null)
            {
                wmsFormatter = new WMSFormatter();
            }
            ActiveWMS = wmsFormatter.ReadWMSFromXML(ActiveWMS, xml);
            resetReaderEvent.Invoke();
            wmsLayerEvent.Invoke(ActiveWMS.Layers);
            isWMSEVent.Invoke(true);
            return;
        }
    }
    private void ProcessWFS(string xmlData)
    {
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlData);

        XmlElement serviceID = xml.DocumentElement["ows:ServiceIdentification"]["ows:ServiceType"];
        if (serviceID != null && serviceID.InnerText.Contains("WFS"))
        {
            print(serviceID.InnerText);
            if (wfsFormatter is null)
            {
                wfsFormatter = new WFSFormatter();
            }
            ActiveWFS = wfsFormatter.ReadFromWFS(xml);
            isWMSEVent.Invoke(false);
            return;
        }
    }
    private void SetResolution(string resolution)
    {
        int res = int.Parse(resolution);
        ActiveWMS.Resolution = new Vector2Int(res, res);
    }

    private void SetBoundingBoxMinX(string value)
    {
        ActiveWMS.BBox.MinX = float.Parse(value);
    }
    private void SetBoundingBoxMaxX(string value)
    {
        ActiveWMS.BBox.MaxX = float.Parse(value);
    }
    private void SetBoundingBoxMinY(string value)
    {
        ActiveWMS.BBox.MinY = float.Parse(value);
    }
    private void SetBoundingBoxMaxY(string value)
    {
        ActiveWMS.BBox.MaxY = float.Parse(value);
    }
}
