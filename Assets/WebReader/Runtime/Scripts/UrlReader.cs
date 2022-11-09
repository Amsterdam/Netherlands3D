using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.Xml;
using System.Threading.Tasks;

public class UrlReader : MonoBehaviour
{
    [SerializeField] private WMSFormatter wmsFormatter;
    [SerializeField] private WFSFormatter wfsFormatter;

    private XmlReader reader;

    private static readonly HttpClient client = new();


    public void GetFromURL(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new System.InvalidOperationException("You must input a valid URL to read");
        }

        string xmlData = GetDataFromURL(url);

        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlData);

        try
        {
            XmlElement service = xml.DocumentElement["Service"]["Name"];
            if (service != null && service.InnerText.Contains("WMS"))
            {
                print(service.InnerText);
                //serviceType = ServiceType.WMS;
                wmsFormatter.ReadLayersFromWMS(xml);
                // We're going to send this over to the WMSFormatter and then return.
                return;
            }
        }
        catch (System.NullReferenceException)
        {

        }
        try
        {
            XmlElement serviceID = xml.DocumentElement["ows:ServiceIdentification"]["ows:ServiceType"];
            if (serviceID != null && serviceID.InnerText.Contains("WFS"))
            {
                print(serviceID.InnerText);
                //serviceType = ServiceType.WFS;
                wfsFormatter.ReadFromWFS(xml);
                // We're going to send this over to the WFSFormatter and then return [NOT IMPLEMENTED].
                return;
            }
        }
        catch (System.NullReferenceException)
        {

        }
    }

    private string GetDataFromURL(string url)
    {
        return client.GetStringAsync(url).Result;
    }
}
