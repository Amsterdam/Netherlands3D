using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
namespace Netherlands3D.Geoservice
{
    public static class createGeServiceUI
    {
        [MenuItem("Netherlands3D/Geoservice/Add GeoServiceConnector")]
        static void createPrefabs()
        {
            GameObject newGO = new GameObject("GeoServiceConnector");
            GeoServiceConnector geoServiceConnector= newGO.AddComponent<GeoServiceConnector>();
            GameObject resultsContainer = new GameObject("GeoServiceResultscontainer");
            ShowGeoServiceResults geoServiceResults = resultsContainer.AddComponent<ShowGeoServiceResults>();
            geoServiceResults.dataOwner = geoServiceConnector;

        }


    }
}
#endif
