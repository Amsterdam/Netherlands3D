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
        /// The time period data for this time period UI
        /// </summary>
        public TimePeriod timePeriod;
        /// <summary>
        /// The rect transform component of the UI
        /// </summary>
        public RectTransform rectTransform;
        /// <summary>
        /// The name field of the UI
        /// </summary>
        public TextMeshProUGUI nameField;
        /// <summary>
        /// The time period layer of the event
        /// </summary>
        public TimePeriodsLayer timePeriodsLayer;

        /// <summary>
        /// Initialize the UI
        /// </summary>
        /// <param name="timePeriod"></param>
        public void Initialize(TimePeriod timePeriod, TimePeriodsLayer timePeriodsLayer)
        {
            this.timePeriod = timePeriod;
            this.timePeriodsLayer = timePeriodsLayer;
            nameField.text = timePeriod.name;
        }

        /// <summary>
        /// When the button of this UI is pressed
        /// </summary>
        public void Pressed()
        {
            if(timePeriod == null) return;
            timePeriod.eventPressed.Invoke();
        }

        /// <summary>
        /// Remove the time period UI
        /// </summary>
        public void Remove()
        {
            timePeriod.eventScreenExit.Invoke();
            timePeriodsLayer.timePeriodsUI.Remove(this);
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
