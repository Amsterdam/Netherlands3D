using Netherlands3D.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.TileSystem
{
    public class ColorSubObjects : MonoBehaviour
    {
        private Dictionary<string, Color> idColors;

        [Header("CSV with:  id;color")]
        [SerializeField]
        private string dataSource = "file:///somecsv.csv";

        [SerializeField]
        private int idColumn = 0;

        [SerializeField]
        private int hexColorColumn = 2;

        private void OnEnable()
        {
            if (idColors == null)
            {
                StartCoroutine(LoadCSV());
            }
            else
            {
                UpdateColors();
            }
        }

        private IEnumerator LoadCSV()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(dataSource))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Could not load {dataSource}");
                }
                else
                {
                    idColors = new Dictionary<string, Color>();
                    var lines = CsvParser.ReadLines(webRequest.downloadHandler.text, 1);
                    foreach (var line in lines)
                    {
                        Color color = Color.magenta;
                        string id = line[0];
                        ColorUtility.TryParseHtmlString(line[hexColorColumn], out color);
                        idColors.Add(id, color);
                    }
                }
                UpdateColors();
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();

            var allSubObjects = gameObject.GetComponentsInChildren<SubObjects>();
            for (int i = allSubObjects.Length - 1; i >= 0; i--)
            {
                allSubObjects[i].ResetColors();
                Destroy(allSubObjects[i]);
            }
        }

        private void OnTransformChildrenChanged()
        {
            UpdateColors();
        }

        private void UpdateColors()
        {
            if (idColors == null) return;

            foreach (Transform child in transform)
            {
                SubObjects subObjects = child.gameObject.GetComponent<SubObjects>();
                if (!subObjects && child.gameObject.GetComponent<MeshFilter>())
                {
                    subObjects = child.gameObject.AddComponent<SubObjects>();
                    subObjects.ColorObjectsByID(idColors);
                }
            }
        }
    }
}