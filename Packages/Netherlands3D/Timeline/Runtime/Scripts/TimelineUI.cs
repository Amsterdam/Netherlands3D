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
        /// At time bar setup this is the most left bar
        /// </summary>
        private int mostLeftIndex;
        private int mostRightIndex;

        // Start is called before the first frame update
        void Start()
        {
            SetupTimeBars();
        }

        // Update is called once per frame
        void Update()
        {
            ScrollTimeBar(v * Time.deltaTime);
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
                // Update the first in line panel
                timeBars[mostLeftIndex].localPosition += Vector3.right * scrollAmount;

                // If first in line is out of bounds set new first in line
                if(timeBars[mostLeftIndex].localPosition.x < timeBarParent.localPosition.x + TimeBarParentWidth * -1)
                {
                    mostLeftIndex = mostLeftIndex >= timeBars.Length - 1 ? 0 : mostLeftIndex + 1;
                }

                // Update rest of panels based on first in line position
                for(int i = 0; i < timeBars.Length; i++)
                {
                    if(i == mostLeftIndex) continue;
                    int increment = i > mostLeftIndex ? i - mostLeftIndex : (timeBars.Length - mostLeftIndex) + i;
                    timeBars[i].localPosition = new Vector3(timeBars[mostLeftIndex].localPosition.x + increment * TimeBarParentWidth, 0, 0);
                }
            }
            else
            {
                // Scrolling right
                // Update the last in line panel
                timeBars[mostRightIndex].localPosition += Vector3.right * scrollAmount;

                // If last in line is out of bounds set new last in line
                if(timeBars[mostRightIndex].localPosition.x >= TimeBarParentWidth)
                {
                    mostRightIndex = mostRightIndex <= 0 ? timeBars.Length - 1 : mostRightIndex - 1;
                    print(mostRightIndex);
                }

                // Update rest of panels based on last in line position
                for(int i = 0; i < timeBars.Length; i++)
                {
                    if(i == mostRightIndex) continue;
                    //int increment = i < mostRightIndex ? i - mostRightIndex : i - mostRightIndex - 1;
                    // need to do something with an array, that just pushes it to the next and loops index around
                    // [0, 1, 2] [2, 1, 0] then based on left or right get its index in the array of how much increment it needs
                    int increment = 0;
                    switch(mostRightIndex) // This should be possible to do in a 1 liner as seen above but i cant wrap my head around it so hardcoded 4 now
                    {
                        default:
                            if(i == 2) increment = 1; else increment = 2;
                            break;
                        case 1:
                            if(i == 0) increment = 1; else increment = 2;
                            break;
                        case 2:
                            if(i == 1) increment = 1; else increment = 2;
                            break;
                    }
                    timeBars[i].localPosition = new Vector3(timeBars[mostRightIndex].localPosition.x + increment * TimeBarParentWidth, 0, 0);
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
            mostRightIndex = 2;
        }
    }
}
