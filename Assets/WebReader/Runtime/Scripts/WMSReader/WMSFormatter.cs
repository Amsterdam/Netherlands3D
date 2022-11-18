using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using Netherlands3D.Events;

public class WMSFormatter
{
    private XmlNamespaceManager namespaceManager;
    private string namespacePrefix = "";
    private XmlDocument xml;

    public WMS ReadWMSFromXML(XmlDocument wmsXml)
    {

        xml = wmsXml;
        FindNamespaces();

        XmlNode capabilityNode = GetChildNode(xml.DocumentElement, "Capability");
        string version = xml.DocumentElement.Attributes.GetNamedItem("version").InnerText;

        WMS constructedWMS = new WMS(version);
        // We create a new WMS if one is being submitted from the Input Field, we also give it the version as a parameter in the constructor as this won't change anymore.

        XmlNode topLayer = GetChildNode(capabilityNode, "Layer");
        // We assume there is a top-level layer without styles, which contains layers that do have styles and get this layer.

        XmlNodeList subLayers = GetChildNodes(topLayer, "Layer");
        // We then get all of the sublayers within the top-level layer, so we can start evaluating them.

        foreach(XmlNode subLayer in subLayers)
        {
            string name = GetChildNodeValue(subLayer, "Name");
            if (string.IsNullOrEmpty(name))
            {
                // The layer has no name and can't be requested;
                continue;
            }
            WMSLayer extractLayer = new WMSLayer();
            extractLayer.Name = name;
            extractLayer.Title = GetChildNodeValue(subLayer, "Title");
            extractLayer.Abstract = GetChildNodeValue(subLayer, "Abstract");

            XmlNodeList crsElements = GetChildNodes(subLayer, "CRS");
            foreach(XmlNode crs in crsElements)
            {
                extractLayer.CRS.Add(crs.InnerText);
            }
            XmlNodeList styles = GetChildNodes(subLayer, "Style");
            foreach (XmlNode style in styles)
            {
                string styleName = GetChildNodeValue(style, "Name");
                if (string.IsNullOrWhiteSpace(styleName))
                {
                    Debug.Log("Found a style without a name and skipping it!");
                    continue;
                }
                WMSStyle extractStyle = new WMSStyle();
                extractStyle.Name = styleName;
                extractStyle.Title = GetChildNodeValue(style, "Title");
                XmlNode legendNode = GetChildNode(style, "LegendURL");
                if(legendNode != null)
                {
                    XmlNode onlineResourceNode = GetChildNode(legendNode, "OnlineResource");
                    extractStyle.LegendURL = onlineResourceNode.Attributes.GetNamedItem("xlink:href").InnerText;
                }
                extractLayer.AddStyleToDictionary(extractStyle.Name, extractStyle);
            }
            if(extractLayer.styles.Count > 0)
            {
                constructedWMS.layers.Add(extractLayer);
            }
            else
            {
                Debug.Log("Found a layer without applicable styles! It won't be added to the possible layers for this WMS!");
            }
        }
        return constructedWMS;
    }

    private void FindNamespaces()
    {
        namespacePrefix = "";
        namespaceManager = new XmlNamespaceManager(xml.NameTable);
        if (xml.DocumentElement.Attributes.GetNamedItem("xmlns") != null)
        {
            namespaceManager.AddNamespace("wms", xml.DocumentElement.Attributes.GetNamedItem("xmlns").InnerText);
            namespacePrefix = "wms:";
        }
    }

    private string GetChildNodeValue(XmlNode parentNode, string childNodeName)
    {
        XmlNode selected = parentNode.SelectSingleNode($"{namespacePrefix}{childNodeName}", namespaceManager);
        if(selected != null)
        {
            return selected.InnerText;
        }
        return "";
    }

    private XmlNode GetChildNode(XmlNode parentNode, string childNodeName)
    {
        return parentNode.SelectSingleNode($"{namespacePrefix}{childNodeName}", namespaceManager);
    }
    private XmlNodeList GetChildNodes(XmlNode parentNode, string childNodeName)
    {
        return parentNode.SelectNodes($"{namespacePrefix}{childNodeName}", namespaceManager);
    }

}
