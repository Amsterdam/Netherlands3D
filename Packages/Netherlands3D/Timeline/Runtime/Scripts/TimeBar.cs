using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// Class attached to a time bar to handle its displaying of time
    /// </summary>
    public class TimeBar : MonoBehaviour
    {
        /// <summary>
        /// The minimum distance in pixels the time fields have to be apart from eachother
        /// </summary>
        public static readonly float PixelDistanceDates = 100;

        /// <summary>
        /// The most left dateTime of this timebar
        /// </summary>
        public DateTime StartDateTime { get; private set; }

        [Header("Components")]
        [Tooltip("The parent of the date prefabs")]
        [SerializeField] private Transform parentDates;
        [Tooltip("The timebarDate prefab")]
        [SerializeField] private GameObject prefabTimeBarDate;

        /// <summary>
        /// The rect transform of the time bar
        /// </summary>
        [HideInInspector] public RectTransform rectTransform;

        /// <summary>
        /// The pixel position and the corresponding dateTime
        /// </summary>
        private Dictionary<float, DateTime> dateTimePositions = new Dictionary<float, DateTime>();

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Get the current selected dateTime from the time bar based on its local x position
        /// </summary>
        /// <returns></returns>
        public DateTime GetCurrentDateTime()
        {
            float posX = transform.localPosition.x * -1;
            // Get the closest dictionary value to posX
            var bestMatch = dateTimePositions.OrderBy(x => Math.Abs(x.Key - posX)).FirstOrDefault();
            return bestMatch.Value;
        }

        /// <summary>
        /// Get the x position of a date in this time bar if it is available
        /// </summary>
        /// <param name="dateTime">The date time to get</param>
        /// <param name="timeUnit">The time unit used 0 = yyyy, 1 = yyyy/MM, 2 = yyyy/MM/dd</param>
        /// <returns>local position x, 0.123f if the date is not in this timebar</returns>
        public float GetDatePosition(DateTime dateTime, TimeUnit.Unit timeUnit)
        {
            // Get the date closest to the dateTime to fetch
            var k = ArrayExtention.MinBy(dateTimePositions, x => Math.Abs((x.Value - dateTime).Ticks));
            if(k.Value == null || 
                timeUnit == TimeUnit.Unit.year && k.Value.Year != dateTime.Year ||
                timeUnit == TimeUnit.Unit.month && k.Value.Year != dateTime.Year ||
                timeUnit == TimeUnit.Unit.month && k.Value.Year == dateTime.Year && k.Value.Month != dateTime.Month ||
                timeUnit == TimeUnit.Unit.day && k.Value.Year != dateTime.Year ||
                timeUnit == TimeUnit.Unit.day && k.Value.Year == dateTime.Year && k.Value.Month != dateTime.Month ||
                timeUnit == TimeUnit.Unit.day && k.Value.Year == dateTime.Year && k.Value.Month == dateTime.Month && k.Value.Day != dateTime.Day)
            {
                return 0.123f;
            }
            //Debug.Log(timeUnit == 1 && k.Value.Year == dateTime.Year && k.Value.Month != dateTime.Month);
            //Debug.Log("Datepos: " + dateTime + " - " + k.Value);
            return k.Key * -1; // have to invert number positivity
        }

        /// <summary>
        /// Update the timebars visuals that are displaying time
        /// </summary>
        /// <param name="dateTimeLeaderIndex">The dateTime of the leader</param>
        /// <param name="barIndex">The index of the bar (based from its position 0-1-2)</param>
        /// <param name="timeUnit">The unit of time used on the bar. 0 = yyyy, 1 = MM/yyyy, 2 = dd/MM/yyyy</param>
        public void UpdateVisuals(DateTime dateTimeLeaderIndex, int barIndex, TimeUnit.Unit timeUnit)
        {
            // Clear old
            foreach(Transform child in parentDates.transform)
            {
                Destroy(child.gameObject);
            }
            dateTimePositions.Clear();

            // Calculate space
            float width = rectTransform.rect.width;
            int datesToPlace = (int)(width / PixelDistanceDates);
            float spaceBetween = width / datesToPlace;

            // Calc bar starting date, and based on timeUnit
            //dateTimeLeaderIndex = timeUnit switch
            //{
            //    TimeUnit.Unit.year => barIndex switch
            //    {
            //        2 => dateTimeLeaderIndex.AddYears(datesToPlace),
            //        1 => dateTimeLeaderIndex,
            //        _ => dateTimeLeaderIndex.AddYears(-datesToPlace)
            //    },
            //    TimeUnit.Unit.month => barIndex switch
            //    {
            //        2 => dateTimeLeaderIndex.AddDays(datesToPlace),
            //        1 => dateTimeLeaderIndex,
            //        _ => dateTimeLeaderIndex.AddDays(-datesToPlace)
            //    },
            //    TimeUnit.Unit.day => barIndex switch
            //    {
            //        2 => dateTimeLeaderIndex.AddMonths(datesToPlace),
            //        1 => dateTimeLeaderIndex,
            //        _ => dateTimeLeaderIndex.AddMonths(-datesToPlace)
            //    },
            //    _ => barIndex switch { _ => null }
            //};            
            dateTimeLeaderIndex = TimeUnit.GetBarStartingDate(dateTimeLeaderIndex, timeUnit, barIndex, datesToPlace);
            StartDateTime = dateTimeLeaderIndex;

            // Space dates evenly
            for(int i = 0; i < datesToPlace; i++)
            {
                TimeBarDate a = Instantiate(prefabTimeBarDate, parentDates).GetComponent<TimeBarDate>();
                float posX = -(width / 2) + (spaceBetween * i) + spaceBetween * 0.5f;
                var dateTime = timeUnit switch
                {
                    TimeUnit.Unit.month => dateTimeLeaderIndex.AddMonths(i),
                    TimeUnit.Unit.day => dateTimeLeaderIndex.AddDays(i),
                    TimeUnit.Unit.year => dateTimeLeaderIndex.AddYears(i),
                    _ => dateTimeLeaderIndex

                };
                a.transform.localPosition = new Vector3(posX, a.transform.localPosition.y, 0);
                a.field.text = dateTime.ToString(TimeUnit.GetUnitString(timeUnit));
                dateTimePositions.Add(posX, dateTime);
            }
        }
    }
}
