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

        // Start is called before the first frame update
        void Start()
        {
        
        }

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
            timelineUI.PlayScroll(isPlaying);
        }
    }
}
