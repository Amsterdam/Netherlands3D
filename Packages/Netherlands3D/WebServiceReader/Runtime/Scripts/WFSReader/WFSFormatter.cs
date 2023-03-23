using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public static class WFSFormatter
{
    //private XmlNamespaceManager namespaceManager;
    //private string namespacePrefix = "";
    //private XmlDocument xml;
    public static void ReadFromWFS(this WFS wfs, XmlDocument wmsXml)
    {
        //xml = wmsXml;
        var namespaceManager = FindNamespaces(wmsXml);
        bool allowsGeoJSON = false;

        XmlNode operationsMetadata = GetChildNode(wmsXml.DocumentElement, "OperationsMetadata", "ows", namespaceManager);
        XmlNodeList operations = GetChildNodes(operationsMetadata, "Operation", "ows", namespaceManager);
        foreach(XmlNode o in operations)
        {
            if (o.Attributes.GetNamedItem("name")?.Value == "GetFeature")
            {
                //Debug.Log("Found the Feature Node!");
                foreach(XmlNode parameter in GetChildNodes(o, "Parameter", "ows", namespaceManager))
                {
                    if(parameter.Attributes.GetNamedItem("name").Value == "outputFormat")
                    {
                        XmlNode allowedValues = GetChildNode(parameter, "AllowedValues", "ows", namespaceManager);
                        foreach(XmlNode value in GetChildNodes(allowedValues, "Value", "ows", namespaceManager))
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

        XmlNode filterCapabilities = GetChildNode(wmsXml.DocumentElement, "Filter_Capabilities", "fes", namespaceManager);

        XmlNode conformance = GetChildNode(filterCapabilities, "Conformance", "fes", namespaceManager);
        //Debug.Log(conformance.ChildNodes.Count);

        //foreach(XmlNode constraint in GetChildNodes(conformance, "Constraint", "fes"))
        //{
        //    //Debug.Log(constraint.Attributes.GetNamedItem("name")?.InnerText);
        //}

        //XmlNode scalarCapabilities = GetChildNode(filterCapabilities, "Scalar_Capabilities", "fes");
        //XmlNode comparisonOperators = GetChildNode(scalarCapabilities, "ComparisonOperators", "fes");

        //XmlNode spatialCapabilites = GetChildNode(filterCapabilities, "Spatial_Capabilities", "fes");
        //XmlNode geometryOperands = GetChildNode(spatialCapabilites, "GeometryOperands", "fes");
        //XmlNode spatialOperators = GetChildNode(spatialCapabilites, "SpatialOperators", "fes");

        //XmlNode temporalCapabilities = GetChildNode(filterCapabilities, "Temporal_Capabilities", "fes");
        //XmlNode temporalOperands = GetChildNode(temporalCapabilities, "TemporalOperands", "fes");
        //XmlNode temporalOperators = GetChildNode(temporalCapabilities, "TemporalOperators", "fes");

        //XmlNode functions = GetChildNode(filterCapabilities, "Functions", "fes");

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

        //if(functions != null)
        //{
        //    Debug.Log(functions.ChildNodes.Count);
        //    //foreach(XmlNode funct in GetChildNodes(functions, "Function", "fes"))
        //    //{
        //    //    Debug.Log(funct.Attributes.GetNamedItem("name")?.InnerText);
        //    //}
        //}


        XmlNode featureList = GetChildNode(wmsXml.DocumentElement, "FeatureTypeList", "wfs", namespaceManager);
        foreach(XmlNode feature in GetChildNodes(featureList, "FeatureType", "wfs", namespaceManager))
        {
            //Debug.Log("Found a feature in the feature type list");
            WFSFeature newFeature = new WFSFeature(GetChildNodeValue(feature, "Name", "wfs", namespaceManager));
            newFeature.CRS.Add(GetChildNodeValue(feature, "DefaultCRS", "wfs", namespaceManager));
            foreach(XmlNode crs in GetChildNodes(feature, "OtherCRS", "wfs", namespaceManager))
            {
                newFeature.CRS.Add(crs.InnerText);
            }
            //Debug.Log(newFeature.CRS.Count);
            wfs.features.Add(newFeature);
        }
        //Debug.Log(featureList.InnerText);

        string version = wmsXml.DocumentElement.Attributes.GetNamedItem("version").InnerText;
        wfs.Version = version;

        //return wfs;

    }

    public static XmlNamespaceManager FindNamespaces(XmlDocument xml)
    {
        var namespaceManager = new XmlNamespaceManager(xml.NameTable);

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

        return namespaceManager;
    }

    private static string GetChildNodeValue(XmlNode parentNode, string childNodeName, string nameSpace, XmlNamespaceManager namespaceManager)
    {

        XmlNode selected = parentNode.SelectSingleNode($"{nameSpace}:{childNodeName}", namespaceManager);
        if (selected != null)
        {
            return selected.InnerText;
        }
        return "";
    }

    private static XmlNode GetChildNode(XmlNode parentNode, string childNodeName, string nameSpace, XmlNamespaceManager namespaceManager)
    {
        return parentNode.SelectSingleNode($"{nameSpace}:{childNodeName}", namespaceManager);
    }

    private static XmlNodeList GetChildNodes(XmlNode parentNode, string childNodeName, string nameSpace, XmlNamespaceManager namespaceManager)
    {
        return parentNode.SelectNodes($"{nameSpace}:{childNodeName}", namespaceManager);
    }

}
