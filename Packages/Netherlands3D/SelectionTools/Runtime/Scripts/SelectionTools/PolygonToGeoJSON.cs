using Netherlands3D.Core;
using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PolygonToGeoJSON : MonoBehaviour
{
    [Header("Listen to")]
    [SerializeField] private Vector3ListEvent gotPolygon;
    [SerializeField] private InputField inputField;

    private string geoJsonTemplate = "{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[<vector2array>]]},\"properties\":{\"identifier\":\"polygon\"}}]}";
    private string arrayVariableString = "<vector2array>";

    void Awake()
    {
        gotPolygon.AddListenerStarted(GotPolygon);
    }

    private void GotPolygon(List<Vector3> polygon)
    {
        var stringBuilder = new StringBuilder();
        for (int i = 0; i < polygon.Count; i++)
        {
            var position = polygon[i];
            var rdCoordinate = CoordConvert.UnitytoWGS84(position);

            stringBuilder.AppendLine($"[{rdCoordinate.lon},{rdCoordinate.lat}]");
            if (i != polygon.Count - 1) stringBuilder.Append(",");
        }

        string geoJson = geoJsonTemplate.Replace(arrayVariableString, stringBuilder.ToString());
        inputField.text = geoJson;
    }
}
