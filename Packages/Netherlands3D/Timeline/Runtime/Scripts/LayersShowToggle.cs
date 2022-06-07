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
            categoryMenuRectTransform.anchoredPosition = new Vector2(categoryMenuRectTransform.anchoredPosition.x * -1, categoryMenuRectTransform.anchoredPosition.y);
        }
    }
}
