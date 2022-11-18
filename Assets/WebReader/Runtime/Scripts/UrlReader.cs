using UnityEngine;
using System.Net.Http;
using System.Xml;
using Netherlands3D.Events;

public class UrlReader : MonoBehaviour
{
    public static UrlReader Instance { get; private set; }

    public WMS ActiveWMS { get; private set; }
    //public WFS ActiveWFS { get; private set; }

    private WMSFormatter wmsFormatter;
    private WFSFormatter wfsFormatter;

    [Header("Events")]
    [SerializeField] private TriggerEvent resetReaderEvent;
    [SerializeField] private ObjectEvent wmsLayerEvent;


    private static readonly HttpClient client = new();

    private void Awake()
    {
        if(Instance is not null)
        {
            Debug.LogWarning("Instance has already been set, duplicate reader found!");
            return;
        }
        Instance = this;
    }
    public void GetFromURL(string url)
    {

        if (resetReaderEvent == null || wmsLayerEvent == null)
        {
            Debug.LogError("Events aren't properly set up! Please resolve this!");
        }
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new System.InvalidOperationException("You must input a valid URL to read");
        }

        string validatedURL = "";
        foreach(char c in url)
        {
            if(c == char.Parse("?"))
            {
                break;
            }
            validatedURL += c;
        }
        WMSRequest.BaseURL = validatedURL;
        string xmlData = GetDataFromURL(WMSRequest.GetCapabilitiesRequest());

        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlData);

        try
        {
            XmlElement service = xml.DocumentElement["Service"]["Name"];
            if (service != null && service.InnerText.Contains("WMS"))
            {

                if(wmsFormatter is null)
                {
                    wmsFormatter = new WMSFormatter();
                }
                ActiveWMS = wmsFormatter.ReadWMSFromXML(xml);
                resetReaderEvent.Invoke();
                wmsLayerEvent.Invoke(ActiveWMS.layers);
                // We're going to send this over to the WMSFormatter and then return.
                return;
            }

            // First, we attempt to read the obtained xml, if it exists and we try to read it as a WMS.


            XmlElement serviceID = xml.DocumentElement["ows:ServiceIdentification"]["ows:ServiceType"];
            if (serviceID != null && serviceID.InnerText.Contains("WFS"))
            {
                print(serviceID.InnerText);
                if (wfsFormatter is null)
                {
                    wfsFormatter = new WFSFormatter();
                }
                wfsFormatter.ReadFromWFS(xml);
                // We're going to send this over to the WFSFormatter and then return [NOT IMPLEMENTED].
                return;
            }

            // If the xml is not a WMS, we attempt to read the obtained xml as a WFS.

        }
        catch (System.NullReferenceException)
        {

        }
    }

    public void SetResolution(string resolution)
    {
        int res = int.Parse(resolution);
        ActiveWMS.Dimensions = new Vector2Int(res, res);
    }

    public void SetBoundingBoxMinX(string value)
    {
        ActiveWMS.BBox.MinX = float.Parse(value);
    }
    public void SetBoundingBoxMaxX(string value)
    {
        ActiveWMS.BBox.MaxX = float.Parse(value);
    }
    public void SetBoundingBoxMinY(string value)
    {
        ActiveWMS.BBox.MinY = float.Parse(value);
    }
    public void SetBoundingBoxMaxY(string value)
    {
        ActiveWMS.BBox.MaxY = float.Parse(value);
    }

    private string GetDataFromURL(string url)
    {
        return client.GetStringAsync(url).Result;
    }
}
