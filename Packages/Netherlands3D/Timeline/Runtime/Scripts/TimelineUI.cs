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

        // Start is called before the first frame update
        void Start()
        {
            SetCurrentDate(DateTime.Now);
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
                print("not correct");
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
                    ShiftArray.Rotate(barIndexes, -1);
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
                    ShiftArray.Rotate(barIndexes, 1);
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
        }

        /// <summary>
        /// Set the current date of the time line and go to that date
        /// </summary>
        public void SetCurrentDate(DateTime newDate)
        {
            currentDate = newDate;
            UpdateTimeBars();
            SetFocusedBarPosition(GetFocusedBar().GetDatePosition(currentDate));
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
            timeUnit = Mathf.Clamp(timeUnit + valueToAdd, 0, 2);
            UpdateTimeBars();
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


        private void UpdateTimeBars()
        {
            // Position the time bars correctly
            // Hardcoded 3 time bars, as there is no need for more than 3
            timeBars[0].rectTransform.SetRect(0, 0, -timeBarParent.rect.width, timeBarParent.rect.width);
            timeBars[1].rectTransform.SetRect(0, 0, 0, 0);
            timeBars[2].rectTransform.SetRect(0, 0, timeBarParent.rect.width, -timeBarParent.rect.width);
            barIndexes = new int[] { 0, 1, 2 };

            // Time bar visual setup
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
