using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// For showing/hiding the layers menu
    /// </summary>
    public class LayersShowToggle : MonoBehaviour
    {
        [SerializeField] private RectTransform categoryMenuRectTransform;
        [SerializeField] private RectTransform iconRectTransform;
        [SerializeField] private CanvasGroup canvasGroupLayers;

        private bool isShowing = true;

        private Vector2 defaultOpenSize;
        private void Awake()
        {
            defaultOpenSize = categoryMenuRectTransform.sizeDelta;
        }

        public void ToggleShow()
        {
            isShowing = !isShowing;

            if(isShowing)
            {
                iconRectTransform.localEulerAngles = new Vector3(0, 0, -90);
                canvasGroupLayers.alpha = 1;
            }
            else
            {
                canvasGroupLayers.alpha = 0;
                iconRectTransform.localEulerAngles = new Vector3(0, 0, 90);
            }
            categoryMenuRectTransform.sizeDelta = (isShowing) ? defaultOpenSize : Vector2.zero;
        }
    }
}
