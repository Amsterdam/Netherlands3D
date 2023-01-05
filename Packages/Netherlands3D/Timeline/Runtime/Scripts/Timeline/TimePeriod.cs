using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// A timeline time period that holds data of something that happend between a start date and a end date
    /// </summary>
    [System.Serializable]
    public class TimePeriod : IComparable
    {
        [Tooltip("The time period name")]
        public string name;
        [Tooltip("The description of the time period")]
        [TextArea(1, 10)]
        public string description;
        [Tooltip("The start date of the time period")]
        public DateTimeSerializable startDate;
        [Tooltip("The end date of the time period")]
        public DateTimeSerializable endDate;
        [Tooltip("The layer of the time period")]
        public string layer;
        //[Tooltip("(WIP) The group the time period belongs too. Example: \"City/Buildings/Fast Food\"")]
        //public string group;

        [Header("Events")]
        [Tooltip("The event that gets invoked when the time period is pressed")]
        public UnityEvent eventPressed = new UnityEvent();
        [Tooltip("When the time period enters on screen")]
        public UnityEvent eventScreenEnter = new UnityEvent();
        [Tooltip("When the time period exits off screen")]
        public UnityEvent eventScreenExit = new UnityEvent();
        [Tooltip("When the time period enters the currentTime range")]
        public UnityEvent eventCurrentTimeEnter = new UnityEvent();
        [Tooltip("When the time period exits the currentTime range")]
        public UnityEvent eventCurrentTimeExit = new UnityEvent();
        [Tooltip("When the layer of this time period gets set to show its periods")]
        public UnityEvent eventLayerShow = new UnityEvent();
        [Tooltip("When the layer of this time period gets set to hide its periods")]
        public UnityEvent eventLayerHide = new UnityEvent();

        public TimePeriod(string name, string description, DateTimeSerializable startDate, DateTimeSerializable endDate, string layer = ""/*, string group = ""*/)
        {
            this.name = name;
            this.description = description;
            this.startDate = startDate;
            this.endDate = endDate;
            this.layer = layer;
            //this.group = group;
        }

        public int CompareTo(object obj)
        {
            TimePeriod t = obj as TimePeriod;
            if(t.startDate.Value > startDate.Value) return -1;
            if(t.startDate.Value == startDate.Value) return 0;
            if(t.startDate.Value < startDate.Value) return 1;
            return 1;
        }
    }
}
