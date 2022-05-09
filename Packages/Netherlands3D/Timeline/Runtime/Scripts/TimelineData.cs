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
        [Tooltip("The time periods SO of this timeline")]
        public List<TimePeriodSO> timePeriodsSO;
        [Tooltip("The time periods of this timeline")]
        public List<TimePeriod> timePeriods;

        /// <summary>
        /// The sorted data. <categoryname, list<EventsBelongingToCategory>>
        /// </summary>
        public Dictionary<string, List<TimePeriod>> sortedTimePeriods = new Dictionary<string, List<TimePeriod>>();

        /// <summary>
        /// List that contains all the timePeriods & timePeriodsSO
        /// </summary>
        [HideInInspector] public List<TimePeriod> allTimePeriods = new List<TimePeriod>();

        /// <summary>
        /// Orders the time periods on its categories
        /// </summary>
        public void OrderTimePeriods()
        {
            // Reset values
            sortedTimePeriods.Clear();
            allTimePeriods.Clear();

            // Order all time periods SO
            foreach(TimePeriodSO item in timePeriodsSO)
            {
                // Check if event category is already present
                if(sortedTimePeriods.ContainsKey(item.timePeriod.layer))
                {
                    // Add to existing
                    Debug.Log(1);
                    sortedTimePeriods[item.timePeriod.layer].Add(item.timePeriod);
                }
                else
                {
                    // Add to new
                    Debug.Log(2);
                    sortedTimePeriods.Add(item.timePeriod.layer, new List<TimePeriod>() { item.timePeriod });
                    Debug.Log(sortedTimePeriods[item.timePeriod.layer][0].startDate.Value);
                }
                allTimePeriods.Add(item.timePeriod);
            }

            // Order all time periods
            foreach(TimePeriod item in timePeriods)
            {
                // Check if event category is already present
                if(sortedTimePeriods.ContainsKey(item.layer))
                {
                    Debug.Log(1);
                    // Add to existing
                    sortedTimePeriods[item.layer].Add(item);
                }
                else
                {
                    Debug.Log(2);
                    // Add to new
                    sortedTimePeriods.Add(item.layer, new List<TimePeriod>() { item });
                }
                allTimePeriods.Add(item);
            }

            Debug.Log("[TimelineData] Sorted time periods layer Count: " + sortedTimePeriods.Count);
        }
    }
}
