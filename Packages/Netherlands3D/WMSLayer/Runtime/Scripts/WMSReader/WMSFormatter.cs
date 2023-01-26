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

    public WMS ReadWMSFromXML(WMS wms, XmlDocument wmsXml)
    {

        xml = wmsXml;
        FindNamespaces();

        XmlNode capabilityNode = GetChildNode(xml.DocumentElement, "Capability");
        string version = xml.DocumentElement.Attributes.GetNamedItem("version").InnerText;
        
        wms.SetVersion(version);

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
            if(crsElements.Count > 0)
            {
                wms.SetCRS(crsElements[0].InnerText);
                foreach (XmlNode crs in crsElements)
                {
                    extractLayer.CRS.Add(crs.InnerText);
                }
            }
            else
            {
                XmlNodeList srsElements = GetChildNodes(subLayer, "SRS");
                if(srsElements.Count > 0)
                {
                    wms.SRSRequirement(true);
                    wms.SetSRS(srsElements[0].InnerText);
                    foreach (XmlNode srs in srsElements)
                    {
                        extractLayer.CRS.Add(srs.InnerText);
                    }
                }
                else
                {
                    // The wms contains neither CRS', nor SRS', so it has no Reference System that can be processed.
                    // Send an error message.
                    Debug.LogWarning("This WMS contains no processable Reference System, cannot continue!");
                    return null;
                }
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
            wms.Layers.Add(extractLayer);
        }
        return wms;
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
