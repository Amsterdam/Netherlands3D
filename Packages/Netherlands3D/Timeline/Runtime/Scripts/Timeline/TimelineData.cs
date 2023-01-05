using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
        [HideInInspector] public UnityEvent OnOrderTimePeriods;

        /// <summary>
        /// Add a timePeriod to the data
        /// </summary>
        /// <param name="timePeriod">The time period to add</param>
        /// <param name="triggerRefresh">If you want to trigger the UnityEvent OnOrderTimePeriods</param>
        public void AddTimePeriod(TimePeriod timePeriod, bool triggerRefresh)
        {
            timePeriods.Add(timePeriod);
            if(triggerRefresh) OrderTimePeriods();
        }

        /// <summary>
        /// Add timePeriods to the data
        /// </summary>
        /// <param name="timePeriods">The time periods to add</param>
        /// <param name="triggerRefresh">If you want to trigger the UnityEvent OnOrderTimePeriods</param>
        public void AddTimePeriod(List<TimePeriod> timePeriods, bool triggerRefresh)
        {
            this.timePeriods.AddRange(timePeriods);
            if(triggerRefresh) OrderTimePeriods();
        }

        /// <summary>
        /// Clear all data
        /// </summary>
        public void ClearData()
        {
            timePeriodsSO.Clear();
            timePeriods.Clear();
            sortedTimePeriods.Clear();
            allTimePeriods.Clear();
            OnOrderTimePeriods.Invoke();
        }

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
                    sortedTimePeriods[item.timePeriod.layer].Add(item.timePeriod);
                }
                else
                {
                    // Add to new
                    sortedTimePeriods.Add(item.timePeriod.layer, new List<TimePeriod>() { item.timePeriod });                    
                }
                allTimePeriods.Add(item.timePeriod);
            }

            // Order all time periods
            foreach(TimePeriod item in timePeriods)
            {
                // Check if event category is already present
                if(sortedTimePeriods.ContainsKey(item.layer))
                {
                    // Add to existing
                    sortedTimePeriods[item.layer].Add(item);
                }
                else
                {
                    // Add to new
                    sortedTimePeriods.Add(item.layer, new List<TimePeriod>() { item });
                }
                allTimePeriods.Add(item);
            }

            //Debug.Log("[TimelineData] Sorted time periods layer Count: " + sortedTimePeriods.Count);
            OnOrderTimePeriods.Invoke();
        }
    }
}
