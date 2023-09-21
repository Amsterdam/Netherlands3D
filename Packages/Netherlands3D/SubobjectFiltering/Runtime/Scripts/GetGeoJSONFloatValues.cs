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

using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Coordinates;
using Netherlands3D.Events;
using Netherlands3D.GeoJSON;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Retrieves GeoJSON id's and specific floats based on a Camera based bbox
/// You can remove the bbox parameters from your request to get all data.
/// </summary>
public class GetGeoJSONFloatValues : MonoBehaviour
{
	private Dictionary<string, float> stringsAndFloats = new Dictionary<string, float>();

	[SerializeField]
	private string geoJsonRequestURL = "https://service.pdok.nl/lv/bag/wfs/v2_0?SERVICE=WFS&VERSION=2.0.0&outputFormat=geojson&REQUEST=GetFeature&typeName=bag:pand&count={count}&startIndex={startIndex}&outputFormat=xml&srsName=EPSG:28992&bbox={bbox}";

	[SerializeField]
	private string idProperty = "identificatie";
	[SerializeField]
	private string floatDataProperty = "bouwjaar";

	[SerializeField]
	private bool clearDataOnViewChange = true;

	private float maxBoundsDistance = 10000;
	private int count = 1000;
	private int startIndex = 0;
	private bool loadedResultsForArea = false;

	[Header("Invoke")]
	[SerializeField]
	private BoolEvent busyLoadingData;
	[SerializeField]
	private ObjectEvent filteredIdsAndFloats;

	private Extent bboxByCameraBounds;

	private Coroutine runningRequest;
	private UnityWebRequest runningWebRequest;

	public bool LoadedAllResultsForArea
	{
		get
		{
			return loadedResultsForArea;
		}
		private set
		{
			loadedResultsForArea = value;
			busyLoadingData.InvokeStarted(!loadedResultsForArea);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(0, 1, 0, 0.3f);
		var center = CoordinateConverter.RDtoUnity(new Vector3RD(bboxByCameraBounds.CenterX, bboxByCameraBounds.CenterY, 0));
		float height = (float)(bboxByCameraBounds.MaxY - bboxByCameraBounds.MinY);
		float width = (float)(bboxByCameraBounds.MaxX - bboxByCameraBounds.MinX);

		Gizmos.DrawCube(center, new Vector3(width, 30, height));
	}

	private void Update()
	{
		CompareCameraExtents();
	}

	private void OnEnable()
	{
		UpdateBoundsByCameraExtent();
		ClearData();
		FetchNewAreaFilteredObjects();
	}

	private void OnDisable()
	{
		ClearData();
	}

	private void CompareCameraExtents()
	{
		var currentCameraExtents = Camera.main.GetRDExtent(maxBoundsDistance);
		var similarCameraBounds = bboxByCameraBounds.Equals(currentCameraExtents);

		if (!similarCameraBounds)
		{
			UpdateBoundsByCameraExtent();
			if(clearDataOnViewChange)
				ClearData();
			FetchNewAreaFilteredObjects();
		}
	}

	private void ClearData()
	{
		stringsAndFloats.Clear();
		LoadedAllResultsForArea = false;
	}

	private void FetchNewAreaFilteredObjects()
	{
		if (runningRequest != null)
		{
			StopCoroutine(runningRequest);
		}
		runningRequest = StartCoroutine(GetFilteredObjects());
	}

	private IEnumerator GetFilteredObjects()
	{
		for (int i = 0; i < 3; i++)
		{
			yield return new WaitForEndOfFrame();
		}

		UpdateBoundsByCameraExtent();

		var bbox = $"{bboxByCameraBounds.MinX},{bboxByCameraBounds.MinY},{bboxByCameraBounds.MaxX},{bboxByCameraBounds.MaxY}";
		var baseUrl = geoJsonRequestURL.Replace("{bbox}", bbox);

		LoadedAllResultsForArea = false;
		Dictionary<string, float> stringAndFloat = new Dictionary<string, float>();
		startIndex = 0;

		while (!LoadedAllResultsForArea)
		{
			var requestUrl = baseUrl;
			if (requestUrl.Contains("{startIndex}"))
			{
				requestUrl = requestUrl.Replace("{count}", count.ToString()).Replace("{startIndex}", startIndex.ToString());
			}
			else
			{
				LoadedAllResultsForArea = true;
			}


			if (runningWebRequest != null)
			{
				runningWebRequest.Dispose();
			}
			runningWebRequest = UnityWebRequest.Get(requestUrl);
			yield return runningWebRequest.SendWebRequest();

			if (runningWebRequest.result == UnityWebRequest.Result.Success)
			{
				GeoJSONStreamReader customJsonHandler = new GeoJSONStreamReader(runningWebRequest.downloadHandler.text);
				int featuresFoundInPage = 0;
				while (customJsonHandler.GotoNextFeature())
				{
					var id = customJsonHandler.GetPropertyStringValue(idProperty);
					var value = customJsonHandler.GetPropertyFloatValue(floatDataProperty);

					if(!stringAndFloat.ContainsKey(id))
						stringAndFloat.Add(id, value);

					featuresFoundInPage++;
				}
				filteredIdsAndFloats.InvokeStarted(stringAndFloat);

				if (featuresFoundInPage < count)
				{
					LoadedAllResultsForArea = true;
					yield break;
				}
			}
			else
			{
				LoadedAllResultsForArea = false;
				yield return new WaitForSeconds(2.0f); //Retry untill API is back up
			}

			startIndex += count;
		}
	}

	//Retrieve main camera extent
	private void UpdateBoundsByCameraExtent()
	{
		bboxByCameraBounds = Camera.main.GetRDExtent(maxBoundsDistance);
	}
}
