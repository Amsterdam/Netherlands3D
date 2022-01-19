using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Events;


public class layergroup : MonoBehaviour
{
    public StringEvent createLayer;
    public Text layernameText;
    public Text layerDescriptionText;

    public string layername;
    public string style;

    public string baseURL;


    string version = "1.1.1";  //needs to be read from server getcapabilities
    string request = "GETMAP";
    string width = "1024";
    string height = "1024";
    string format = "image/jpeg";  //needs check if available
    string srs = "epsg:28992";
    string bbox = "120000, 487000, 121000, 488000";

    public void createURL()
    {
        string newURL = baseURL.Replace("request=GetCapabilities&", "");
        string tempURL = $"{newURL}&request={request}&VERSION={version}&LAYERS={layername}&STYLES={style}&WIDTH={width}&HEIGHT={height}&FORMAT={format}&SRS={srs}&BBOX={bbox}";
        Debug.Log(tempURL);
        createLayer.started.Invoke(tempURL);
    }
    
}
