using System.Xml;
using System.Collections.Generic;

namespace Netherlands3D.Geoservice
{
    public class wms : ImageGeoservice
    {
        private string namespacePrefix = "";
        private XmlDocument Capabilities;
        XmlNamespaceManager nsmgr;
        public override string getType()
        {
            return "WMS";
        }

        public override bool UrlIsValid(string url)
        {
            return true;
            
        }

        private void FindNameSpaces()
        {
            namespacePrefix = "";
            nsmgr = new XmlNamespaceManager(Capabilities.NameTable);
            if (Capabilities.DocumentElement.Attributes.GetNamedItem("xmlns")!=null)
            {
                nsmgr.AddNamespace("wms", Capabilities.DocumentElement.Attributes.GetNamedItem("xmlns").InnerText);
                namespacePrefix = "wms:";
            }
        }

        public override bool readCapabilities(ServerData serverData, string xmlstring)
        {
            //parse the xml
            Capabilities = new XmlDocument();
            Capabilities.LoadXml(xmlstring);
            
            FindNameSpaces();

            //find the ServiceNode
            XmlNode ServiceNode = getChildNode(Capabilities.DocumentElement, "Service");
            // read service-properties
            serverData.ServiceName = getChildNodeValue(ServiceNode,"Name");
            serverData.ServiceTitle = getChildNodeValue(ServiceNode, "Title");
            serverData.ServiceAbstract = getChildNodeValue(ServiceNode, "Abstract");
            serverData.maxWidth = getChildNodeValue(ServiceNode, "MaxWidth");
            serverData.maxHeight = getChildNodeValue(ServiceNode, "MaxHeight");

            //find the CapabilityNode
            XmlNode CapabilityNode = getChildNode(Capabilities.DocumentElement, "Capability");
            //find the requestNode
            XmlNode requestNode = getChildNode(CapabilityNode,"Request");
            //read getMap formats
            XmlNode GetMapNode = getChildNode(requestNode, "GetMap");
            // read the available fileformats
            serverData.fileFormats = new List<string>();
            foreach (XmlNode formatNode in getChildNodes(GetMapNode,"Format"))
            {
                serverData.fileFormats.Add(formatNode.InnerText);
            }
            // get the getmapURL
            XmlNode dcptypeNode = getChildNode(GetMapNode, "DCPType");
            XmlNode httpNode = getChildNode(dcptypeNode, "HTTP");
            XmlNode getNode = getChildNode(httpNode, "Get");
            XmlNode OnlineResourceNode = getChildNode(getNode, "OnlineResource");
            serverData.TemplateURL = OnlineResourceNode.Attributes.GetNamedItem("xlink:href").InnerText;

            //find the parentLayer
            serverData.layer = new List<ImageLayerData>();
            XmlNode parentLayer = getChildNode(CapabilityNode, "Layer");
            serverData.globalCRS = new List<string>();
            foreach (XmlNode crsNode in getChildNodes(parentLayer, "CRS"))
            {
                serverData.globalCRS.Add(crsNode.InnerText.ToUpper());
            }
            foreach (XmlNode srsNode in getChildNodes(parentLayer, "SRS"))
            {
                var values = srsNode.InnerText.Split(' ');
                foreach (var srsvalue in values)
                {
                    serverData.globalCRS.Add(srsvalue.ToUpper());
                }
            }

            foreach (XmlNode item in getChildNodes(parentLayer,"Layer"))  // go through all the layers
            {
                string name = getChildNodeValue(item, "Name");
                if (name=="")   {continue;}// we arent able to request alaye without a name
                ImageLayerData layerdata = new ImageLayerData();
                layerdata.Name = name;
                layerdata.Title = getChildNodeValue(item, "Title");
                layerdata.Abstract = getChildNodeValue(item, "Abstract");
                //get available crs's
                
                foreach (XmlNode crsNode in getChildNodes(item,"CRS"))
                {
                    layerdata.CRS.Add(crsNode.InnerText.ToUpper());
                }
                foreach (XmlNode srsNode in getChildNodes(item, "SRS"))
                {
                    var values = srsNode.InnerText.Split(' ');
                    foreach (var srsvalue in values)
                    {
                        layerdata.CRS.Add(srsvalue.ToUpper());
                    }
                }
                if (layerdata.CRS.Count==0)
                {
                    layerdata.CRS = serverData.globalCRS;
                }

                // read the styles
                
                foreach (XmlNode styleNode in getChildNodes(item,"Style"))
                {
                    ImageGeoserviceStyle style = new ImageGeoserviceStyle();
                    style.Name = getChildNodeValue(styleNode, "Name");
                    style.Title = getChildNodeValue(styleNode, "Title");
                    style.Abstract = getChildNodeValue(styleNode, "Abstract");
                    XmlNode legendNode = getChildNode(styleNode,"LegendURL");
                    if (legendNode!=null)
                    {
                        OnlineResourceNode = getChildNode(legendNode, "OnlineResource");
                        style.LegendURL = OnlineResourceNode.Attributes.GetNamedItem("xlink:href").InnerText;
                    }
                    if(createImageURL(serverData,style,layerdata))
                    {
                        layerdata.styles.Add(style);
                    }
                    
                }
                if (layerdata.styles.Count==0)
                {
                    // we can only usee the default style
                    ImageGeoserviceStyle style = new ImageGeoserviceStyle();
                    style.Name = "Default";
                    if (createImageURL(serverData, style, layerdata))
                    {
                        layerdata.styles.Add(style);
                    }
                }


                //add the layer to the serverdata
                if (layerdata.styles.Count>0)
                {
                    serverData.layer.Add(layerdata);
                }
               
            }

            return true;
        }
        private string getChildNodeValue(XmlNode parentNode,string childNodename)
        {
            XmlNode SelectedNode = parentNode.SelectSingleNode($"{namespacePrefix}{childNodename}", nsmgr);
            if (SelectedNode == null)
            {
                return "";
            }
            else
            {
               return SelectedNode.InnerText;
            }
        }
        private XmlNode getChildNode(XmlNode parentNode, string childNodeName)
        {
            return parentNode.SelectSingleNode($"{namespacePrefix}{childNodeName}",nsmgr);
        }
        private XmlNodeList getChildNodes(XmlNode parentNode, string childNodeName)
        {
            return parentNode.SelectNodes($"{namespacePrefix}{childNodeName}",nsmgr);
        }


        private bool createImageURL(ServerData serverData, ImageGeoserviceStyle style, ImageLayerData layerdata)
        {

            string width = "{width}";
            string height = "{height}";
            string Format = "image/jpeg";
            string srs = "EPSG:28992";
            string bbox = "{Xmin},{Ymin},{Xmax},{Ymax}";

            if (serverData.fileFormats.Contains(Format)==false) //need to compare caseInsensitive
            {
                return false;
            }
            if (layerdata.CRS.Contains(srs)==false) //need to compare caseInsensitive
            {
                return false;
            }

            string url = serverData.TemplateURL;
            if (url.Contains("?")==false)
            {
                url = url + "?";
            }
            url = $"{url}service=WMS&request=GETMAP&version=1.1.1&LAYERS={layerdata.Name}&Styles={style.Name}&WIDTH={width}&HEIGHT={height}&format={Format}&srs={srs}&bbox={bbox}&tansparent=true";
            style.imageURL = url;
            return true;
        }
        
    }
}
