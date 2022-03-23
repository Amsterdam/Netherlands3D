using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLIDDES.UI;

namespace Netherlands3D.Timeline
{
    public class TimelineUI : MonoBehaviour
    {
        public float TimeBarParentWidth { get { return timeBarParent.rect.width; } }

        public TimelineData data;

        public float v = 1;
        [Header("UI Components")]
        public RectTransform timeBarParent;
        public RectTransform[] timeBars = new RectTransform[3];

        /// <summary>
        /// Array int holding the order of the indexes of timeBars in which they appear/move
        /// </summary>
        private int[] barIndexes;

        // Start is called before the first frame update
        void Start()
        {
            SetupTimeBars();
        }

        // Update is called once per frame
        void Update()
        {
            //ScrollTimeBar(v * Time.deltaTime);
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
                timeBars[leaderIndex].localPosition += Vector3.right * scrollAmount;

                // If first in line is out of bounds set new first in line
                if(timeBars[leaderIndex].localPosition.x <= timeBarParent.localPosition.x + TimeBarParentWidth * -1)
                {
                    // Circular rotate array left
                    ShiftArray.Rotate(barIndexes, -1);
                    leaderIndex = barIndexes[0];
                }

                // Update remaining indexes positions
                for(int i = 1; i < barIndexes.Length; i++)
                {
                    int index = barIndexes[i];                    
                    timeBars[index].localPosition = new Vector3(timeBars[leaderIndex].localPosition.x + i * TimeBarParentWidth, 0, 0);
                }
            }
            else
            {
                // Scrolling right
                int leaderIndex = barIndexes[barIndexes.Length - 1];

                // Update the leader position
                timeBars[leaderIndex].localPosition += Vector3.right * scrollAmount;

                // If last in line is out of bounds set new last in line
                if(timeBars[leaderIndex].localPosition.x >= timeBarParent.localPosition.x + TimeBarParentWidth)
                {
                    // Circular rotate array right
                    ShiftArray.Rotate(barIndexes, 1);
                    leaderIndex = barIndexes[barIndexes.Length - 1];
                }

                // Update remaining indexes positions
                for(int i = 0; i < barIndexes.Length - 1; i++)
                {
                    int index = barIndexes[i];
                    timeBars[index].localPosition = new Vector3(timeBars[leaderIndex].localPosition.x - (2 - i) * TimeBarParentWidth, 0, 0);
                }
            }            
        }

        private void SetupTimeBars()
        {
            // Position the time bars correctly
            // Hardcoded 3 time bars, as there is no need for more than 3
            timeBars[0].SetRect(0, 0, -timeBarParent.rect.width, timeBarParent.rect.width);
            timeBars[1].SetRect(0, 0, 0, 0);
            timeBars[2].SetRect(0, 0, timeBarParent.rect.width, -timeBarParent.rect.width);
            barIndexes = new int[] { 0, 1, 2 };
        }
    }
}
