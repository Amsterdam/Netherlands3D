using Netherlands3D.Events;
using Netherlands3D.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BAGWFSDataLoader : MonoBehaviour
{
    [Header("Listen to")]
    [SerializeField]
    private StringListEvent loadBagIDData;

	[Header("Invoke")]
	[SerializeField]
	private StringListEvent loadedBagProperty;


	[Tooltip("{BagID} will be replaced with the bag ID variable")]
    [SerializeField]
    private string buildingWFSRequestURL = "https://service.pdok.nl/lv/bag/wfs/v2_0?SERVICE=WFS&VERSION=2.0.0&outputFormat=geojson&REQUEST=GetFeature&typeName=bag:pand&count=100&outputFormat=xml&srsName=EPSG:28992&filter=%3cFilter%3e%3cPropertyIsEqualTo%3e%3cPropertyName%3eidentificatie%3c/PropertyName%3e%3cLiteral%3e{BagID}%3c/Literal%3e%3c/PropertyIsEqualTo%3e%3c/Filter%3e";

    [Tooltip("{BagID} will be replaced with the bag ID variable")]
    [SerializeField]
    private string residenceWFSRequestURL = "https://service.pdok.nl/lv/bag/wfs/v2_0?service=wfs&request=getFeature&version=2.0.0&outputFormat=geojson&typeName=bag:verblijfsobject&filter=%3CFilter%3E%3CPropertyIsEqualTo%3E%3CPropertyName%3Epandidentificatie%3C/PropertyName%3E%3CLiteral%3E{BagID}%3C/Literal%3E%3C/PropertyIsEqualTo%3E%3C/Filter%3E";

	private Coroutine downloadProcess;

    void Start()
    {
        loadBagIDData.started.AddListener(DownloadWFSBagIDData);
    }

	private void DownloadWFSBagIDData(List<string> bagIDs)
	{
		if(bagIDs.Count > 0)
        {
            var ID = bagIDs[0];
			if (downloadProcess != null)
			{
				StopCoroutine(downloadProcess);
			}
			downloadProcess = StartCoroutine(GetIDData(ID));
        }
	}

	IEnumerator GetIDData(string bagID)
	{
		print($"Load BAG data for {bagID}");

		var requestUrl = buildingWFSRequestURL.Replace("{BagID}", bagID);
		var webRequest = UnityWebRequest.Get(requestUrl);
		yield return webRequest.SendWebRequest();

		if (webRequest.result == UnityWebRequest.Result.Success)
		{
			print(requestUrl);
			GeoJSON customJsonHandler = new GeoJSON(webRequest.downloadHandler.text);
			while (customJsonHandler.GotoNextFeature())
			{
				var properties = customJsonHandler.GetProperties();
				foreach(KeyValuePair<string,object> propertyKeyAndValue in properties)
				{
					var propertyAndValue = new List<string>();
					print($"{propertyKeyAndValue.Key}:{propertyKeyAndValue.Value}");
					propertyAndValue.Capacity = 2;
					propertyAndValue.Add(propertyKeyAndValue.Key);
					propertyAndValue.Add(propertyKeyAndValue.Value.ToString());

					loadedBagProperty.Invoke(propertyAndValue);
				}
			}
		}
	}
}
