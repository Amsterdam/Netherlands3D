using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// Data used for displaying a timeline
    /// </summary>
    [CreateAssetMenu(fileName = "Timeline Data", menuName = "ScriptableObjects/Timeline/Timeline Data")]
    public class TimelineData : ScriptableObject
    {
        [Tooltip("The events of this timeline")]
        public List<Event> events;
    }
}
