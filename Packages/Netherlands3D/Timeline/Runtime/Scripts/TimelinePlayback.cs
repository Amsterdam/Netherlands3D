using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// Handles the timeline playback system
    /// </summary>
    [AddComponentMenu("Netherlands3D/Timeline/TimelinePlayback")]
    public class TimelinePlayback : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image playIcon;
        [SerializeField] private Sprite spritePlay;
        [SerializeField] private Sprite spritePause;
        [SerializeField] private TMP_InputField inputFieldPlaybackSpeed;
        [SerializeField] private TimelineUI timelineUI;

        /// <summary>
        /// Is the playback playing the timeline?
        /// </summary>
        private bool isPlaying;
        /// <summary>
        /// How fast the timeline is playing
        /// </summary>
        private int playbackSpeed;
        /// <summary>
        /// Enumerator for ScrollTimeBarAutomaticly
        /// </summary>
        private Coroutine coroutineScrollTimeBarAutomaticly;
        /// <summary>
        /// Coroutine for ScrollScrubberAutomaticly
        /// </summary>
        private Coroutine coroutineScrollScrubberAutomaticly;

        /// <summary>
        /// When the playback speed is changed
        /// </summary>
        public void OnPlaybackSpeedChanged()
        {
            int value = 1;
            int.TryParse(inputFieldPlaybackSpeed.text.Replace("x", ""), out value);
            playbackSpeed = value;
            inputFieldPlaybackSpeed.text = value.ToString() + "x";
        }

        /// <summary>
        /// Toggle the playback
        /// </summary>
        public void TogglePlay()
        {
            isPlaying = !isPlaying;
            playIcon.sprite = isPlaying ? spritePause : spritePlay;
            OnPlaybackSpeedChanged();
            PlayScroll(isPlaying);
        }

        /// <summary>
        /// Play the automatic scrolling of the timeline/scrubber
        /// </summary>
        /// <param name="autoPlay"></param>
        public void PlayScroll(bool play)
        {
            if(play)
            {
                timelineUI.timeScrubber.PlaybackScroll = true;
                // Check if timeline or time scrubber
                if(timelineUI.timeScrubber.IsActive)
                {
                    // Use timescrubber
                    coroutineScrollScrubberAutomaticly = StartCoroutine(ScrollTimeScrubberAutomaticly());
                }
                else
                {
                    // Use timeline
                    coroutineScrollTimeBarAutomaticly = StartCoroutine(ScrollTimeBarAutomaticly());
                }
            }
            else
            {
                if(coroutineScrollTimeBarAutomaticly != null) StopCoroutine(coroutineScrollTimeBarAutomaticly);
                if(coroutineScrollScrubberAutomaticly != null) StopCoroutine(coroutineScrollScrubberAutomaticly);
                timelineUI.timeScrubber.PlaybackScroll = false;
            }
        }

        /// <summary>
        /// Scroll the timebar automaticly
        /// </summary>
        /// <returns></returns>
        private IEnumerator ScrollTimeBarAutomaticly()
        {
            while(true)
            {
                timelineUI.ScrollTimeBar(-100 * playbackSpeed * Time.deltaTime);
                yield return null;
            }
        }

        /// <summary>
        /// Scroll the time scrubber automaticly
        /// </summary>
        /// <returns></returns>
        private IEnumerator ScrollTimeScrubberAutomaticly()
        {
            while(true)
            {
                timelineUI.timeScrubber.ScrollTimeScrubber(0.1f * playbackSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}
