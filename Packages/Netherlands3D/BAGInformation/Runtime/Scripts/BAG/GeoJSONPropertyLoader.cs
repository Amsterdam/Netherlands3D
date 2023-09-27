/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/Netherlands3D/blob/main/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.GeoJSON;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.BAGInformation
{
	/// <summary>
	/// Loads GeoJSON from an URL using unique ID's, and invoke events for
	/// returned key/value pairs for all properties.
	/// </summary>
	public class GeoJSONPropertyLoader : MonoBehaviour
	{
		[SerializeField]
		private string idReplacementString = "{BagID}";

		[Tooltip("Id replacement string will be replaced")]
		[SerializeField]
		private string geoJsonRequestURL = "https://service.pdok.nl/lv/bag/wfs/v2_0?SERVICE=WFS&VERSION=2.0.0&outputFormat=geojson&REQUEST=GetFeature&typeName=bag:pand&count=100&outputFormat=xml&srsName=EPSG:28992&filter=%3cFilter%3e%3cPropertyIsEqualTo%3e%3cPropertyName%3eidentificatie%3c/PropertyName%3e%3cLiteral%3e{BagID}%3c/Literal%3e%3c/PropertyIsEqualTo%3e%3c/Filter%3e";
		[SerializeField] private string removeFromID = "NL.IMBAG.Pand.";

		[Header("Listen to")]
		[SerializeField]
		private StringListEvent loadPropertiesForIDs;

		[Header("Invoke")]
		[SerializeField]
		private TriggerEvent loadedFeatureWithProperties;
		[SerializeField]
		private StringListEvent loadedPropertyKeyValue;

		private Coroutine downloadProcess;

		void Start()
		{
			loadPropertiesForIDs.AddListenerStarted(DownloadGeoJSONProperties);
		}

		private void OnValidate()
		{
			if (!geoJsonRequestURL.Contains(idReplacementString))
			{
				Debug.LogWarning("Make sure the url contains the id replacement string", this.gameObject);
			}
		}

		private void DownloadGeoJSONProperties(List<string> bagIDs)
		{
			if (bagIDs.Count > 0)
			{
				var ID = bagIDs[0];
				if(removeFromID.Length > 0) ID = ID.Replace(removeFromID, "");

				if (downloadProcess != null)
				{
					StopCoroutine(downloadProcess);
				}
				downloadProcess = StartCoroutine(GetIDData(ID));
			}
		}

		IEnumerator GetIDData(string bagID)
		{
			var requestUrl = geoJsonRequestURL.Replace(idReplacementString, bagID);
			var webRequest = UnityWebRequest.Get(requestUrl);
			yield return webRequest.SendWebRequest();

			if (webRequest.result == UnityWebRequest.Result.Success)
			{
				GeoJSONStreamReader customJsonHandler = new GeoJSONStreamReader(webRequest.downloadHandler.text);
				while (customJsonHandler.GotoNextFeature())
				{
					var properties = customJsonHandler.GetProperties();
					foreach (KeyValuePair<string, object> propertyKeyAndValue in properties)
					{
						AddPropertyAndValue(propertyKeyAndValue);
					}

					if (properties.Count > 0)
					{
						loadedFeatureWithProperties.InvokeStarted();
					}
				}
			}
		}

		private void AddPropertyAndValue(KeyValuePair<string, object> propertyKeyAndValue)
		{
			var propertyAndValue = new List<string>();
			propertyAndValue.Capacity = 2;
			propertyAndValue.Add(propertyKeyAndValue.Key);
			propertyAndValue.Add(propertyKeyAndValue.Value.ToString());

			loadedPropertyKeyValue.InvokeStarted(propertyAndValue);
		}
	}
}
