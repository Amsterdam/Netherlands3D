using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline
{
    public class TimeUnit
    {
        /// <summary>
        /// The unit of time used for display
        /// </summary>
        /// <remarks>
        /// If time unity where to be minutes it will show as "year/month/day hour:minutes"
        /// </remarks>
        public enum Unit
        {
            year,
            month,
            day,
            hour,
            minutes,
            seconds,
            milliseconds
        }

        /// <summary>
        /// Get the starting date for a bar based on dateTime, the time unit used and the change value
        /// </summary>
        /// <param name="dateTime">The dateTime to add it too</param>
        /// <param name="unit">The time unit used</param>
        /// <param name="barIndex">The index of the bar it is for</param>
        /// <param name="changeValue">The change value (the datesToPlace value) </param>
        /// <returns>Starting date time</returns>
        public static DateTime GetBarStartingDate(DateTime dateTime, Unit unit, int barIndex, int changeValue)
        {
            return unit switch
            {
                Unit.year => barIndex switch
                {
                    0 => dateTime.AddYears(-changeValue),
                    1 => dateTime,
                    2 => dateTime.AddYears(changeValue),
                    _ => throw new Exception("TimeUnit out of range exception!"),
                },
                Unit.month => barIndex switch
                {
                    0 => dateTime.AddMonths(-changeValue),
                    1 => dateTime,
                    2 => dateTime.AddMonths(changeValue),
                    _ => throw new Exception("TimeUnit out of range exception!"),
                },
                Unit.day => barIndex switch
                {
                    0 => dateTime.AddDays(-changeValue),
                    1 => dateTime,
                    2 => dateTime.AddDays(changeValue),
                    _ => throw new Exception("TimeUnit out of range exception!"),
                },
                Unit.hour => barIndex switch
                {
                    0 => dateTime.AddHours(-changeValue),
                    1 => dateTime,
                    2 => dateTime.AddHours(changeValue),
                    _ => throw new Exception("TimeUnit out of range exception!"),
                },
                Unit.minutes => barIndex switch
                {
                    0 => dateTime.AddMinutes(-changeValue),
                    1 => dateTime,
                    2 => dateTime.AddMinutes(changeValue),
                    _ => throw new Exception("TimeUnit out of range exception!"),
                },
                Unit.seconds => barIndex switch
                {
                    0 => dateTime.AddSeconds(-changeValue),
                    1 => dateTime,
                    2 => dateTime.AddSeconds(changeValue),
                    _ => throw new Exception("TimeUnit out of range exception!"),
                },
                Unit.milliseconds => barIndex switch
                {
                    0 => dateTime.AddMilliseconds(-changeValue),
                    1 => dateTime,
                    2 => dateTime.AddMilliseconds(changeValue),
                    _ => throw new Exception("TimeUnit out of range exception!"),
                },
                _ => throw new Exception("TimeUnit out of range exception!"),
            };
        }

        /// <summary>
        /// Get the visible date for either left or right
        /// </summary>
        /// <param name="isLeft"></param>
        /// <param name="dateTime"></param>
        /// <param name="unit"></param>
        /// <param name="datesToPlace"></param>
        /// <returns></returns>
        public static DateTime GetVisibleDateLeftRight(bool isLeft, DateTime dateTime, Unit unit, int datesToPlace)
        {
            int v = isLeft ? -1 : 1;
            return unit switch
            {
                Unit.year => dateTime.AddYears(datesToPlace * v),
                Unit.month => dateTime.AddMonths(datesToPlace * v),
                Unit.day => dateTime.AddDays(datesToPlace * v),
                Unit.hour => dateTime.AddHours(datesToPlace * v),
                Unit.minutes => dateTime.AddMinutes(datesToPlace * v),
                Unit.seconds => dateTime.AddSeconds(datesToPlace * v),
                Unit.milliseconds => dateTime.AddMilliseconds(datesToPlace * v),
                _ => throw new ArgumentOutOfRangeException("unit"),
            };
        }

        /// <summary>
        /// Get the corresponding display string for Unit
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static string GetUnitString(Unit unit)
        {
            return unit switch
            {
                Unit.year =>        "yyyy",
                Unit.month =>       "MM",
                Unit.day =>         "dd",
                Unit.hour =>        "HH",
                Unit.minutes =>     "mm",
                _ => ""
            };
        }

        /// <summary>
        /// Get the full corresponding display string for Unit
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static string GetUnitFullString(Unit unit)
        {
            return unit switch
            {
                Unit.year =>        "yyyy",
                Unit.month =>       "yyyy/MM",
                Unit.day =>         "yyyy/MM/dd",
                Unit.hour =>        "yyyy/MM/dd HH",
                Unit.minutes =>     "yyyy/MM/dd HH:mm",
                Unit.seconds =>     "yyyy/MM/dd HH:mm:ss",
                Unit.milliseconds => "yyyy/MM/dd HH:mm:ss.ff",
                _ => ""
            };
        }

        /// <summary>
        /// Change the unit by a added/subtracted value
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="value"></param>
        public static void ChangeUnit(ref Unit unit, int value)
        {
            int uValue = (int)unit;
            uValue += value;
            if(uValue < 0) uValue = 0;
            else if(uValue >= Enum.GetNames(typeof(Unit)).Length) uValue = Enum.GetNames(typeof(Unit)).Length - 1;
            Debug.Log(uValue);
            unit = (Unit)uValue;
        }
    }

}
