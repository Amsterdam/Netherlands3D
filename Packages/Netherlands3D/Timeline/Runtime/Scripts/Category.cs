using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SLIDDES.UI;

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
        private EventLayer eventLayer;
        /// <summary>
        /// Is the category visible
        /// </summary>
        private bool isVisible = true;
        /// <summary>
        /// The rect transform attached to this category
        /// </summary>
        private RectTransform rectTransform;

        public void Initialize(string name, EventLayer eventLayer)
        {
            this.name = name;
            nameField.text = name;
            this.eventLayer = eventLayer;
            rectTransform = GetComponent<RectTransform>();
        }

        public void ToggleVisability()
        {
            isVisible = !isVisible;
            print("t");
            if(isVisible)
            {
                rectTransform.SetHeight(96);
                eventLayer.rectTransform.SetHeight(96);
            }
            else
            {
                rectTransform.SetHeight(46);
                eventLayer.rectTransform.SetHeight(46);
            }
        }
    }
}
