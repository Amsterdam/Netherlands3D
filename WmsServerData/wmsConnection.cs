using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Xml;
using Netherlands3D.Events;

public class wmsConnection : MonoBehaviour
{
    // Start is called before the first frame update
    public BoolValueUnityEvent onServerconnectionSuccesfull;

    public GameObject LayerPanelPrefab;
    public BoolEvent dataUsable;

    [SerializeField]
    private Canvas layersCanvas;


    public StringListEvent foundLayers;
    private string url="";
    private XmlDocument Capabilities;
    [SerializeField]
    private List<string> fileFormats;
    [SerializeField]
    private List<string> layerNames;
    [SerializeField]
    private List<string> crs;
    public void setURL(string urlString)
    {
        url = urlString;
        
    }
    public void TryConnect()
    {
        Debug.Log($"trying to connect to {url}");
        StartCoroutine(connectToServer(url));
    }
    private IEnumerator connectToServer(string url)
    {
        var serverRequest = UnityWebRequest.Get(url);
        yield return serverRequest.SendWebRequest();

        if (serverRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"connected to {url}");
            Capabilities = new XmlDocument();
            Capabilities.LoadXml(serverRequest.downloadHandler.text);
            onServerconnectionSuccesfull.Invoke(true);
            InterpreteCapabilities();
        }
        else
        {
            onServerconnectionSuccesfull.Invoke(false);
        }
            yield return null;
    }

    private void InterpreteCapabilities()
    {
        // make sure it is a WMS_Capabilties
        string firstNodeName = Capabilities.DocumentElement.Name;
        if (firstNodeName != "WMS_Capabilities")
        {
            Debug.Log("not a WMS-server");
        }
        else
        {
            InterpretWMSCapabilities();
        }

        if (CheckUsability())
        {
            foundLayers.started.Invoke(layerNames);
            showLayers();
        }
    }


    private bool CheckUsability()
    {
        
        if (layerNames.Count==0)
        {
            return false;
        }
        if (crs.Contains("EPSG:28992")==false)
        {
            return false;
        }

        return true;

    }

    private void showLayers()
    {
        Component[] components = layersCanvas.GetComponentsInChildren(typeof(Transform), true);
        //for (int i = components.Length - 1; i >= 0; i--)
        //{
        //    Destroy(components[i].gameObject);
        //}

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(Capabilities.NameTable);
        nsmgr.AddNamespace("wfs", "http://www.opengis.net/wms");
        //find the capabilitiesNode
        XmlNode CapabilityNode = Capabilities.DocumentElement.SelectSingleNode("wfs:Capability", nsmgr); ;
        //get topLayer
        XmlNode topLayer = CapabilityNode.SelectSingleNode("wfs:Layer", nsmgr);
        // get available crs's
        foreach (XmlNode layerNode in topLayer.ChildNodes)
        {
            if (layerNode.Name == "Layer")
            {
                string layername = layerNode.SelectSingleNode("wfs:Name", nsmgr).InnerText;
                string layerdescription = layerNode.SelectSingleNode("wfs:Abstract", nsmgr).InnerText;


                foreach (XmlNode styleNode in layerNode.SelectNodes("wfs:Style", nsmgr))
                {
                    GameObject newLayer = Instantiate(LayerPanelPrefab, layersCanvas.transform);
                    layergroup layerdata = newLayer.GetComponent<layergroup>();
                    layerdata.layername = layername;
                    string stylename = styleNode.SelectSingleNode("wfs:Name", nsmgr).InnerText;
                    XmlNode styleAbstract = styleNode.SelectSingleNode("wfs:Abstract", nsmgr);
                string styledescription;
                if (styleAbstract==null)
                {
                    styledescription = stylename;
                }
                else
                {
                    styledescription = styleAbstract.InnerText;
                }
                   
                    layerdata.style = stylename;
                    layerdata.layernameText.text = layername+" ("+ styledescription+")";
                    
                    layerdata.layerDescriptionText.text = layerdescription;
                    layerdata.baseURL = url;
                }


            }
        }

    }

    private void InterpretWMSCapabilities()
    {
        // set the namespace
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(Capabilities.NameTable);
        nsmgr.AddNamespace("wfs", "http://www.opengis.net/wms");

        //find the capabilitiesNode
        XmlNode CapabilityNode = Capabilities.DocumentElement.SelectSingleNode("wfs:Capability", nsmgr); ;
        
        // read avaiable fileFormats
        XmlNode Formats = CapabilityNode.SelectSingleNode("wfs:Request", nsmgr).SelectSingleNode("wfs:GetMap", nsmgr);
        fileFormats = new List<string>();
        foreach (XmlNode format in Formats.ChildNodes)
        {
            fileFormats.Add(format.InnerText);
        }

        //get topLayer
        XmlNode topLayer = CapabilityNode.SelectSingleNode("wfs:Layer", nsmgr);
        // get available crs's
        foreach (XmlNode crsNode in topLayer.ChildNodes)
        {
            if (crsNode.Name == "CRS")
            {
                crs.Add(crsNode.InnerText);
            }
        }

        //get available Layers
        layerNames = new List<string>();
        foreach (XmlNode layer in topLayer.ChildNodes)
        {
            if (layer.Name == "Layer")
            {
                layerNames.Add(layer.SelectSingleNode("wfs:Name", nsmgr).InnerText);
            }
        }
    }
}
