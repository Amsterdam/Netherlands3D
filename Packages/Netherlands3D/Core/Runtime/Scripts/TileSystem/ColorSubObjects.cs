using Netherlands3D.Core;
using Netherlands3D.Core.Colors;
using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.TileSystem
{
    public class ColorSubObjects : MonoBehaviour
    {
        private Dictionary<string, Color> idColors;
        private Dictionary<Vector2Int, Dictionary<string, Color>> tileIdColors = new();

        [SerializeField]
        private bool disableOnStart = false;
        [SerializeField]
        private bool useManualIdColorInput;

        [Header("Listen to")]
        [SerializeField]
        private BoolEvent onEnableDrawingColors;
        [SerializeField]
        private ObjectEvent onReceiveIdsAndColors;
        [SerializeField]
        private ObjectEvent onReceiveIdsAndFloats;
        [SerializeField]
        private ObjectEvent onReceiveIdsAndFloatsForAlpha;
        [SerializeField]
        private ObjectEvent onReceiveTileKeysIdsAndFloats;
        [SerializeField]
        private GradientContainerEvent onSetGradient;

        [SerializeField]
        private FloatEvent onReceiveMinRange;
        [SerializeField]
        private FloatEvent onReceiveMaxRange;

        [SerializeField]
        private TriggerEvent onClearData;

        [Header("Sample float from gradient")]
        [SerializeField]
        private double minimumValue;
        [SerializeField]
        private double maximumValue;
        [SerializeField]
        private GradientContainer gradientContainer;

        [Header("Default")]
        [SerializeField]
        private Color defaultColor;

        private void Awake()
        {
            if (onEnableDrawingColors)
            {
                onEnableDrawingColors.AddListenerStarted(EnableDrawingColors);
            }

            if (onReceiveIdsAndColors)
            {
                onReceiveIdsAndColors.AddListenerStarted(SetIDsAndColors);
                if (onEnableDrawingColors) onEnableDrawingColors.InvokeStarted(true);
                this.enabled = true;
            }

            if (onReceiveIdsAndFloats)
            {
                onReceiveIdsAndFloats.AddListenerStarted(SetIDsAndFloatsAsColors);

                //If we can receive ids+floats, add listeners to determine the min and max of the range
                if (onReceiveMinRange) onReceiveMinRange.AddListenerStarted(SetMinRange);
                if (onReceiveMaxRange) onReceiveMaxRange.AddListenerStarted(SetMaxRange);
                if (onEnableDrawingColors) onEnableDrawingColors.InvokeStarted(true);
                this.enabled = true;
            }


            if (onReceiveTileKeysIdsAndFloats)
            {
                onReceiveTileKeysIdsAndFloats.AddListenerStarted(SetTileKeysIDsAndFloatsAsColors);

                //If we can receive ids+floats, add listeners to determine the min and max of the range
                if (onReceiveMinRange) onReceiveMinRange.AddListenerStarted(SetMinRange);
                if (onReceiveMaxRange) onReceiveMaxRange.AddListenerStarted(SetMaxRange);
                if (onEnableDrawingColors) onEnableDrawingColors.InvokeStarted(true);
                this.enabled = true;
            }

            if (onReceiveIdsAndFloatsForAlpha)
            {
                onReceiveIdsAndFloatsForAlpha.AddListenerStarted(SetIDsAndFloatsAsAlpha);

                //If we can receive ids+floats, add listeners to determine the min and max of the range
                if (onReceiveMinRange) onReceiveMinRange.AddListenerStarted(SetMinRange);
                if (onReceiveMaxRange) onReceiveMaxRange.AddListenerStarted(SetMaxRange);
                if (onEnableDrawingColors) onEnableDrawingColors.InvokeStarted(true);
                this.enabled = true;
            }

            if (onClearData)
            {
                onClearData.AddListenerStarted(ClearData);
            }

            if (onSetGradient)
            {
                onSetGradient.AddListenerStarted(SwapGradient);
            }
        }

        private void Start()
        {
            if (disableOnStart)
            {
                this.enabled = false;
                if (onEnableDrawingColors) onEnableDrawingColors.InvokeStarted(false);
            }
        }

        public void SwapGradient(GradientContainer newGradientContainer)
        {
            gradientContainer = newGradientContainer;
            UpdateColors(true);
        }

        private void EnableDrawingColors(bool enable)
        {
            this.enabled = enable;
        }

        private void ClearData()
        {
            if (idColors != null)
                idColors.Clear();

            ClearAllSubObjectColorData();
        }

        private void OnEnable()
        {
            if (onReceiveIdsAndColors || onReceiveIdsAndFloats)
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

        public void SetTileKeysIDsAndFloatsAsColors(object tileKeysIdsAndColors)
        {
            var tileIdFloats = (Tuple<Vector2Int, Dictionary<string, float>>)tileKeysIdsAndColors;
            var tileKey = tileIdFloats.Item1;
            var idFloats = tileIdFloats.Item2;

            //this.tileIdColors.Add(tileIdColors.Item1, tileIdColors.Item2);
            //idColors = new Dictionary<string, Color>();

            //foreach (var tileDictionary in this.tileIdColors)
            //{
            //    idColors.Union(tileDictionary.Value).ToDictionary(x => x.Key, x => x.Value);
            //}
            var tileIdColors = new Dictionary<string, Color>();
            foreach (var keyValuePair in idFloats)
            {
                Color colorFromGradient = gradientContainer.gradient.Evaluate(Mathf.InverseLerp((float)minimumValue, (float)maximumValue, keyValuePair.Value));
                tileIdColors.Add(keyValuePair.Key, colorFromGradient);
            }
            if (this.tileIdColors.ContainsKey(tileKey))
                this.tileIdColors[tileKey] = tileIdColors;
            else
                this.tileIdColors.Add(tileKey, tileIdColors);

            UpdateColorsByTileKey(tileIdFloats.Item1, tileIdColors);
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

        public void SetIDsAndFloatsAsAlpha(object idsAndFloats)
        {
            this.enabled = true;
            var idFloats = (Dictionary<string, float>)idsAndFloats;
            idColors = new Dictionary<string, Color>();
            foreach (var keyValuePair in idFloats)
            {
                Color color = Color.white;
                color.a = 1 - Mathf.InverseLerp((float)minimumValue, (float)maximumValue, keyValuePair.Value);
                idColors.Add(keyValuePair.Key, color);
            }

            UpdateColors(true);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            ClearAllSubObjectColorData();
        }

        private void ClearAllSubObjectColorData()
        {
            var allSubObjects = gameObject.GetComponentsInChildren<SubObjects>();
            for (int i = allSubObjects.Length - 1; i >= 0; i--)
            {
                allSubObjects[i].ResetColors();
                Destroy(allSubObjects[i]);
            }
        }

        private void OnTransformChildrenChanged()
        {
            if (useManualIdColorInput)
                foreach (var tileKeyDictionaryPair in tileIdColors)
                    UpdateColorsByTileKey(tileKeyDictionaryPair.Key, tileKeyDictionaryPair.Value);
            else
                UpdateColors(false);
        }

        private void UpdateColors(bool applyToExistingSubObjects = false)
        {
            if (useManualIdColorInput || idColors == null) return;

            foreach (Transform child in transform)
            {
                SubObjects subObjects = child.gameObject.GetComponent<SubObjects>();
                if (!subObjects)
                {
                    subObjects = child.gameObject.AddComponent<SubObjects>();
                    subObjects.ColorObjectsByID(idColors, defaultColor);
                }
                else if (applyToExistingSubObjects)
                {
                    subObjects.ColorObjectsByID(idColors, defaultColor);
                }
            }
        }

        private void UpdateColorsByTileKey(Vector2Int tileKey, Dictionary<string, Color> idColorsOfTile)
        {
            if (!useManualIdColorInput)
                Debug.LogWarning("Updating colors by tile key can result in unexpected behaviour if useManualIdColorInput is not enabled");

            if (idColorsOfTile == null) return;

            var subObjects = GetSubObjectsByTileKey(tileKey);
            if (subObjects)
                subObjects.ColorObjectsByID(idColorsOfTile, defaultColor);
        }

        public SubObjects GetSubObjectsByTileKey(Vector2Int tileKey)
        {
            foreach (Transform child in transform)
            {
                SubObjects subObjects = child.gameObject.GetComponent<SubObjects>();
                if (!subObjects)
                {
                    subObjects = child.gameObject.AddComponent<SubObjects>();
                }
                if (subObjects.TileKey == tileKey)
                {
                    return subObjects;
                }
            }
            return null;
        }
    }
}
