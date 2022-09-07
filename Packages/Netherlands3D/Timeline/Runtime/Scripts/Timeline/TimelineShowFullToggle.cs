using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline
{
    public class TimelineShowFullToggle : MonoBehaviour
    {
        [SerializeField] RectTransform rectTransform;
        [SerializeField] RectTransform iconRectTransform;

        private bool isShowing = true;

        public void ToggleShow()
        {
            isShowing = !isShowing;

            if(isShowing)
            {
                iconRectTransform.localEulerAngles = new Vector3(0, 0, 0);
                rectTransform.anchoredPosition = new Vector3(0, rectTransform.rect.height / 2f, 0);
            }
            else
            {
                iconRectTransform.localEulerAngles = new Vector3(0, 0, 180);
                rectTransform.anchoredPosition = new Vector3(0, -1 * (rectTransform.rect.height / 2f) + 96, 0);
            }
        }
    }
}
