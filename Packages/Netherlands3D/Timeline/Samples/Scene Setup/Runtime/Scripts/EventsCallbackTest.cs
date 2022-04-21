using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline.Samples
{
    public class EventsCallbackTest : MonoBehaviour
    {
        public TimePeriod timePeriod;
        public GameObject cube;
        public TimelineUI timelineUI;
        public GameObject sphere;

        private bool cubeActive = true;
        private bool sphereActive = true;

        private void OnEnable()
        {
            timePeriod.unityEvent.AddListener(ToggleCube);
            timelineUI.onCurrentDateChange.AddListener(OnDateChange);
            timelineUI.onCategoryToggle.AddListener(OnCategoryToggle);
        }

        private void OnDisable()
        {
            timePeriod.unityEvent.RemoveListener(ToggleCube);
            timelineUI.onCurrentDateChange.RemoveListener(OnDateChange);
            timelineUI.onCategoryToggle.RemoveListener(OnCategoryToggle);
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void ToggleCube()
        {
            cube.SetActive(!cube.activeSelf);
        }

        public void OnDateChange(DateTime date)
        {
            sphere.SetActive(sphereActive && TimeUnit.DateTimeInRange(date, new DateTime(2018, 1, 1), new DateTime(2024, 1, 1)));
        }

        public void OnCategoryToggle(string name, bool isActive)
        {
            if(name == "Bomen") cubeActive = isActive;
            cube.SetActive(cubeActive);
            if(name == "Auto's") sphereActive = isActive;
            sphere.SetActive(sphereActive && TimeUnit.DateTimeInRange(timelineUI.CurrentDate, new DateTime(2018, 1, 1), new DateTime(2024, 1, 1)));
        }
    }
}
