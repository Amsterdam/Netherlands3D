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

        /// <summary>
        /// The sorted data. <categoryname, list<EventsBelongingToCategory>>
        /// </summary>
        public Dictionary<string, List<Event>> data = new Dictionary<string, List<Event>>();

        /// <summary>
        /// Orders the events on its categories
        /// </summary>
        public void OrderEvents()
        {
            // Reset values
            data.Clear();

            // Order all events
            foreach(Event item in events)
            {
                // Check if event category is already present
                if(data.ContainsKey(item.category))
                {
                    // Add to existing
                    data[item.category].Add(item);
                }
                else
                {
                    // Add to new
                    data.Add(item.category, new List<Event>() { item });
                }
            }
        }
    }
}
