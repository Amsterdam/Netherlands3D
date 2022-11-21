using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class WFSFormatter
{
    private XmlNamespaceManager namespaceManager;
    private string namespacePrefix = "";
    private XmlDocument xml;

    private Dictionary<string, string> wfsXmlNamespaces;
    public WFS ReadFromWFS(XmlDocument wmsXml)
    {

        xml = wmsXml;
        FindNamespaces();

        string fes = "http://www.opengis.net/fes/2.0";

        //XmlNode filterCapabilities = GetChildNode(xml.DocumentElement, $"{fes}:Filter_Capabilities");
        //Debug.Log(filterCapabilities.InnerText);

        //XmlNode featureList = GetChildNode(xml.DocumentElement, "FeatureTypeList");
        //Debug.Log(featureList.InnerText);

        string version = xml.DocumentElement.Attributes.GetNamedItem("version").InnerText;

        WFS wfs = new WFS(version);

        //XmlNode topLayer = GetChildNode(filterCapabilities, "Layer");
        //XmlNodeList subLayers = GetChildNodes(topLayer, "Layer");


        //XmlNodeList layersList = wmsXml.SelectNodes("/WMS_Capabilities/Layer/Layer");
        //List<WFSLayer> wfsLayers = new();

        //foreach (XmlNode subLayer in subLayers)
        //{
        //    string name = GetChildNodeValue(subLayer, "Name");
        //    Debug.Log(name);
        //    if (string.IsNullOrEmpty(name))
        //    {
        //        // The layer has no name and can't be requested;
        //        continue;
        //    }
        //    WMSLayer extractLayer = new WMSLayer();
        //    extractLayer.Name = name;
        //    extractLayer.Title = GetChildNodeValue(subLayer, "Title");
        //    extractLayer.Abstract = GetChildNodeValue(subLayer, "Abstract");

        //    XmlNodeList crsElements = GetChildNodes(subLayer, "CRS");
        //    foreach (XmlNode crs in crsElements)
        //    {
        //        Debug.Log("Found a CRS element!");
        //        extractLayer.CRS.Add(crs.InnerText);
        //    }
        //    XmlNodeList styles = GetChildNodes(subLayer, "Style");
        //    foreach (XmlNode style in styles)
        //    {
        //        WMSStyle extractStyle = new WMSStyle();
        //        extractStyle.Name = GetChildNodeValue(style, "Name");
        //        extractStyle.Title = GetChildNodeValue(style, "Title");
        //        XmlNode legendNode = GetChildNode(style, "LegendURL");
        //        if (legendNode != null)
        //        {
        //            XmlNode onlineResourceNode = GetChildNode(legendNode, "OnlineResource");
        //            extractStyle.LegendURL = onlineResourceNode.Attributes.GetNamedItem("xlink:href").InnerText;
        //        }
        //        extractLayer.AddStyleToDictionary(extractStyle.Name, extractStyle);
        //    }
        //    wmsLayers.Add(extractLayer);
        //    Debug.Log(extractLayer);
        //}

        //resetReaderEvent.Invoke();
        //wfsEvent.Invoke(wfsLayers);

        return wfs;

    }

    private void FindNamespaces()
    {
        wfsXmlNamespaces = new();
        namespaceManager = new XmlNamespaceManager(xml.NameTable);

        if (xml.DocumentElement.Attributes.GetNamedItem("xmlns:fes") != null)
        {
            string ns = xml.DocumentElement.Attributes.GetNamedItem("xmlns:fes").InnerText;
            namespaceManager.AddNamespace("fes", ns);
            wfsXmlNamespaces.Add("fes", ns);
        }
        if (xml.DocumentElement.Attributes.GetNamedItem("xmlns:ows") != null)
        {
            string ns = xml.DocumentElement.Attributes.GetNamedItem("xmlns:ows").InnerText;
            namespaceManager.AddNamespace("wfs", ns);
            wfsXmlNamespaces.Add("ows", ns);
        }
        if (xml.DocumentElement.Attributes.GetNamedItem("xmlns") != null)
        {
            string ns = xml.DocumentElement.Attributes.GetNamedItem("xmlns").InnerText;
            namespaceManager.AddNamespace("wfs", xml.DocumentElement.Attributes.GetNamedItem("xmlns").InnerText);
            wfsXmlNamespaces.Add("wfs", ns);
        }
    }

    private string GetChildNodeValue(XmlNode parentNode, string childNodeName, string nameSpace)
    {
        XmlNode selected = parentNode.SelectSingleNode($"{wfsXmlNamespaces[nameSpace]}{childNodeName}", namespaceManager);
        if (selected != null)
        {
            return selected.InnerText;
        }
        return "";
    }

    private XmlNode GetChildNode(XmlNode parentNode, string childNodeName, string nameSpace)
    {
        return parentNode.SelectSingleNode($"{wfsXmlNamespaces[nameSpace]}{childNodeName}", namespaceManager);
    }
    private XmlNodeList GetChildNodes(XmlNode parentNode, string childNodeName, string nameSpace)
    {
        return parentNode.SelectNodes($"{wfsXmlNamespaces[nameSpace]}{childNodeName}", namespaceManager);
    }

}
