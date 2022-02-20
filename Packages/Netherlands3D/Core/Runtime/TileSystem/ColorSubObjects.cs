using Netherlands3D.Core;
using Netherlands3D.Core.Colors;
using Netherlands3D.Events;
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


        [SerializeField]
        private bool disableOnStart = false;

        [Header("Listen to")]
        [SerializeField]
        private BoolEvent onEnableDrawingColors;
        [SerializeField]
        private ObjectEvent onReceiveIdsAndColors;
        [SerializeField]
        private ObjectEvent onReceiveIdsAndFloats;

        [SerializeField]
        private FloatEvent onReceiveMinRange;
        [SerializeField]
        private FloatEvent onReceiveMaxRange;

        [SerializeField]
        private TriggerEvent onClearData;

        [Header("Or from URL")]
        [SerializeField]
        [Tooltip("CSV should have: id;color")]
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
        private GradientContainer gradientContainer;

        [Header("Default")]
        [SerializeField]
        private Color defaultColor;

        public enum ColorInterpretation
        {
            HEX,
            INTERPOLATE
        }

        public class ColorAndValue
        {
            public float value = 0;
            public Color color;
        }

        private void Awake()
        {
            if (onReceiveIdsAndColors)
            {
                onReceiveIdsAndColors.started.AddListener(SetIDsAndColors);
                onEnableDrawingColors.started.Invoke(true);
                this.enabled = true;
            }

            if (onReceiveIdsAndFloats)
            {
                onReceiveIdsAndFloats.started.AddListener(SetIDsAndFloatsAsColors);

                //If we can receive ids+floats, add listeners to determine the min and max of the range
                if (onReceiveMinRange) onReceiveMinRange.started.AddListener(SetMinRange);
                if (onReceiveMaxRange) onReceiveMaxRange.started.AddListener(SetMaxRange);
                onEnableDrawingColors.started.Invoke(true);
                this.enabled = true;
            }

            if (onEnableDrawingColors)
            {
                onEnableDrawingColors.started.AddListener(EnableDrawingColors);
            }

            if (onClearData)
            {
                onClearData.started.AddListener(ClearData);
            }
        }

        private void EnableDrawingColors(bool enable)
        {
            this.enabled = enable;
        }

        private void ClearData()
        {
            idColors.Clear();
        }

        private void OnEnable()
        {
            if (onReceiveIdsAndColors || onReceiveIdsAndFloats)
            {
                //Colors are updated via event
                return;
            }

            if (idColors == null)
            {
                StartCoroutine(LoadCSV());
            }
            else
            {
                UpdateColors();
            }
        }

        public void SetMinRange(float value)
        {
            minimumValue = value;
        }
        public void SetMaxRange(float value)
        {
            maximumValue = value;
        }

        public void SetIDsAndColors(object idsAndColors)
        {
            this.enabled = true;
            idColors = (Dictionary<string, Color>)idsAndColors;
            UpdateColors(true);
        }
        public void SetIDsAndFloatsAsColors(object idsAndFloats)
        {
            this.enabled = true;
            var idFloats = (Dictionary<string, float>)idsAndFloats;

            idColors = new Dictionary<string, Color>();
            foreach (var keyValuePair in idFloats)
            {
                Color colorFromGradient = gradientContainer.gradient.Evaluate(Mathf.InverseLerp((float)minimumValue, (float)maximumValue, keyValuePair.Value));
                idColors.Add(keyValuePair.Key, colorFromGradient);
            }

            UpdateColors(true);
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

                        if (idColors.ContainsKey(id))
                        {
                            Debug.Log($"Duplicate key found in dataset:{id}. Skipping.");
                        }
                        else
                        {
                            idColors.Add(id, color);
                        }
                    }
                }
                UpdateColors(true);
            }
        }

        private void ParseColor(string colorInput, out Color color)
        {
            color = Color.white;
            switch (colorInterpretation)
            {
                case ColorInterpretation.HEX:
                    ColorUtility.TryParseHtmlString(colorInput, out color);
                    break;
                case ColorInterpretation.INTERPOLATE:
                    if (float.TryParse(colorInput, out float parsed))
                    {
                        color = gradientContainer.gradient.Evaluate(Mathf.InverseLerp((float)minimumValue, (float)maximumValue, parsed));
                    }
                    else
                    {
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
            UpdateColors(false);
        }

        private void UpdateColors(bool applyToExistingSubObjects = false)
        {
            if (idColors == null) return;

            foreach (Transform child in transform)
            {
                SubObjects subObjects = child.gameObject.GetComponent<SubObjects>();
                if (!subObjects)
                {
                    if (child.gameObject.GetComponent<MeshFilter>())
                    {
                        subObjects = child.gameObject.AddComponent<SubObjects>();
                        subObjects.ColorObjectsByID(idColors, defaultColor);
                    }
                }
                else if (applyToExistingSubObjects)
                {
                    subObjects.ColorObjectsByID(idColors, defaultColor);
                }
            }
        }
    }
}