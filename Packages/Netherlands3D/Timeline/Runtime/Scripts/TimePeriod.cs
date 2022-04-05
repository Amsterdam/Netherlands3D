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
        [Tooltip("The time period name")]
        public new string name;
        [Tooltip("The description of the time period")]
        [TextArea(1, 10)]
        public string description;
        [Tooltip("The start date of the time period")]
        public DateTimeSerializable startDate;
        [Tooltip("The end date of the time period")]
        public DateTimeSerializable endDate;
        [Tooltip("The category of the time period")]
        public string category;
        [Space]
        [Tooltip("The unity time period that gets invoked when the time period gets invoked")]
        public UnityEvent unityEvent;

        /// <summary>
        /// Invoke the time period unity event
        /// </summary>
        public void Invoke()
        {
            unityEvent.Invoke();
        }
    }
}
