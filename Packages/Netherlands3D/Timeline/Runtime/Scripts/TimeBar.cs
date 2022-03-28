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
        private static float pixelDistanceDates = 100;

        [Header("Components")]
        [SerializeField] private Transform parentDates;
        [SerializeField] private GameObject prefabTimeBarDate;

        /// <summary>
        /// The rect transform of the time bar
        /// </summary>
        [HideInInspector] public RectTransform rectTransform;
        /// <summary>
        /// The most left dateTime of this timebar
        /// </summary>
        public DateTime startDateTime { get; private set; }

        /// <summary>
        /// The pixel position and the corresponding dateTime
        /// </summary>
        private Dictionary<float, DateTime> dateTimePositions = new Dictionary<float, DateTime>();

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
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
        /// <returns>local position x, 0 if the date is not in this timebar</returns>
        public float GetDatePosition(DateTime dateTime)
        {
            return dateTimePositions.FirstOrDefault(x => x.Value == dateTime).Key * -1; // have to invert number positivity
        }

        /// <summary>
        /// Update the timebars visuals that are displaying time
        /// </summary>
        /// <param name="dateTimeLeaderIndex">The dateTime of the leader</param>
        /// <param name="barIndex">The index of the bar (based from its position 0-1-2)</param>
        /// <param name="timeUnit">The unit of time used on the bar. 0 = yyyy, 1 = MM/yyyy, 2 = dd/MM/yyyy</param>
        public void UpdateVisuals(DateTime dateTimeLeaderIndex, int barIndex, int timeUnit)
        {
            // Clear old
            foreach(Transform child in parentDates.transform)
            {
                Destroy(child.gameObject);
            }
            dateTimePositions.Clear();

            // Calculate space
            float width = rectTransform.rect.width;
            int datesToPlace = (int)(width / pixelDistanceDates);
            float spaceBetween = width / datesToPlace;
            string format;

            // Calc bar starting date, and based on timeUnit
            switch(timeUnit)
            {
                default: // yyyy
                    format = "yyyy";
                    switch(barIndex)
                    {
                        default: // 0 (left bar)
                            dateTimeLeaderIndex = dateTimeLeaderIndex.AddYears(-datesToPlace);
                            break;
                        case 1: // 1 (middle bar)
                            break;
                        case 2: // 2 (right bar)
                            dateTimeLeaderIndex = dateTimeLeaderIndex.AddYears(datesToPlace);
                            break;
                    }
                    break;
                case 1: // MM/yyyy
                    format = "MM";
                    switch(barIndex)
                    {
                        default: // 0 (left bar)
                            dateTimeLeaderIndex = dateTimeLeaderIndex.AddMonths(-datesToPlace);
                            break;
                        case 1: // 1 (middle bar)
                            break;
                        case 2: // 2 (right bar)
                            dateTimeLeaderIndex = dateTimeLeaderIndex.AddMonths(datesToPlace);
                            break;
                    }
                    break;
                case 2: // dd/MM/yyyy
                    format = "dd/MM";
                    switch(barIndex)
                    {
                        default: // 0 (left bar)
                            dateTimeLeaderIndex = dateTimeLeaderIndex.AddDays(-datesToPlace);
                            break;
                        case 1: // 1 (middle bar)
                            break;
                        case 2: // 2 (right bar)
                            dateTimeLeaderIndex = dateTimeLeaderIndex.AddDays(datesToPlace);
                            break;
                    }
                    break;
            }
            startDateTime = dateTimeLeaderIndex;

            // Space dates evenly
            for(int i = 0; i < datesToPlace; i++)
            {
                TimeBarDate a = Instantiate(prefabTimeBarDate, parentDates).GetComponent<TimeBarDate>();
                float posX = -(width / 2) + (spaceBetween * i) + spaceBetween * 0.5f;
                var dateTime = timeUnit switch
                {
                    1 => dateTimeLeaderIndex.AddMonths(i),
                    2 => dateTimeLeaderIndex.AddDays(i),
                    _ => dateTimeLeaderIndex.AddYears(i)

                };
                a.transform.localPosition = new Vector3(posX, a.transform.localPosition.y, 0);
                a.field.text = dateTime.ToString(format);
                dateTimePositions.Add(posX, dateTime);
            }
        }
    }
}
