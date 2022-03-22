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

        private int firstInLineIndex;

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
            // Update the first in line panel
            timeBars[firstInLineIndex].localPosition += Vector3.right * scrollAmount;

            // If first in line is out of bounds set new first in line
            // Scrolling left
            if(timeBars[firstInLineIndex].localPosition.x < timeBarParent.localPosition.x + TimeBarParentWidth * -1)
            {
                firstInLineIndex = firstInLineIndex >= timeBars.Length - 1 ? 0 : firstInLineIndex + 1;
            }
            // Scrolling right
            else if(timeBars[firstInLineIndex].localPosition.x > timeBarParent.localPosition.x + TimeBarParentWidth)
            {
                firstInLineIndex = firstInLineIndex <= 0 ? timeBars.Length - 1 : firstInLineIndex - 1;
            }

            // Update rest of panels based on first in line position
            for(int i = 0; i < timeBars.Length; i++)
            {
                if(i == firstInLineIndex) continue;
                int increment = i > firstInLineIndex ? i - firstInLineIndex : (timeBars.Length - firstInLineIndex) + i;
                timeBars[i].localPosition = new Vector3(timeBars[firstInLineIndex].localPosition.x + increment * TimeBarParentWidth, 0, 0);
                //timeBars[i].SetRect(0, 0, -timeBars[firstInLineIndex].position.x + increment * TimeBarParentWidth, timeBars[firstInLineIndex].position.x + increment * TimeBarParentWidth);
            }
        }

        private void SetupTimeBars()
        {
            // Position the time bars correctly
            // Hardcoded 3 time bars, as there is no need for more than 3
            timeBars[0].SetRect(0, 0, -timeBarParent.rect.width, timeBarParent.rect.width);
            timeBars[1].SetRect(0, 0, 0, 0);
            timeBars[2].SetRect(0, 0, timeBarParent.rect.width, -timeBarParent.rect.width);
        }
    }
}
