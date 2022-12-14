using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class WFSFormatter
{
    private XmlNamespaceManager namespaceManager;
    //private string namespacePrefix = "";
    private XmlDocument xml;
    public WFS ReadFromWFS(WFS wfs, XmlDocument wmsXml)
    {

        xml = wmsXml;
        FindNamespaces();
        bool allowsGeoJSON = false;

        XmlNode operationsMetadata = GetChildNode(wmsXml.DocumentElement, "OperationsMetadata", "ows");
        XmlNodeList operations = GetChildNodes(operationsMetadata, "Operation", "ows");
        foreach(XmlNode o in operations)
        {
            if (o.Attributes.GetNamedItem("name")?.Value == "GetFeature")
            {
                Debug.Log("Found the Feature Node!");
                foreach(XmlNode parameter in GetChildNodes(o, "Parameter", "ows"))
                {
                    if(parameter.Attributes.GetNamedItem("name").Value == "outputFormat")
                    {
                        XmlNode allowedValues = GetChildNode(parameter, "AllowedValues", "ows");
                        foreach(XmlNode value in GetChildNodes(allowedValues, "Value", "ows"))
                        {
                            if (value.InnerText.ToLower().Contains("geojson"))
                            {
                                allowsGeoJSON = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
  
        if (!allowsGeoJSON)
            throw new System.NotImplementedException("This WFS does not support GeoJSON and currently cannot be processed!");

        XmlNode filterCapabilities = GetChildNode(xml.DocumentElement, "Filter_Capabilities", "fes");

        XmlNode conformance = GetChildNode(filterCapabilities, "Conformance", "fes");
        Debug.Log(conformance.ChildNodes.Count);

        foreach(XmlNode constraint in GetChildNodes(conformance, "Constraint", "fes"))
        {
            //Debug.Log(constraint.Attributes.GetNamedItem("name")?.InnerText);
        }

        XmlNode scalarCapabilities = GetChildNode(filterCapabilities, "Scalar_Capabilities", "fes");
        XmlNode comparisonOperators = GetChildNode(scalarCapabilities, "ComparisonOperators", "fes");

        XmlNode spatialCapabilites = GetChildNode(filterCapabilities, "Spatial_Capabilities", "fes");
        XmlNode geometryOperands = GetChildNode(spatialCapabilites, "GeometryOperands", "fes");
        XmlNode spatialOperators = GetChildNode(spatialCapabilites, "SpatialOperators", "fes");

        XmlNode temporalCapabilities = GetChildNode(filterCapabilities, "Temporal_Capabilities", "fes");
        XmlNode temporalOperands = GetChildNode(temporalCapabilities, "TemporalOperands", "fes");
        XmlNode temporalOperators = GetChildNode(temporalCapabilities, "TemporalOperators", "fes");

        XmlNode functions = GetChildNode(filterCapabilities, "Functions", "fes");

        //foreach(XmlNode co in comparisonOperators)
        //{
        //    Debug.Log(co.Attributes.GetNamedItem("name")?.InnerText);
        //}
        //foreach (XmlNode go in GetChildNodes(geometryOperands, "GeometryOperand", "fes"))
        //{
        //    Debug.Log(go.Attributes.GetNamedItem("name")?.InnerText);
        //}
        //foreach (XmlNode so in GetChildNodes(spatialOperators, "SpatialOperator", "fes"))
        //{
        //    Debug.Log(so.Attributes.GetNamedItem("name")?.InnerText);
        //}
        //foreach(XmlNode to in GetChildNodes(temporalOperands, "TemporalOperand", "fes"))
        //{
        //    Debug.Log(to.Attributes.GetNamedItem("name")?.InnerText);
        //}
        //foreach (XmlNode to in GetChildNodes(temporalOperators, "TemporalOperator", "fes"))
        //{
        //    Debug.Log(to.Attributes.GetNamedItem("name")?.InnerText);
        //}

        if(functions != null)
        {
            Debug.Log(functions.ChildNodes.Count);
            //foreach(XmlNode funct in GetChildNodes(functions, "Function", "fes"))
            //{
            //    Debug.Log(funct.Attributes.GetNamedItem("name")?.InnerText);
            //}
        }


        XmlNode featureList = GetChildNode(xml.DocumentElement, "FeatureTypeList", "wfs");
        foreach(XmlNode feature in GetChildNodes(featureList, "FeatureType", "wfs"))
        {
            Debug.Log("Found a feature in the feature type list");
            wfs.features.Add(new WFSFeature(GetChildNodeValue(feature, "Name", "wfs")));
        }
        //Debug.Log(featureList.InnerText);

        string version = xml.DocumentElement.Attributes.GetNamedItem("version").InnerText;
        wfs.Version = version;
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
        namespaceManager = new XmlNamespaceManager(xml.NameTable);

        if (xml.DocumentElement.Attributes.GetNamedItem("xmlns:fes") != null)
        {
            string ns = xml.DocumentElement.Attributes.GetNamedItem("xmlns:fes").InnerText;
            namespaceManager.AddNamespace("fes", ns);
        }
        if (xml.DocumentElement.Attributes.GetNamedItem("xmlns:ows") != null)
        {
            string ns = xml.DocumentElement.Attributes.GetNamedItem("xmlns:ows").InnerText;
            namespaceManager.AddNamespace("ows", ns);
        }
        if (xml.DocumentElement.Attributes.GetNamedItem("xmlns") != null)
        {
            string ns = xml.DocumentElement.Attributes.GetNamedItem("xmlns").InnerText;
            namespaceManager.AddNamespace("wfs", xml.DocumentElement.Attributes.GetNamedItem("xmlns").InnerText);
        }
    }

    private string GetChildNodeValue(XmlNode parentNode, string childNodeName, string nameSpace)
    {
        XmlNode selected;
        if(nameSpace == "")
        {
            selected = parentNode.SelectSingleNode($"wfs:{childNodeName}", namespaceManager);
        }
        else
        {
            selected = parentNode.SelectSingleNode($"{nameSpace}:{childNodeName}", namespaceManager);
        }
        if (selected != null)
        {
            return selected.InnerText;
        }
        return "";
    }

    private XmlNode GetChildNode(XmlNode parentNode, string childNodeName, string nameSpace)
    {
        if (nameSpace == "")
        {
            return parentNode.SelectSingleNode($"wfs:{childNodeName}", namespaceManager);
        }
        return parentNode.SelectSingleNode($"{nameSpace}:{childNodeName}", namespaceManager);
    }
    private XmlNodeList GetChildNodes(XmlNode parentNode, string childNodeName, string nameSpace)
    {
        if (nameSpace == "")
        {
            return parentNode.SelectNodes($"wfs:{childNodeName}", namespaceManager);
        }
        return parentNode.SelectNodes($"{nameSpace}:{childNodeName}", namespaceManager);
    }

}
