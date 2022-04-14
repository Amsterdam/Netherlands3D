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
using Netherlands3D.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FilterObjectsByGeoJSON : MonoBehaviour
{
	private List<string> retrievedIDs = new List<string>();
	private List<float> retrievedFloats = new List<float>();

	[SerializeField]
	private string idProperty = "identificatie";
	private string filterProperty = "bouwjaar";
	private float filterValue = 1900;

	[SerializeField]
	private ComparisonOperator filterComparisonOperator = ComparisonOperator.PropertyIsLessThan;

	[SerializeField]
	private ObjectEvent retrievedObjectsAndYear;

	[SerializeField]
	private FloatEvent setFilterValue;

	[SerializeField]
    private string geoJsonRequestURL = "https://service.pdok.nl/lv/bag/wfs/v2_0?SERVICE=WFS&VERSION=2.0.0&outputFormat=geojson&REQUEST=GetFeature&typeName=bag:pand&count=1000&outputFormat=xml&srsName=EPSG:28992&bbox={bbox}";

	/// <summary>
	/// Request operator comparison type. We do this post retrieving the data, but these can be used in the Filter of the WFS request too.
	/// </summary>
	public enum ComparisonOperator
	{
		PropertyIsEqualTo,
		PropertyIsNotEqualTo,
		PropertyIsLessThan,
		PropertyIsLessThanOrEqualTo,
		PropertyIsGreaterThan,
		PropertyIsGreaterThanOrEqualTo
	}

    void Start()
    {
		if (setFilterValue) setFilterValue.started.AddListener((value) => filterValue = value);


		StartCoroutine(GetFilteredObjects());
    }

	IEnumerator GetFilteredObjects()
	{
		retrievedIDs.Clear();
		retrievedFloats.Clear();

		var bboxByCameraBounds = Camera.main.GetRDExtent(10000);
		var bbox = $"{bboxByCameraBounds.MinX},{bboxByCameraBounds.MinY},{bboxByCameraBounds.MaxX},{bboxByCameraBounds.MaxY}";
		var requestUrl = geoJsonRequestURL.Replace("{bbox}", bbox);

		var webRequest = UnityWebRequest.Get(requestUrl);
		yield return webRequest.SendWebRequest();

		if (webRequest.result == UnityWebRequest.Result.Success)
		{
			GeoJSON customJsonHandler = new GeoJSON(webRequest.downloadHandler.text);
			while (customJsonHandler.GotoNextFeature())
			{
				var id = customJsonHandler.GetPropertyStringValue(idProperty);
				var value = customJsonHandler.GetPropertyFloatValue(filterProperty);

				retrievedIDs.Add(id);
				retrievedFloats.Add(value);
			}

			InvokeFilteredIdsAndValues();
		}
	}

	private void InvokeFilteredIdsAndValues()
	{
		Dictionary<string, float> stringAndFloat = new Dictionary<string, float>();
		for (int i = 0; i < retrievedFloats.Count; i++)
		{
			var value = retrievedFloats[i];
			if(ComparesToFilter(value))
			{
				
			}
		}
	}

	private bool ComparesToFilter(float value)
	{
		switch (filterComparisonOperator)
		{
			case ComparisonOperator.PropertyIsEqualTo:
				return (value == filterValue);
			case ComparisonOperator.PropertyIsNotEqualTo:
				return (value != filterValue);
			case ComparisonOperator.PropertyIsLessThan:
				return (value < filterValue);
			case ComparisonOperator.PropertyIsLessThanOrEqualTo:
				return (value <= filterValue);
			case ComparisonOperator.PropertyIsGreaterThan:
				return (value > filterValue);
			case ComparisonOperator.PropertyIsGreaterThanOrEqualTo:
				return (value >= filterValue);
			default:
				break;
		}
		return false;
	}
}
