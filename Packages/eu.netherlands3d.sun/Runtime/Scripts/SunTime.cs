/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/Netherlands3D/blob/main/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/

using System;
using Netherlands3D.Coordinates;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Sun
{
    [ExecuteInEditMode]
    public class SunTime : MonoBehaviour
    {
        [Header("Time")]
        [SerializeField] private DateTimeKind dateTimeKind = DateTimeKind.Local;

        [SerializeField] private bool jumpToCurrentTimeAtStart = false;
        [SerializeField] [Range(0, 24)] private int hour = 18;
        [SerializeField] [Range(0, 60)] private int minutes = 0;
        [SerializeField] [Range(0, 60)] private int seconds = 0;
        [SerializeField] [Range(1, 31)] private int day = 13;
        [SerializeField] [Range(1, 12)] private int month = 8;
        [SerializeField] [Range(1, 9999)] private int year = 2022;

        [Header("Settings")]
        [SerializeField] private Light sunDirectionalLight;
        [SerializeField] private bool animate = true;
        [SerializeField] private float timeSpeed = 1;
        [SerializeField] private int frameSteps = 1;

        [Header("Events")]
        public UnityEvent<DateTime> timeOfDayChanged = new();
        public UnityEvent<float> timeSpeedChanged = new();

        private float longitude;
        private float latitude;
        private DateTime time;
        private int frameStep;
        private const int gizmoRayLength = 10000;

        private void Start()
        {
            if (jumpToCurrentTimeAtStart)
            {
                ResetToNow();
            }

            DetermineCurrentLocationFromOrigin();
            Apply();
        }

        private void OnValidate()
        {
            Apply();
        }

        private void Update()
        {
            if (!animate) return;

            time = time.AddSeconds(timeSpeed * Time.deltaTime);
            if (frameStep == 0)
            {
                UpdateTimeOfDayPartsFromTime();
                timeOfDayChanged.Invoke(time);
                SetPosition();
            }

            frameStep = (frameStep + 1) % frameSteps;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var position = this.transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(position, position - sunDirectionalLight.transform.forward * gizmoRayLength);
        }
#endif

        public void ToggleAnimation(bool animate)
        {
            this.animate = animate;
        }

        public DateTime GetTime()
        {
            return time;
        }

        public void SetTime(DateTime time)
        {
            this.time = time;
            UpdateTimeOfDayPartsFromTime();
            Apply();
        }

        public void SetTime(int hour, int minutes)
        {
            this.hour = Mathf.Clamp(hour, 0, 24);
            this.minutes = Mathf.Clamp(minutes, 0, 60);
            Apply();
        }

        public void SetHour(int hour)
        {
            this.hour = Mathf.Clamp(hour, 0, 24);
            Apply();
        }

        public void SetSeconds(int seconds)
        {
            this.seconds = Mathf.Clamp(seconds, 0, 60);
            Apply();
        }

        public void SetMinutes(int minutes)
        {
            this.minutes = Mathf.Clamp(minutes, 0, 60);
            Apply();
        }

        public void SetDay(int day)
        {
            this.day = Mathf.Clamp(day, 1, 31);
            Apply();
        }

        public void SetMonth(int month)
        {
            this.month = Mathf.Clamp(month, 1, 12);
            Apply();
        }

        public void SetYear(int year)
        {
            this.year = year;
            Apply();
        }

        public void SetLocation(float longitude, float latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;

            SetPosition();
        }

        public void SetUpdateSteps(int i)
        {
            frameSteps = i;
        }

        public void SetTimeSpeed(float multiplicationFactor)
        {
            timeSpeed = Math.Clamp(timeSpeed * multiplicationFactor, 1, 10000);
            timeSpeedChanged.Invoke(timeSpeed);
        }

        public void ResetToNow()
        {
            time = DateTime.Now;

            UpdateTimeOfDayPartsFromTime();
        }

        private void UpdateTimeOfDayPartsFromTime()
        {
            hour = time.Hour;
            minutes = time.Minute;
            day = time.Day;
            month = time.Month;
            year = time.Year;
        }

        private void Apply()
        {
            var newTime = new DateTime(year, month, day, hour, minutes, seconds, dateTimeKind);
            if (newTime != time)
            {
                timeOfDayChanged.Invoke(time);
            }

            time = newTime;
            SetPosition();
        }

        private void DetermineCurrentLocationFromOrigin()
        {
            var wgs84SceneCenter = CoordinateConverter.RDtoWGS84(EPSG7415.relativeCenter.x, EPSG7415.relativeCenter.y);
            longitude = (float)wgs84SceneCenter.lon;
            latitude = (float)wgs84SceneCenter.lat;
        }

        private void SetPosition()
        {
            Vector3 angles = new Vector3();
            SunPosition.CalculateSunPosition(time, (double)latitude, (double)longitude, out double azi, out double alt);
            angles.x = (float)alt * Mathf.Rad2Deg;
            angles.y = (float)azi * Mathf.Rad2Deg;

            sunDirectionalLight.transform.localRotation = Quaternion.Euler(angles);
        }
    }
}
