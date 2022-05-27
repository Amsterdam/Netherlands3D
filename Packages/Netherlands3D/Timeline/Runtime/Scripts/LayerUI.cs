using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SLIDDES.UI;
using UnityEngine.UI;

namespace Netherlands3D.Timeline
{

    /// <summary>
    /// A layer UI element of a time period layer
    /// </summary>
    public class LayerUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI nameField;

        /// <summary>
        /// Name of the layer
        /// </summary>
        private new string name;
        /// <summary>
        /// The linked event layer
        /// </summary>
        private TimePeriodsLayer eventLayer;
        /// <summary>
        /// The time line component of this layer
        /// </summary>
        private TimelineUI timelineUI;
        /// <summary>
        /// Is the layer visible
        /// </summary>
        private bool isVisible = true;

        public void Initialize(string name, TimePeriodsLayer eventLayer, TimelineUI timelineUI)
        {
            this.name = name;
            nameField.text = name;
            this.eventLayer = eventLayer;
            this.timelineUI = timelineUI;
        }

        /// <summary>
        /// Toggle the visability of the layer
        /// </summary>
        public void ToggleVisibility()
        {
            if(eventLayer == null) return;

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
