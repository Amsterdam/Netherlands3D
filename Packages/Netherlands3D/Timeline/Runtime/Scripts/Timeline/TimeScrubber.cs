using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// The time scrubber component of the timeline UI
    /// </summary>
    public class TimeScrubber : MonoBehaviour
    {
        /// <summary>
        /// Is the time scrubber being used?
        /// </summary>
        public bool IsActive { get; private set; }
        /// <summary>
        /// Is the time scrubber scrolling by being on the edge on the left/right side?
        /// </summary>
        public bool IsScrollingWithEdge { get; private set; }
        /// <summary>
        /// Is the timescrubber scrolling via the TimelinePlayback?
        /// </summary>
        public bool PlaybackScroll { get; set; }

        [Tooltip("When the scroll is all the way on the left/right move the timebar with this speed")]
        public float timeBarScrollSpeed = 5;

        [Header("Components")]
        [SerializeField] private RectTransform rt;
        [SerializeField] private Slider slider;
        [SerializeField] private TimelineUI timelineUI;

        private Coroutine coroutineScrollWithTimeScrubber;

        // Start is called before the first frame update
        void Start()
        {            
            StoppedUsing();
        }

        /// <summary>
        /// When the slider changes value
        /// </summary>
        public void OnValueChanged()
        {
            // If the time slider is all the way left or right move the timeline & ignore slider
            if(!PlaybackScroll)
            {
                if(slider.value <= 0.05f)
                {
                    StopScrollWithTimeScrubber();
                    coroutineScrollWithTimeScrubber = StartCoroutine(ScrollWithTimeScrubber(100 * timeBarScrollSpeed * Time.deltaTime));
                    return;
                }
                else if(slider.value >= 0.95f)
                { 
                    StopScrollWithTimeScrubber();
                    coroutineScrollWithTimeScrubber = StartCoroutine(ScrollWithTimeScrubber(-100 * timeBarScrollSpeed * Time.deltaTime));
                    return;
                }
                StopScrollWithTimeScrubber();
            }

            // Set the timelineUI currentDate based on slider value
            // First get the range of the slider since the sides are off screen
            float width = rt.rect.width;
            float value = width * slider.value;
            // Convert to bar local x (-960, 0, 960)
            float convertedValue = value - (width / 2);
            DateTime dt = timelineUI.GetClosestBar(convertedValue).GetCurrentDateTime(convertedValue, false);
            if(dt.Year != 1)
            {
                timelineUI.SetCurrentDateNoNotify(dt);
                IsActive = true;
            }
        }

        /// <summary>
        /// Scroll the time scrubber.
        /// </summary>
        /// <remarks>Scroll amount value has to be between -1 to 1 (Since the slider gets updated by this value)</remarks>
        /// <param name="scrollAmount"></param>
        public void ScrollTimeScrubber(float scrollAmount)
        {
            slider.value += scrollAmount;
        }

        /// <summary>
        /// Called when the time scrubber is not being used anymore
        /// </summary>
        public void StoppedUsing()
        {
            if(coroutineScrollWithTimeScrubber != null) StopCoroutine(coroutineScrollWithTimeScrubber);
            // Center scrubber
            slider.SetValueWithoutNotify(0.5f);
            IsActive = false;
        }

        public void StopScrollWithTimeScrubber()
        {
            IsScrollingWithEdge = false;
            if(coroutineScrollWithTimeScrubber != null) StopCoroutine(coroutineScrollWithTimeScrubber);
        }


        private IEnumerator ScrollWithTimeScrubber(float speed)
        {
            IsScrollingWithEdge = true;
            while(true)
            {
                timelineUI.ScrollTimeBar(speed, false);
                if(PlaybackScroll) yield break;
                yield return null;
            }
        }
    }
}
