using System;
using System.Collections.Generic;
using System.Xml;
using Netherlands3D.GeoJSON;
using Netherlands3D.Web;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.WFSHandlers
{
    public class WFS2
    {
        public string BaseUrl { get; private set; }
        public Dictionary<string, string> RequestHeaders { get; set; }
        public BoundingBox BBox;

        public UriQueryParameter serviceQuery = new("SERVICE", "WFS");
        public UriQueryParameter getCapabilitiesQuery = new("REQUEST", "GetCapabilities");
        public UriQueryParameter getFeatureQuery = new("REQUEST", "GetFeature");
        public UriQueryParameter describeFeatureTypeQuery = new("REQUEST", "DescribeFeatureType");
        public UriQueryParameter versionQuery = new("VERSION", "2.0.0");
        //public UrlQueryParameter typeNameQuery = new UrlQueryParameter("TYPENAME", "");
        public UriQueryParameter outputFormatQuery = new("OUTPUTFORMAT", "geojson");
        public UriQueryParameter countQuery = new("COUNT", "0");
        public UriQueryParameter startIndexQuery = new("STARTINDEX", "0");
        public UriQueryParameter boundingBoxQuery => new("bbox", $"{BBox.MinX},{BBox.MinY},{BBox.MaxX},{BBox.MaxY}");

        public UnityEvent<object, List<WFSFeature>> getCapabilitiesReceived = new();
        public UnityEvent<object, List<WFSFeatureDescriptor>> featureDescriptorsReceived = new();
        public UnityEvent<object, GeoJSONStreamReader> rawGeoJSONReceived = new();
        public UnityEvent<object, List<WFSFeatureData>> featureDataReceived = new();

        public WFS2(string baseURL)
        {
            BaseUrl = baseURL;
        }

        public WFS2(string baseURL, Dictionary<string, string> requestHeaders)
        {
            BaseUrl = baseURL;
            RequestHeaders = requestHeaders;
        }

        #region STANDARD_REQUESTS

        public string GetCapabilitiesURL()
        {
            var url = new UriBuilder(BaseUrl);
            url.AddQueryParameter(serviceQuery);
            url.AddQueryParameter(getCapabilitiesQuery);
            return url.ToString();
        }

        public string GetFeatureURL(string typeName, List<string> propertyFilters)
        {
            var url = new UriBuilder(BaseUrl);
            url.AddQueryParameter(serviceQuery);
            url.AddQueryParameter(versionQuery);
            url.AddQueryParameter(getFeatureQuery);
            var typeNameQuery = new UriQueryParameter("TYPENAME", typeName);
            url.AddQueryParameter(typeNameQuery);
            url.AddQueryParameter(boundingBoxQuery);
            url.AddQueryParameter(outputFormatQuery);

            if (countQuery.Value != "0")
            {
                url.AddQueryParameter(countQuery);
            }
            if (countQuery.Value != "0")
            {
                url.AddQueryParameter(startIndexQuery);
            }

            if (propertyFilters != null && propertyFilters.Count > 0)
            {
                var propertyFilterValue = "";
                foreach (var propertyFilter in propertyFilters)
                {
                    propertyFilterValue += $"{propertyFilter},";
                }
                propertyFilterValue = propertyFilterValue.Remove(propertyFilterValue.Length - 1);//remove trailing comma
                url.AddQueryParameter("PROPERTYNAME", propertyFilterValue);
            }

            return url.ToString();
        }

        private string DescribeFeatureTypeURL()
        {
            var url = new UriBuilder(BaseUrl);
            url.AddQueryParameter(serviceQuery);
            url.AddQueryParameter(versionQuery);
            url.AddQueryParameter(describeFeatureTypeQuery);
            return url.ToString();
        }
        #endregion

        #region REQUEST_METHODS
        public void RequestWFSGetCapabilities(object source)
        {
            var url = GetCapabilitiesURL();
            WebRequest.CreateWebRequest(url, RequestHeaders, (data) => ProcessGetCapabilites(source, data));
        }

        private void ProcessGetCapabilites(object source, string getCapabilitiesXML)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(getCapabilitiesXML);

            XmlElement serviceID = xml.DocumentElement["ows:ServiceIdentification"]["ows:ServiceType"];
            if (serviceID != null && serviceID.InnerText.Contains("WFS"))
            {
                var features = ReadFromWFS(xml, out var version);
                versionQuery = new UriQueryParameter(versionQuery.Key, version);
                getCapabilitiesReceived.Invoke(source, features);
            }
        }

        public void GetDescribeFeatureType(object source)
        {
            string url = DescribeFeatureTypeURL();

            WebRequest.CreateWebRequest(url, RequestHeaders, (data) => ProcessGetDescribeFeatureType(source, data));
        }

        private void ProcessGetDescribeFeatureType(object source, string describeFeatureTypeXML)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(describeFeatureTypeXML);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(xml.NameTable);
            nsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            XmlNodeList elements = xml.SelectNodes("//xsd:element", nsManager);

            var featureDescriptors = new List<WFSFeatureDescriptor>();
            foreach (XmlNode element in elements)
            {
                string name = element.Attributes["name"].Value;
                string typeString = element.Attributes["type"].Value;
                //Debug.Log($"Name: {name}, Type: {typeString}");
                if (Enum.TryParse(typeof(FeatureType), typeString, true, out var type))
                {
                    featureDescriptors.Add(new WFSFeatureDescriptor(name, (FeatureType)type));
                }
                else
                {
                    Debug.Log(typeString + " is not an available FeatureType, setting it to custom: " + name);
                    featureDescriptors.Add(new WFSFeatureDescriptor(name, typeString));
                }
            }

            featureDescriptorsReceived.Invoke(source, featureDescriptors);
        }

        public void GetFeatureData(object source, string typeName, List<string> propertyNames = null)
        {
            var url = GetFeatureURL(typeName, propertyNames);
            WebRequest.CreateWebRequest(url, RequestHeaders, (data) => ProcessGetFeature(source, data));
        }

        public void GetFeatureDataWithGeometry(object source, string typeName, List<string> propertyNames = null)
        {
            var url = GetFeatureURL(typeName, propertyNames);
            WebRequest.CreateWebRequest(url, RequestHeaders, (data) => ProcessGetFeatureWithGeometry(source, data));
        }

        private void ProcessGetFeature(object source, string geoJSONString)
        {
            var geoJSON = new GeoJSONStreamReader(geoJSONString);
            //Debug.Log("Handling Feature JSON!");

            var list = new List<WFSFeatureData>();
            while (geoJSON.GotoNextFeature())
            {
                var featureData = new WFSFeatureData();
                featureData.GeometryType = geoJSON.GetGeometryType();
                featureData.TransferDictionary(geoJSON.GetProperties());
                list.Add(featureData); //list of all features for this request only
            }
            //geoJSON.FindFirstFeature(); //reset geoJSON to allow it to be used by other classes through the event.
            //rawGeoJSONReceived.Invoke(source, geoJSON); //if multiple classes listen to this event, reading the GeoJSON will cause problems because it retains the featureIndex
            featureDataReceived.Invoke(source, list); //todo: check if any listeners are present and if not skip the processing
        }

        private void ProcessGetFeatureWithGeometry(object source, string geoJSONString)
        {
            var geoJSON = new GeoJSONStreamReader(geoJSONString);
            var geometry = new GeoJSONGeometry();
            geometry.ActiveGeoJsonStreamReader = geoJSON;
            //Debug.Log("Handling Feature JSON!");

            var list = new List<WFSFeatureData>();
            while (geoJSON.GotoNextFeature())
            {
                var featureData = new WFSFeatureData();
                featureData.GeometryType = geoJSON.GetGeometryType();
                featureData.TransferDictionary(geoJSON.GetProperties());
                list.Add(featureData); //list of all features for this request only
                geometry.EvaluateGeoType();
            }
            geoJSON.FindFirstFeature(); //reset geoJSON to allow it to be used by other classes through the event.
            rawGeoJSONReceived.Invoke(source, geoJSON); //if multiple classes listen to this event, reading the GeoJSON will cause problems because it retains the featureIndex
            featureDataReceived.Invoke(source, list); //todo: check if any listeners are present and if not skip the processing
        }
        #endregion

        #region XMLParser
        public static List<WFSFeature> ReadFromWFS(XmlDocument wmsXml, out string version) //todo: is returning features needed?
        {
            var namespaceManager = FindNamespaces(wmsXml);
            bool allowsGeoJSON = false;

            XmlNode operationsMetadata = GetChildNode(wmsXml.DocumentElement, "OperationsMetadata", "ows", namespaceManager);
            XmlNodeList operations = GetChildNodes(operationsMetadata, "Operation", "ows", namespaceManager);
            foreach (XmlNode o in operations)
            {
                if (o.Attributes.GetNamedItem("name")?.Value == "GetFeature")
                {
                    foreach (XmlNode parameter in GetChildNodes(o, "Parameter", "ows", namespaceManager))
                    {
                        if (parameter.Attributes.GetNamedItem("name").Value == "outputFormat")
                        {
                            XmlNode allowedValues = GetChildNode(parameter, "AllowedValues", "ows", namespaceManager);
                            foreach (XmlNode value in GetChildNodes(allowedValues, "Value", "ows", namespaceManager))
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
                throw new NotImplementedException("This WFS does not support GeoJSON and currently cannot be processed!");

            XmlNode filterCapabilities = GetChildNode(wmsXml.DocumentElement, "Filter_Capabilities", "fes", namespaceManager);

            XmlNode conformance = GetChildNode(filterCapabilities, "Conformance", "fes", namespaceManager);

            XmlNode featureList = GetChildNode(wmsXml.DocumentElement, "FeatureTypeList", "wfs", namespaceManager);
            var features = new List<WFSFeature>();
            foreach (XmlNode feature in GetChildNodes(featureList, "FeatureType", "wfs", namespaceManager))
            {
                WFSFeature newFeature = new WFSFeature(GetChildNodeValue(feature, "Name", "wfs", namespaceManager));
                newFeature.CRS.Add(GetChildNodeValue(feature, "DefaultCRS", "wfs", namespaceManager));
                foreach (XmlNode crs in GetChildNodes(feature, "OtherCRS", "wfs", namespaceManager))
                {
                    newFeature.CRS.Add(crs.InnerText);
                }
                features.Add(newFeature);
            }

            string xmlVersion = wmsXml.DocumentElement.Attributes.GetNamedItem("version").InnerText;
            version = xmlVersion;
            return features;
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
        #endregion
    }
}
