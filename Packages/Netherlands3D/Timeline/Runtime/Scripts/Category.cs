using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SLIDDES.UI;
using UnityEngine.UI;

namespace Netherlands3D.Timeline
{

    /// <summary>
    /// A category of a event
    /// </summary>
    public class Category : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI nameField;

        /// <summary>
        /// Name of the category
        /// </summary>
        private new string name;
        /// <summary>
        /// The linked event layer
        /// </summary>
        private TimePeriodsLayer eventLayer;
        /// <summary>
        /// The time line component of this category
        /// </summary>
        private TimelineUI timelineUI;
        /// <summary>
        /// Is the category visible
        /// </summary>
        private bool isVisible = true;
        /// <summary>
        /// The rect transform attached to this category
        /// </summary>
        private RectTransform rectTransform;

        public void Initialize(string name, TimePeriodsLayer eventLayer, TimelineUI timelineUI)
        {
            this.name = name;
            nameField.text = name;
            this.eventLayer = eventLayer;
            this.timelineUI = timelineUI;
            rectTransform = GetComponent<RectTransform>();
        }

        public void ToggleVisability()
        {
            isVisible = !isVisible;
            if(isVisible)
            {
                eventLayer.canvasGroup.alpha = 1;
            }
            else
            {
                eventLayer.canvasGroup.alpha = 0;
            }
            timelineUI.onCategoryToggle.Invoke(name, isVisible);
        }
    }
}
