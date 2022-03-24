using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SLIDDES.UI;
using TMPro;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// Main script for time line UI
    /// </summary>
    public class TimelineUI : MonoBehaviour
    {
        public float TimeBarParentWidth { get { return timeBarParent.rect.width; } }

        public TimelineData data;

        [Header("UI Components")]
        public RectTransform timeBarParent;
        public TimeBar[] timeBars = new TimeBar[3];
        public TextMeshProUGUI currentDateText;

        /// <summary>
        /// Array int holding the order of the indexes of timeBars in which they appear/move
        /// </summary>
        private int[] barIndexes;

        private DateTime currentDate;

        // Start is called before the first frame update
        void Start()
        {
            SetupCurrentDate();
            SetupTimeBars();
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
                    timeBars[barIndexes.Last()].UpdateVisuals(timeBars[barIndexes[1]].startDateTime, 2);
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
                    timeBars[barIndexes.First()].UpdateVisuals(timeBars[barIndexes[1]].startDateTime, 0);
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
            UpdateCurrentDate();
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

        private void SetupCurrentDate()
        {
            currentDate = DateTime.Now;
            UpdateCurrentDate();
        }

        private void SetupTimeBars()
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
                timeBars[i].UpdateVisuals(currentDate, i);
            }
        }

        private void UpdateCurrentDate()
        {
            // Set text
            currentDateText.text = currentDate.ToString("dd/MM/yyyy");
        }
    }
}
