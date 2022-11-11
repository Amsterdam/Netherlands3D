using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using Netherlands3D.Events;

public class WMSFormatter : MonoBehaviour
{
    public WMS CurrentWMS { get; private set; }
    public static WMSFormatter Instance { get; private set; }

    [Header("Events")]
    [SerializeField] private TriggerEvent resetReaderEvent;
    [SerializeField] private ObjectEvent wmsLayerEvent;

    private XmlNamespaceManager namespaceManager;
    private string namespacePrefix = "";
    private XmlDocument xml;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("An instance of WMS Formatter already exists!");
            return;
        }
        Instance = this;
    }
    public void ReadLayersFromWMS(XmlDocument wmsXml)
    {
        if (resetReaderEvent == null || wmsLayerEvent == null)
        {
            Debug.LogError("Events aren't properly set up! Please resolve this!");
        }
        xml = wmsXml;
        FindNamespaces();

        XmlNode capabilityNode = GetChildNode(xml.DocumentElement, "Capability");
        string version = xml.DocumentElement.Attributes.GetNamedItem("version").InnerText;

        CurrentWMS = new WMS(version);
        // We create a new WMS if one is being submitted from the Input Field, we also give it the version as a parameter in the constructor (this won't change anymore).

        XmlNode topLayer = GetChildNode(capabilityNode, "Layer");
        XmlNodeList subLayers = GetChildNodes(topLayer, "Layer");

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
            CurrentWMS.layers.Add(extractLayer);
        }

        resetReaderEvent.Invoke();
        wmsLayerEvent.Invoke(CurrentWMS.layers);

    }

    public void SetMapDimensions(Vector2Int dimensions)
    {
        CurrentWMS.Dimensions = dimensions;
    }
    public void SetResolution(string resolution)
    {
        int res = int.Parse(resolution);
        CurrentWMS.Dimensions = new Vector2Int(res, res);
    }

    public void SetBoundingBoxMinX(string value)
    {
        CurrentWMS.BBox.MinX = int.Parse(value);
    }
    public void SetBoundingBoxMaxX(string value)
    {
        CurrentWMS.BBox.MaxX = int.Parse(value);
    }
    public void SetBoundingBoxMinY(string value)
    {
        CurrentWMS.BBox.MinY = int.Parse(value);
    }
    public void SetBoundingBoxMaxY(string value)
    {
        CurrentWMS.BBox.MaxY = int.Parse(value);
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
