using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// For serializing DateTime to be used in Unity inspector
    /// </summary>
    [Serializable]
    public class DateTimeSerializable : ISerializationCallbackReceiver
    {
        /// <summary>
        /// The format of how the date is
        /// </summary>
        private static readonly string dateFormat = "yyyy/MM/dd";// HH:mm";

        /// <summary>
        /// The DateTime value
        /// </summary>
        public DateTime Value;

        /// <summary>
        /// The DateTime in string format
        /// </summary>
        [HideInInspector] [SerializeField] private string dateTimeString;

        /// <summary>
        /// For converting DateTimeSerializable to DateTime
        /// </summary>
        /// <param name="dateTimeSerializable"></param>
        public static implicit operator DateTime(DateTimeSerializable dateTimeSerializable)
        {
            return dateTimeSerializable.Value;
        }

        /// <summary>
        /// For converting DateTime to DateTimeSerializable
        /// </summary>
        /// <param name="dateTime"></param>
        public static implicit operator DateTimeSerializable(DateTime dateTime)
        {
            return new DateTimeSerializable() { Value = dateTime };
        }

        public void OnBeforeSerialize()
        {
            dateTimeString = Value.ToString(dateFormat);
        }

        public void OnAfterDeserialize()
        {
            DateTime.TryParse(dateTimeString, out Value);
        }
    }
}
