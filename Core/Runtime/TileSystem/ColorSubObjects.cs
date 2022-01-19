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
        private int colorColumn = 2;

        [SerializeField]
        private ColorInterpretation colorInterpretation = ColorInterpretation.HEX;

        [Header("Interpolation")]
        [SerializeField]
        private double minimumValue;
        [SerializeField]
        private double maximumValue;
        [SerializeField]
        private Gradient gradient;

        public enum ColorInterpretation{
            HEX,
            INTERPOLATE
		}

        public class ColorAndValue{
            public float value = 0;
            public Color color;
		}

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
                    //Ready CSV lines ( skip header )
                    var lines = CsvParser.ReadLines(webRequest.downloadHandler.text, 1);
                    foreach (var line in lines)
                    {
                        Color color = Color.magenta;
                        string id = line[0];
                        ParseColor(line[colorColumn], out color);
                        if(!idColors.ContainsKey(id))
                            idColors.Add(id, color);
                    }
                }
                UpdateColors();
            }
        }

        private void ParseColor(string colorInput, out Color color){
            color = Color.white;
			switch (colorInterpretation)
			{
				case ColorInterpretation.HEX:
                    ColorUtility.TryParseHtmlString(colorInput, out color);
                    break;
				case ColorInterpretation.INTERPOLATE:
                    if(float.TryParse(colorInput, out float parsed))
                    {
                        color = gradient.Evaluate(Mathf.InverseLerp((float)minimumValue, (float)maximumValue, parsed));
					}
                    else{
                        Debug.Log($"Cant parse {colorInput} as float");
                    }
					break;
				default:
					break;
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