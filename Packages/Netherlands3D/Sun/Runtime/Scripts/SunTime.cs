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
using UnityEngine;

namespace Netherlands3D.Sun
{
    [ExecuteInEditMode]
    public class SunTime : MonoBehaviour
    {
        [Header("Location")]

        [SerializeField]
        private float longitude;

        [SerializeField]
        private float latitude;

        [Header("Time")]
        [SerializeField]
        private DateTimeKind dateTimeKind = DateTimeKind.Local;

        [SerializeField]
        private bool jumpToCurrentTimeAtStart = false;

        [SerializeField]
        [Range(0, 24)]
        private int hour = 18;

        [SerializeField]
        [Range(0, 60)]
        private int minutes = 0;

        [SerializeField]
        [Range(0, 60)]
        private int seconds = 0;

        [SerializeField]
        [Range(1,31)]
        private int day = 13;

        [SerializeField]
        [Range(1, 12)]
        private int month = 8;

        [SerializeField]
        [Range(1, 9999)]
        private int year = 2022;

        private DateTime time;

        [SerializeField]
        private Light sunDirectionalLight;

        [SerializeField]
        private bool animate = true;

        [SerializeField]
        private float timeSpeed = 1;

        [SerializeField]
        private int frameSteps = 1;
        private int frameStep;

        private const int gizmoRayLength = 10000;
        private const int gizmoSunSize = 20;
        private const int gizmoSunDistance = 20;

		private void OnDrawGizmos()
		{
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(this.transform.position, this.transform.position - sunDirectionalLight.transform.forward * gizmoRayLength);
		}

		public void ToggleAnimation(bool animate)
        {
            this.animate = animate;
        }

        public void SetTime(DateTime time) {
            this.time = time;
            OnValidate();
        }

        public void SetTime(int hour, int minutes) {
            this.hour = hour;
            this.minutes = minutes;
            OnValidate();
        }

        public void SetHour(int hour)
        {
            this.hour = hour;
            OnValidate();
        }

        public void SetSeconds(int seconds)
        {
            this.seconds = seconds;
            OnValidate();
        }

        public void SetMinutes(int minutes)
        {
            this.minutes = minutes;
            OnValidate();
        }

        public void SetDay(int day)
        {
            this.day = day;
            OnValidate();
        }

        public void SetMonth(int month)
        {
            this.month = month;
            OnValidate();
        }

        public void SetYear(int year)
        {
            this.year = year;
            OnValidate();
        }

        public void SetLocation(float longitude, float latitude){
          this.longitude = longitude;
          this.latitude = latitude;
        }

        public void SetUpdateSteps(int i) {
            frameSteps = i;
        }

        public void SetTimeSpeed(float speed) {
            timeSpeed = speed;
        }

        private void Start()
        {
            if (jumpToCurrentTimeAtStart)
            {
                time = DateTime.Now;

                hour = time.Hour;
                minutes = time.Minute;
                day = time.Day;
                month = time.Month;
                year = time.Year;
            }
        }

        private void OnValidate()
        {
            time = new DateTime(year,month,day,hour,minutes,seconds,dateTimeKind);
            SetPosition();
        }

        private void Update()
        {
            if (!animate) return;

            time = time.AddSeconds(timeSpeed * Time.deltaTime);
            if (frameStep==0) {
                SetPosition();
            }
            frameStep = (frameStep + 1) % frameSteps;
		}

        void SetPosition()
        {
            Vector3 angles = new Vector3();
			SunPosition.CalculateSunPosition(time, (double)latitude, (double)longitude, out double azi, out double alt);
			angles.x = (float)alt * Mathf.Rad2Deg;
            angles.y = (float)azi * Mathf.Rad2Deg;

            sunDirectionalLight.transform.localRotation = Quaternion.Euler(angles);
        }        
    }
}