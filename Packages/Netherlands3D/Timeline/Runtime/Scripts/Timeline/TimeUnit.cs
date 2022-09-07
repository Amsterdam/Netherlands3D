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
            year10, // Every 10 years
            year5, // Every 5 years
            year,
            month,
            day,
            hour,
            minutes,
            seconds,
            milliseconds
        }

        /// <summary>
        /// Add time unit to a datime time with corresponding value
        /// </summary>
        /// <param name="dateTime">The dateTime to 'add' the unit too</param>
        /// <param name="unit">The time unit to add</param>
        /// <param name="value">The value to add</param>
        /// <returns>The adjusted DateTime</returns>
        public static DateTime AddUnitToDateTime(DateTime dateTime, Unit unit, int value)
        {
            return unit switch
            {
                Unit.year10 =>          dateTime.AddYears(value * 10),
                Unit.year5 =>           dateTime.AddYears(value * 5),
                Unit.year =>            dateTime.AddYears(value),
                Unit.month =>           dateTime.AddMonths(value),
                Unit.day =>             dateTime.AddDays(value),
                Unit.hour =>            dateTime.AddHours(value),
                Unit.minutes =>         dateTime.AddMinutes(value),
                Unit.seconds =>         dateTime.AddSeconds(value),
                Unit.milliseconds =>    dateTime.AddMilliseconds(value),
                _=> throw new ArgumentOutOfRangeException("unit")
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
            unit = (Unit)uValue;
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
            int v = 0;
            if(barIndex == 0) v = -1; else if(barIndex == 2) v = 1;

            return unit switch
            {
                Unit.year10 =>          dateTime.AddYears(changeValue * v * 10),
                Unit.year5 =>           dateTime.AddYears(changeValue * v * 5),
                Unit.year =>            dateTime.AddYears(changeValue * v),
                Unit.month =>           dateTime.AddMonths(changeValue * v),
                Unit.day =>             dateTime.AddDays(changeValue * v),
                Unit.hour =>            dateTime.AddHours(changeValue * v),
                Unit.minutes =>         dateTime.AddMinutes(changeValue * v),
                Unit.seconds =>         dateTime.AddSeconds(changeValue * v),
                Unit.milliseconds =>    dateTime.AddMilliseconds(changeValue * v),
                _ => throw new ArgumentOutOfRangeException("unit"),
            };
        }

        /// <summary>
        /// Get the closest dateTime out of an array
        /// </summary>
        /// <param name="dateTime">The dateTime to find the closest of in the array</param>
        /// <param name="dateTimes">The array of dateTimes to search</param>
        /// <returns>DateTime</returns>
        public static DateTime GetClosestDateTime(DateTime dateTime, DateTime[] dateTimes)
        {
            return ArrayExtention.MinBy(dateTimes, x => Math.Abs((x - dateTime).Ticks));
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
                Unit.year10 =>          dateTime.AddYears(datesToPlace * v * 10),
                Unit.year5 =>           dateTime.AddYears(datesToPlace * v * 5),
                Unit.year =>            dateTime.AddYears(datesToPlace * v),
                Unit.month =>           dateTime.AddMonths(datesToPlace * v),
                Unit.day =>             dateTime.AddDays(datesToPlace * v),
                Unit.hour =>            dateTime.AddHours(datesToPlace * v),
                Unit.minutes =>         dateTime.AddMinutes(datesToPlace * v),
                Unit.seconds =>         dateTime.AddSeconds(datesToPlace * v),
                Unit.milliseconds =>    dateTime.AddMilliseconds(datesToPlace * v),
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
                Unit.year10 =>      "yyyy",
                Unit.year5 =>       "yyyy",
                Unit.year =>        "yyyy",
                Unit.month =>       "MM",
                Unit.day =>         "dd",
                Unit.hour =>        "HH",
                Unit.minutes =>     "mm",
                Unit.seconds =>     "ss",
                Unit.milliseconds => ".ff",
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
                Unit.year10 =>      "yyyy" + " x10",
                Unit.year5 =>       "yyyy" + " x5",
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
        /// Check if the time unit & datetime match
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="dateTimeA"></param>
        /// <param name="dateTimeB"></param>
        /// <returns>bool</returns>
        public static bool CompareDateTimes(Unit unit, DateTime dateTimeA, DateTime dateTimeB)
        {
            return unit switch
            {
                Unit.year10 =>          dateTimeA.Year == dateTimeB.Year,
                Unit.year5 =>           dateTimeA.Year == dateTimeB.Year,
                Unit.year =>            dateTimeA.Year == dateTimeB.Year,
                Unit.month =>           dateTimeA.Year == dateTimeB.Year && dateTimeA.Month == dateTimeB.Month,
                Unit.day =>             dateTimeA.Year == dateTimeB.Year && dateTimeA.Month == dateTimeB.Month & dateTimeA.Day == dateTimeB.Day,
                Unit.hour =>            dateTimeA.Year == dateTimeB.Year && dateTimeA.Month == dateTimeB.Month & dateTimeA.Day == dateTimeB.Day && dateTimeA.Hour == dateTimeB.Hour,
                Unit.minutes =>         dateTimeA.Year == dateTimeB.Year && dateTimeA.Month == dateTimeB.Month & dateTimeA.Day == dateTimeB.Day && dateTimeA.Hour == dateTimeB.Hour && dateTimeA.Minute == dateTimeB.Minute,
                Unit.seconds =>         dateTimeA.Year == dateTimeB.Year && dateTimeA.Month == dateTimeB.Month & dateTimeA.Day == dateTimeB.Day && dateTimeA.Hour == dateTimeB.Hour && dateTimeA.Minute == dateTimeB.Minute && dateTimeA.Second == dateTimeB.Second,
                Unit.milliseconds =>    dateTimeA.Year == dateTimeB.Year && dateTimeA.Month == dateTimeB.Month & dateTimeA.Day == dateTimeB.Day && dateTimeA.Hour == dateTimeB.Hour && dateTimeA.Minute == dateTimeB.Minute && dateTimeA.Second == dateTimeB.Second && dateTimeA.Millisecond == dateTimeB.Millisecond,
                _ => throw new ArgumentOutOfRangeException("unit")
            };
        }

        /// <summary>
        /// Check if the datetime is in the datetime range
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="rangeStart"></param>
        /// <param name="rangeEnd"></param>
        /// <returns></returns>
        public static bool DateTimeInRange(DateTime dateTime, DateTime rangeStart, DateTime rangeEnd)
        {
            return rangeStart <= dateTime && dateTime <= rangeEnd;
        }
    }

}
