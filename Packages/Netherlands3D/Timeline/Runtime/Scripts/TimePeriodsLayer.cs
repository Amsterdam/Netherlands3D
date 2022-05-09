using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// Layer for time periods
    /// </summary>
    public class TimePeriodsLayer : MonoBehaviour
    {
        [HideInInspector] public RectTransform rectTransform;
        [HideInInspector] public CanvasGroup canvasGroup;
        /// <summary>
        /// List of all the time period ui's contained in this layer
        /// </summary>
        public List<TimePeriodUI> timePeriodsUI = new List<TimePeriodUI>();

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Add (and create) a time period to the layer
        /// </summary>
        /// <param name="timePeriod"></param>
        /// <param name="prefabTimePeriodUI"></param>
        public TimePeriodUI AddTimePeriod(TimePeriod timePeriod, GameObject prefabTimePeriodUI)
        {
            TimePeriodUI a = Instantiate(prefabTimePeriodUI, transform).GetComponent<TimePeriodUI>();
            a.Initialize(timePeriod, this);
            timePeriodsUI.Add(a);
            timePeriod.eventScreenEnter?.Invoke();
            return a;
        }
    }
}
