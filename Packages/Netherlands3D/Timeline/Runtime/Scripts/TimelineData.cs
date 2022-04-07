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
        [Tooltip("The time periods of this timeline")]
        public List<TimePeriod> timePeriods;

        /// <summary>
        /// The sorted data. <categoryname, list<EventsBelongingToCategory>>
        /// </summary>
        public Dictionary<string, List<TimePeriod>> sortedTimePeriods = new Dictionary<string, List<TimePeriod>>();

        /// <summary>
        /// Orders the time periods on its categories
        /// </summary>
        public void OrderTimePeriods()
        {
            // Reset values
            sortedTimePeriods.Clear();

            // Order all time periods
            foreach(TimePeriod item in timePeriods)
            {
                // Check if event category is already present
                if(sortedTimePeriods.ContainsKey(item.category))
                {
                    // Add to existing
                    sortedTimePeriods[item.category].Add(item);
                }
                else
                {
                    // Add to new
                    sortedTimePeriods.Add(item.category, new List<TimePeriod>() { item });
                }
            }
        }
    }
}
