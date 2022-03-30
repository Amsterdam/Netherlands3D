using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SLIDDES.UI;
using TMPro;
using System.Globalization;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// Main script for time line UI
    /// </summary>
    public class TimelineUI : MonoBehaviour
    {
        /// <summary>
        /// The width of the parent rect
        /// </summary>
        public float TimeBarParentWidth { get { return timeBarParent.rect.width; } }

        [Tooltip("The scriptable so data")]
        public TimelineData data;

        [Header("UI Components")]
        public RectTransform timeBarParent;
        [Tooltip("The 3 timebar script components")]
        public TimeBar[] timeBars = new TimeBar[3];
        [Tooltip("The input field of the current date")]
        public TMP_InputField inputFieldCurrentDate;

        [Header("Timeline Components")]
        [SerializeField] private GameObject prefabEventLayer;
        [SerializeField] private GameObject prefabEventUI;
        [SerializeField] private GameObject prefabCategory;
        [SerializeField] private Transform parentEventLayers;
        [SerializeField] private Transform parentCategories;

        /// <summary>
        /// The time unit used for the timeline. 0 = yyyy, 1 = mm/yyyy, 2 = dd/mm/yyyy
        /// </summary>
        private int timeUnit = 0;
        /// <summary>
        /// Array int holding the order of the indexes of timeBars in which they appear/move
        /// </summary>
        private int[] barIndexes;
        /// <summary>
        /// The current time line date
        /// </summary>
        private DateTime currentDate;
        /// <summary>
        /// The most visable date left
        /// </summary>
        private DateTime visableDateLeft;
        /// <summary>
        /// The most visable date right
        /// </summary>
        private DateTime visableDateRight;
        /// <summary>
        /// List of all categories scripts
        /// </summary>
        private List<Category> categories = new List<Category>();
        /// <summary>
        /// String of each event layer with as key category name
        /// </summary>
        private Dictionary<string, EventLayer> eventLayers = new Dictionary<string, EventLayer>();

        // Start is called before the first frame update
        void Start()
        {            
            SetCurrentDate(DateTime.Now);
            LoadData();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        /// <summary>
        /// Load the TimelineData in the UI
        /// </summary>
        public void LoadData()
        {
            if(data == null)
            {
                Debug.LogError("[TimelineUI] Data not assigned");
                return;
            }

            // Clear old
            foreach(Transform item in parentEventLayers.transform) Destroy(item.gameObject);
            foreach(Transform item in parentCategories.transform) Destroy(item.gameObject);
            categories.Clear();
            eventLayers.Clear();

            // Order all events based on category
            data.OrderEvents();

            // Create each category & event layer
            string[] keys = data.data.Keys.ToArray();
            foreach(string item in keys)
            {
                Category c = Instantiate(prefabCategory, parentCategories).GetComponent<Category>();
                c.Initialize(item);
                categories.Add(c);
                EventLayer e = Instantiate(prefabEventLayer, parentEventLayers).GetComponent<EventLayer>();
                eventLayers.Add(item, e);
            }
        }

        /// <summary>
        /// When the input field receives a new date
        /// </summary>
        public void OnInputFieldCurrentDateChanged()
        {
            // Try to parse the new date
            DateTime result;
            if(DateTime.TryParseExact(inputFieldCurrentDate.text, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                timeUnit = 0;
                SetCurrentDate(result);
            }
            else if(DateTime.TryParseExact(inputFieldCurrentDate.text, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                timeUnit = 1;
                SetCurrentDate(result);
            }
            else if(DateTime.TryParseExact(inputFieldCurrentDate.text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                timeUnit = 2;
                SetCurrentDate(result);
            }
            else
            {
                UpdateCurrentDateVisual();
            }
        }

        /// <summary>
        /// Scroll the time bar horizontally by a amount
        /// </summary>
        /// <param name="scrollAmount"></param>
        public void ScrollTimeBar(float scrollAmount)
        {
            // Check scroll direction
            if(scrollAmount < 0)
            {
                // Scrolling left
                int leaderIndex = barIndexes[0];

                // Update the first in line panel
                timeBars[leaderIndex].rectTransform.localPosition += Vector3.right * scrollAmount;

                // If first in line is out of bounds set new first in line
                if(timeBars[leaderIndex].rectTransform.localPosition.x <= timeBarParent.localPosition.x + TimeBarParentWidth * -1)
                {
                    // Circular rotate array left
                    ArrayExtention.Rotate(barIndexes, -1);
                    leaderIndex = barIndexes[0];
                    // Update visuals of previous leader bar
                    timeBars[barIndexes.Last()].UpdateVisuals(timeBars[barIndexes[1]].startDateTime, 2, timeUnit);
                }

                // Update remaining indexes positions
                for(int i = 1; i < barIndexes.Length; i++)
                {
                    int index = barIndexes[i];                    
                    timeBars[index].rectTransform.localPosition = new Vector3(timeBars[leaderIndex].rectTransform.localPosition.x + i * TimeBarParentWidth, 0, 0);
                }
            }
            else
            {
                // Scrolling right
                int leaderIndex = barIndexes[barIndexes.Length - 1];

                // Update the leader position
                timeBars[leaderIndex].rectTransform.localPosition += Vector3.right * scrollAmount;

                // If last in line is out of bounds set new last in line
                if(timeBars[leaderIndex].rectTransform.localPosition.x >= timeBarParent.localPosition.x + TimeBarParentWidth)
                {
                    // Circular rotate array right
                    ArrayExtention.Rotate(barIndexes, 1);
                    leaderIndex = barIndexes[barIndexes.Length - 1];
                    // Update visuals of previous leader bar
                    timeBars[barIndexes.First()].UpdateVisuals(timeBars[barIndexes[1]].startDateTime, 0, timeUnit);
                }

                // Update remaining indexes positions
                for(int i = 0; i < barIndexes.Length - 1; i++)
                {
                    int index = barIndexes[i];
                    timeBars[index].rectTransform.localPosition = new Vector3(timeBars[leaderIndex].rectTransform.localPosition.x - (2 - i) * TimeBarParentWidth, 0, 0);
                }
            }

            // Get currentDate from middle bar
            currentDate = GetFocusedBar().GetCurrentDateTime();
            UpdateCurrentDateVisual();
            UpdateVisableDateRange();
        }

        /// <summary>
        /// Set the current date of the time line and go to that date
        /// </summary>
        public void SetCurrentDate(DateTime newDate)
        {
            currentDate = newDate;

            UpdateTimeBars();
            SetFocusedBarPosition(GetFocusedBar().GetDatePosition(currentDate));
            UpdateCurrentDateVisual();
            UpdateVisableDateRange();
        }

        /// <summary>
        /// Set the focused bar local position.
        /// </summary>
        /// <example>
        /// When setting the currentDate the bar will not center itself on the set date, thats where this funtion comes in place to move the local x positions
        /// </example>
        /// <param name="localPosX">The local x position to set the bar to</param>
        private void SetFocusedBarPosition(float localPosX)
        {
            // Middle bar
            TimeBar t1 = timeBars[barIndexes[1]]; 
            t1.transform.localPosition = new Vector3(localPosX, t1.transform.localPosition.y, t1.transform.localPosition.z);
            // Left bar
            TimeBar t0 = timeBars[barIndexes[0]];
            t0.transform.localPosition = new Vector3(localPosX - TimeBarParentWidth, t0.transform.localPosition.y, t0.transform.localPosition.z);
            // Right bar
            TimeBar t2 = timeBars[barIndexes[2]];
            t2.transform.localPosition = new Vector3(localPosX + TimeBarParentWidth, t2.transform.localPosition.y, t2.transform.localPosition.z);
        }

        /// <summary>
        /// Set the timeline currentDateTimeUnit
        /// </summary>
        /// <param name="valueToAdd">The value to add to currentDateTimeUnit</param>
        public void SetTimeUnit(int valueToAdd)
        {
            if(timeUnit + valueToAdd < 0 || timeUnit + valueToAdd > 2) return;
            timeUnit = Mathf.Clamp(timeUnit + valueToAdd, 0, 2);
            SetCurrentDate(currentDate);
            UpdateCurrentDateVisual();
        }

        /// <summary>
        /// Get the focused bar
        /// </summary>
        /// <returns></returns>
        private TimeBar GetFocusedBar()
        {
            // Based on time bar which is closest to position.x 0
            return timeBars.OrderBy(x => Math.Abs(0 - x.transform.localPosition.x)).FirstOrDefault();
        }

        /// <summary>
        /// Get the visable date range the user can see
        /// </summary>
        /// <remarks>
        /// We need the visable date range in order to know what loaded data we need to display
        /// </remarks>
        private void UpdateVisableDateRange()
        {
            int datesToPlace = (int)((TimeBarParentWidth / TimeBar.PixelDistanceDates) / 2) + 2;
            // based on timeUnit & current date, get the most left and right date
            switch(timeUnit) // 0 = yyyy, 1 = mm/yyyy, 2 = dd/mm/yyyy
            {
                default: //0
                    visableDateLeft = currentDate.AddYears(-datesToPlace);
                    visableDateRight = currentDate.AddYears(datesToPlace);
                    break;
                case 1: //1
                    visableDateLeft = currentDate.AddMonths(-datesToPlace);
                    visableDateRight = currentDate.AddMonths(datesToPlace);
                    break;
                case 2: //2
                    visableDateLeft = currentDate.AddDays(-datesToPlace);
                    visableDateRight = currentDate.AddDays(datesToPlace);
                    break;
            }

            // Based on whats visable, show corresponding events
            // Reset values
            foreach(var item in eventLayers.Values)
            {
                foreach(Transform item1 in item.transform)
                {
                    Destroy(item1.gameObject);
                }
                item.events.Clear();
            }

            // For each category, loop trough events to check if the event dates are in the visable range
            foreach(var item in data.data.Keys)
            {
                foreach(var dEvent in data.data[item])
                {
                    // Check if event is in visable range
                    if(//dEvent.startDate <= visableDateLeft && dEvent.endDate >= visableDateRight)// ||     // 0---[-------]---0
                        //dEvent.startDate <= visableDateLeft && dEvent.endDate <= visableDateRight ||    // 0---[---0   ]
                        //dEvent.startDate >= visableDateLeft && dEvent.endDate >= visableDateRight ||    //     [   0---]---0
                        dEvent.startDate >= visableDateLeft && dEvent.endDate <= visableDateRight)      //     [0-----0]                    
                    {
                        // Event is visable, show it & add to event layer
                        float xL = EventUIGetPosX(dEvent.startDate);
                        print("XL " + xL);
                        float xR = EventUIGetPosX(dEvent.endDate);
                        print("XR " + xR);
                        eventLayers[dEvent.category].AddEvent(dEvent, prefabEventUI, xL, xR);
                    }
                }
            }

        }

        /// <summary>
        /// Get the local x position of a date from the timeBars
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>The local x position for the dateTime</returns>
        private float EventUIGetPosX(DateTime dateTime)
        {
            // Loop through timebars to check if the date is available
            for(int i = 0; i < timeBars.Length; i++)
            {
                float value = timeBars[i].GetDatePosition(dateTime);
                if(value == 0.123f) continue;
                else
                {
                    return value;
                }
            }
            return 0;
        }

        /// <summary>
        /// Setup the time bars
        /// </summary>
        private void UpdateTimeBars()
        {
            // Position the time bars correctly
            // Hardcoded 3 time bars, as there is no need for more than 3
            timeBars[0].rectTransform.SetRect(0, 0, -timeBarParent.rect.width, timeBarParent.rect.width);
            timeBars[1].rectTransform.SetRect(0, 0, 0, 0);
            timeBars[2].rectTransform.SetRect(0, 0, timeBarParent.rect.width, -timeBarParent.rect.width);
            barIndexes = new int[] { 0, 1, 2 };

            // Time bar visual
            for(int i = 0; i < 3; i++)
            {
                timeBars[i].UpdateVisuals(currentDate, i, timeUnit);
            }
        }

        /// <summary>
        /// Updates the current date inputfield text to currentDate datetime value
        /// </summary>
        private void UpdateCurrentDateVisual()
        {
            string format = timeUnit switch
            {
                1 => "MM/yyyy",
                2 => "dd/MM/yyyy",
                _ => "yyyy",
            };
            inputFieldCurrentDate.text = currentDate.ToString(format);
        }


    }
}
