using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// A timeline time period that holds data of something that happend between x - x
    /// </summary>
    [CreateAssetMenu(fileName = "Timeline Time Period", menuName = "ScriptableObjects/Timeline/Time Period")]
    public class TimePeriod : ScriptableObject
    {
        [Tooltip("The event name")]
        public new string name;
        [Tooltip("The description of the event")]
        [TextArea(1, 10)]
        public string description;
        [Tooltip("The start date of the event")]
        public DateTimeSerializable startDate;
        [Tooltip("The end date of the event")]
        public DateTimeSerializable endDate;
        [Tooltip("The category of the event")]
        public string category;
        [Space]
        [Tooltip("The unity event that gets invoked when the event gets invoked")]
        public UnityEvent unityEvent;

        /// <summary>
        /// Invoke the event (for triggering a unity event)
        /// </summary>
        public void Invoke()
        {
            unityEvent.Invoke();
        }
    }
}
