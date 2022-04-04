using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLIDDES.UI;
using TMPro;

namespace Netherlands3D.Timeline
{
    public class EventUI : MonoBehaviour
    {
        /// <summary>
        /// The event data for this eventUI
        /// </summary>
        public Event dEvent;
        /// <summary>
        /// The rect transform component of the UI
        /// </summary>
        public RectTransform rectTransform;
        /// <summary>
        /// The name field of the UI
        /// </summary>
        public TextMeshProUGUI nameField;
        /// <summary>
        /// The event layer of the event
        /// </summary>
        public EventLayer eventLayer;

        /// <summary>
        /// Initialize the UI
        /// </summary>
        /// <param name="dEvent"></param>
        public void Initialize(Event dEvent, EventLayer eventLayer)
        {
            this.dEvent = dEvent;
            this.eventLayer = eventLayer;            
            nameField.text = dEvent.name;
        }

        /// <summary>
        /// Remove the EventUI
        /// </summary>
        public void Remove()
        {
            eventLayer.events.Remove(this);
            Destroy(gameObject);
        }

        public void UpdateUI(float posXLeft, float posXRight)
        {
            rectTransform.SetRect(0, 0, posXLeft, posXRight);
        }
    }
}
