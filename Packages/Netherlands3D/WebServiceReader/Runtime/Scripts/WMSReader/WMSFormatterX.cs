using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class WMSFormatterX 
{
    private XmlNamespaceManager namespaceManager;
    private string namespacePrefix = "";
    private XmlDocument xml;

    public void ReadWMSFromXML(ref WMS wms, XmlDocument wmsXml)
    {
        xml = wmsXml;
        FindNamespaces();

        XmlNode capabilityNode = GetChildNode(xml.DocumentElement, "Capability");
        string version = xml.DocumentElement.Attributes.GetNamedItem("version").InnerText;

        wms.SetVersion(version);

        XmlNode topLayer = GetChildNode(capabilityNode, "Layer");
        WMSLayer topLevelLayer = ProcessLayerNode(topLayer);
        if(topLevelLayer != null)
        {
            XmlNodeList topLevelStyles = GetChildNodes(topLayer, "Style");
            foreach(XmlNode style in topLevelStyles)
            {
                WMSStyle wmsStyle = ProcessStyleNode(style);
                if (wmsStyle != null)
                {
                    topLevelLayer.AddStyleToDictionary(wmsStyle.Name, wmsStyle);
                }
            }
            wms.Layers.Add(topLevelLayer);
            if(topLevelLayer.CRS.Count > 0)
            {
                if (topLevelLayer.CoordinatesAreSRS)
                {
                    wms.SRSRequirement(true);
                    wms.SetSRS(topLevelLayer.CRS[0]);
                }
                else
                {
                    wms.SetCRS(topLevelLayer.CRS[0]);
                }
            }
        }

        XmlNodeList subLayers = GetChildNodes(topLayer, "Layer");
        foreach(XmlNode sublayer in subLayers)
        {
            WMSLayer subLevelLayer = ProcessLayerNode(sublayer);
            if(subLevelLayer != null)
            {
                XmlNodeList subLevelStyles = GetChildNodes(sublayer, "Style");
                foreach (XmlNode style in subLevelStyles)
                {
                    WMSStyle wmsStyle = ProcessStyleNode(style);
                    if (wmsStyle != null)
                    {
                        subLevelLayer.AddStyleToDictionary(wmsStyle.Name, wmsStyle);
                    }
                }
                wms.Layers.Add(subLevelLayer);
            }
            XmlNodeList bottomLayers = GetChildNodes(sublayer, "Layer");
            foreach(XmlNode bottomLayer in bottomLayers)
            {
                WMSLayer bottomLevelLayer = ProcessLayerNode(bottomLayer);
                if(bottomLevelLayer != null)
                {
                    XmlNodeList bottomLevelStyles = GetChildNodes(bottomLayer, "Style");
                    foreach (XmlNode style in bottomLevelStyles)
                    {
                        WMSStyle wmsStyle = ProcessStyleNode(style);
                        if (wmsStyle != null)
                        {
                            bottomLevelLayer.AddStyleToDictionary(wmsStyle.Name, wmsStyle);
                        }
                    }
                    wms.Layers.Add(bottomLevelLayer);
                }
            }
        }
        if (string.IsNullOrEmpty(wms.CRS))
        {
            wms.SRSRequirement(wms.Layers[1].CoordinatesAreSRS);
            wms.SetCRS(wms.Layers[1].CRS[0]);
        }
    }
    private WMSLayer ProcessLayerNode(XmlNode layerNode)
    {
        string name = GetChildNodeValue(layerNode, "Name");
        if (string.IsNullOrEmpty(name))
        {
            // The layer has no name and can't be requested;
            return null;
        }
        WMSLayer extractLayer = new WMSLayer();
        extractLayer.Name = name;
        extractLayer.Title = GetChildNodeValue(layerNode, "Title");
        extractLayer.Abstract = GetChildNodeValue(layerNode, "Abstract");
        XmlNodeList crsElements = GetChildNodes(layerNode, "CRS");
        if (crsElements.Count > 0)
        {
            //wms.SetCRS(crsElements[0].InnerText);
            foreach (XmlNode crs in crsElements)
            {
                extractLayer.CRS.Add(crs.InnerText);
            }
            return extractLayer;
        }
        else
        {
            XmlNodeList srsElements = GetChildNodes(layerNode, "SRS");
            if (srsElements.Count > 0)
            {
                //wms.SRSRequirement(true);
                //wms.SetSRS(srsElements[0].InnerText);
                extractLayer.CoordinatesAreSRS = true;
                foreach (XmlNode srs in srsElements)
                {
                    extractLayer.CRS.Add(srs.InnerText);
                }
                return extractLayer;
            }
            else
            {
                // The wms contains neither CRS', nor SRS', so it has no Reference System that can be processed.
                // Send an error message.
                Debug.LogWarning("This WMS contains no processable Reference System, cannot continue!");
                return null;
            }
        }
    }
    private WMSStyle ProcessStyleNode(XmlNode styleNode)
    {
        string styleName = GetChildNodeValue(styleNode, "Name");
        if (string.IsNullOrWhiteSpace(styleName))
        {
            Debug.Log("Found a style without a name and skipping it!");
            return null;
        }
        WMSStyle extractStyle = new WMSStyle();
        extractStyle.Name = styleName;
        extractStyle.Title = GetChildNodeValue(styleNode, "Title");
        XmlNode legendNode = GetChildNode(styleNode, "LegendURL");
        if (legendNode != null)
        {
            XmlNode onlineResourceNode = GetChildNode(legendNode, "OnlineResource");
            extractStyle.LegendURL = onlineResourceNode.Attributes.GetNamedItem("xlink:href").InnerText;
        }
        return extractStyle;
        //extractLayer.AddStyleToDictionary(extractStyle.Name, extractStyle);
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
        if (selected != null)
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
