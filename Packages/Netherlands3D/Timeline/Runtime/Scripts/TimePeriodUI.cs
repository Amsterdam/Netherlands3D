using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLIDDES.UI;
using TMPro;

namespace Netherlands3D.Timeline
{
    public class TimePeriodUI : MonoBehaviour
    {
        /// <summary>
        /// The event data for this time period UI
        /// </summary>
        public TimePeriod dEvent;
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
        public TimePeriodsLayer eventLayer;

        /// <summary>
        /// Initialize the UI
        /// </summary>
        /// <param name="dEvent"></param>
        public void Initialize(TimePeriod dEvent, TimePeriodsLayer eventLayer)
        {
            this.dEvent = dEvent;
            this.eventLayer = eventLayer;            
            nameField.text = dEvent.name;
        }

        /// <summary>
        /// Invokes the event from this UI
        /// </summary>
        public void InvokeEvent()
        {
            if(dEvent == null) return;
            dEvent.Invoke();
        }

        /// <summary>
        /// Remove the time period UI
        /// </summary>
        public void Remove()
        {
            eventLayer.timePeriodsUI.Remove(this);
            Destroy(gameObject);
        }

        /// <summary>
        /// Update the UI elementss from this prefab
        /// </summary>
        /// <param name="posXLeft">The rt left position</param>
        /// <param name="posXRight">The rt right position</param>
        public void UpdateUI(float posXLeft, float posXRight)
        {
            rectTransform.SetRect(0, 0, posXLeft, posXRight);
        }
    }
}
