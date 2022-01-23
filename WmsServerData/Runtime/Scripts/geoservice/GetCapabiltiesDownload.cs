using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.wmsServer;
using System.Reflection;
using System.Linq;

public class GetCapabiltiesDownload : MonoBehaviour
{
    // Start is called before the first frame update

    public ServerData serverData;
    [SerializeField]
    public List<ImageGeoservice> services = new List<ImageGeoservice>();
    private void Start()
    {
        serverData.loadGetCapabilities.AddListener(GetTextDataFromURL);
        //collect imageGeoServiceTypes

        getAvailableImageGeoServices();

    }

    private void getAvailableImageGeoServices()
    {
        serverData.services.Clear();
        serverData.availableServices.Clear();
        var types = Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => typeof(ImageGeoservice).IsAssignableFrom(t) &&
                        t != typeof(ImageGeoservice))
                    .ToArray();
        foreach (var item in types)
        {
            ImageGeoservice newService = (ImageGeoservice)System.Activator.CreateInstance(item);
            serverData.services.Add(newService);
            serverData.availableServices.Add(newService.getType());
        }
    }

    public void GetTextDataFromURL()
    {
        StartCoroutine(connectToServer(serverData.getCapabilitiesURL));
    }

    private IEnumerator connectToServer(string url)
    {
        var serverRequest = UnityWebRequest.Get(url);
        yield return serverRequest.SendWebRequest();

        if (serverRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"connected to {url}");


            // continue with the received data
            serverData.readXML(serverRequest.downloadHandler.text);
            serverData.downloadSuccesfull(true);
        }
        else
        {
            serverData.downloadSuccesfull(false);


        }
        yield return null;
    }

}
