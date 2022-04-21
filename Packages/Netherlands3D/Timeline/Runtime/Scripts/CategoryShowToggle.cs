using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// For showing/hiding the category menu
    /// </summary>
    public class CategoryShowToggle : MonoBehaviour
    {
        [SerializeField] RectTransform categoryMenuRectTransform;
        [SerializeField] RectTransform iconRectTransform;

        private bool isShowing = true;

        public void ToggleShow()
        {
            isShowing = !isShowing;

            if(isShowing)
            {
                iconRectTransform.localEulerAngles = new Vector3(0, 0, -90);
            }
            else
            {
                iconRectTransform.localEulerAngles = new Vector3(0, 0, 90);
            }
            categoryMenuRectTransform.anchoredPosition = new Vector2(categoryMenuRectTransform.anchoredPosition.x * -1, categoryMenuRectTransform.anchoredPosition.y);
        }
    }
}
