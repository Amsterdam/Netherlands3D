using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.Xml;
using System.Threading.Tasks;

public class UrlReader : MonoBehaviour
{
    public enum ServiceType { UNDEFINED, WMS, WFS };

    private string namespacePrefix = "";
    private ServiceType serviceType;

    private XmlReader reader;
    private static readonly HttpClient client = new();



    public void GetFromURL(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new System.InvalidOperationException("You must input a valid URL to read");
        }
        if (url.Contains("service=wms") || url.Contains("service=WMS"))
        {
            print("This link is identified as a WMS!");
            serviceType = ServiceType.WMS;
        }
        if (url.Contains("service=wfs") || url.Contains("service=WFS"))
        {
            print("This link is identified as a WFS!");
            serviceType = ServiceType.WFS;
        }

        // These if-Contains need to be refactored as they're too inaccurate and not reliable enough.
        // Will evaluate the Service/ServiceType within the XML at a later time.

        //reader = XmlReader.Create(url);

        string xmlData = GetDataFromURL(url);

        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlData);


        //XmlNode serviceNode = GetChildNode(xml.DocumentElement, "Service");
        //XmlNode sNode = xml.GetElementById("Service");

        //print(serviceNode.Name);
        print(xml);
        print(xmlData);

        //switch (serviceType)
        //{
        //    case ServiceType.WMS:
        //        ShowWMSXml();
        //        WMSFormatter.DeserializeToWMS(xml);
        //        break;
        //    case ServiceType.WFS:
        //        ShowWFSXml();
        //        break;
        //}
        //reader.Close();
        //reader.Dispose();
    }

    private void ShowWMSXml()
    {
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if(reader.Name is "Layer")
                    {
                        print("This is a Layer!");
                        //Create a button in the layer interface for this layer, with it's name
                    }
                    print($"<{reader.Name}>");
                    break;
                case XmlNodeType.Text:
                    print(reader.Value);
                    break;
                case XmlNodeType.EndElement:
                    print($"</{reader.Name}>");
                    break;
            }
        }
        reader.Close();
        reader.Dispose();
    }

    private void ShowWFSXml()
    {
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    print($"<{reader.Name}>");
                    break;
                case XmlNodeType.Text:
                    print(reader.Value);
                    break;
                case XmlNodeType.EndElement:
                    print($"</{reader.Name}>");
                    break;
            }
        }
        reader.Close();
        reader.Dispose();
    }

    private string GetDataFromURL(string url)
    {
        return client.GetStringAsync(url).Result;
    }

    private XmlNode GetChildNode(XmlNode parentNode, string childName)
    {
        return parentNode.SelectSingleNode($"{namespacePrefix}{childName}");
    }
}
