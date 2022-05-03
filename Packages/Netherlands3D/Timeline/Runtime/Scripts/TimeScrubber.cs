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

        [Header("Components")]
        [SerializeField] private Slider slider;
        [SerializeField] private TimelineUI timelineUI;

        private RectTransform rt;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
        }

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
            // Set the timelineUI currentDate based on slider value
            // First get the range of the slider since the sides are off screen
            float width = rt.rect.width;
            float value = width * slider.value;
            // Convert to bar local x (-960, 0, 960)
            float convertedValue = value - (width / 2);
            timelineUI.SetCurrentDateNoNotify(timelineUI.GetClosestBar(convertedValue).GetCurrentDateTime(convertedValue));
            IsActive = true;
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
            // Center scrubber
            slider.SetValueWithoutNotify(0.5f);
            IsActive = false;
        }
    }
}
