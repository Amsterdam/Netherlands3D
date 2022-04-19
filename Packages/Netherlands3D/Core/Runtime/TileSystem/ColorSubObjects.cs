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
                onEnableDrawingColors.started.AddListener(EnableDrawingColors);
            }

            if (onReceiveIdsAndColors)
            {
                onReceiveIdsAndColors.started.AddListener(SetIDsAndColors);
                if (onEnableDrawingColors) onEnableDrawingColors.started.Invoke(true);
                this.enabled = true;
            }

            if (onReceiveIdsAndFloats)
            {
                onReceiveIdsAndFloats.started.AddListener(SetIDsAndFloatsAsColors);

                //If we can receive ids+floats, add listeners to determine the min and max of the range
                if (onReceiveMinRange) onReceiveMinRange.started.AddListener(SetMinRange);
                if (onReceiveMaxRange) onReceiveMaxRange.started.AddListener(SetMaxRange);
                if (onEnableDrawingColors) onEnableDrawingColors.started.Invoke(true);
                this.enabled = true;
            }

            if (onClearData)
            {
                onClearData.started.AddListener(ClearData);
            }

            if(onSetGradient)
            {
                onSetGradient.started.AddListener(SwapGradient);
            }
        }

        private void Start()
        {
            if (disableOnStart)
            {
                this.enabled = false;
                if(onEnableDrawingColors) onEnableDrawingColors.started.Invoke(false);
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
            if(idColors != null)
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