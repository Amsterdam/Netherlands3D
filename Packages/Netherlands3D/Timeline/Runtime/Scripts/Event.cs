using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// A timeline event that holds data of something that happend between x - x
    /// </summary>
    [CreateAssetMenu(fileName = "Timeline Event", menuName = "ScriptableObjects/Timeline/Event")]
    public class Event : ScriptableObject
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
    }
}
