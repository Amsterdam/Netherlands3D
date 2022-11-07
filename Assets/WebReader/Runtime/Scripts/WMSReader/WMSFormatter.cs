using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using Netherlands3D.Events;

public class WMSFormatter : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private TriggerEvent resetReaderEvent;
    [SerializeField] private ObjectEvent wmsLayerEvent;

    private XmlNamespaceManager namespaceManager;
    private string namespacePrefix = "";
    private XmlDocument xml;


    public void ReadLayersFromWMS(XmlDocument wmsXml)
    {
        if (resetReaderEvent == null || wmsLayerEvent == null)
        {
            Debug.LogError("Events aren't properly set up! Please resolve this!");
        }
        xml = wmsXml;
        FindNamespaces();
        //XmlNodeList layersList = wmsXml.SelectNodes("/WMS_Capabilities/Layer/Layer");

        XmlNode capabilityNode = GetChildNode(xml.DocumentElement, "Capability");
        XmlNode topLayer = GetChildNode(capabilityNode, "Layer");
        XmlNodeList subLayers = GetChildNodes(topLayer, "Layer");

        List<WMSLayer> wmsLayers = new();

        foreach(XmlNode subLayer in subLayers)
        {
            string name = GetChildNodeValue(subLayer, "Name");
            Debug.Log(name);
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
                Debug.Log("Found a CRS element!");
                extractLayer.CRS.Add(crs.InnerText);
            }
            XmlNodeList styles = GetChildNodes(subLayer, "Style");
            foreach (XmlNode style in styles)
            {
                WMSStyle extractStyle = new WMSStyle();
                extractStyle.Name = GetChildNodeValue(style, "Name");
                extractStyle.Title = GetChildNodeValue(style, "Title");
                XmlNode legendNode = GetChildNode(style, "LegendURL");
                if(legendNode != null)
                {
                    XmlNode onlineResourceNode = GetChildNode(legendNode, "OnlineResource");
                    extractStyle.LegendURL = onlineResourceNode.Attributes.GetNamedItem("xlink:href").InnerText;
                }
                extractLayer.AddStyleToDictionary(extractStyle.Name, extractStyle);
            }
            wmsLayers.Add(extractLayer);
            Debug.Log(extractLayer);
        }

        resetReaderEvent.Invoke();
        wmsLayerEvent.Invoke(wmsLayers);

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
