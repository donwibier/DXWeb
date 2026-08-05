////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	Scheduler\Cron\CronScheduler.cs
//
// summary:	Implements the cron scheduler class
////////////////////////////////////////////////////////////////////////////////////////////////////

using DX.Utils.CronJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DX.Utils.CronJobs
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   An implementation of the Cron scheduler. </summary>
    ///
    /// <remarks>   Don, 29-11-2012. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class CronSchedule
    {
        /// <summary>   The minutes. </summary>
        private readonly MinutesCronEntry _minutes;
        /// <summary>   The hours. </summary>
        private readonly HoursCronEntry _hours;
        /// <summary>   The days. </summary>
        private readonly DaysCronEntry _days;
        /// <summary>   The months. </summary>
        private readonly MonthsCronEntry _months;
        /// <summary>   The days of week. </summary>
        private readonly DaysOfWeekCronEntry _daysOfWeek;

        public static CronSchedule Create(CronExpression cronExpression)
        {
            if (cronExpression == null)
            {
                throw new ArgumentNullException("cronExpression");
            }
            return Parse(cronExpression.Minutes, cronExpression.Hours, cronExpression.Days, cronExpression.Months, cronExpression.DaysOfWeek);
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Parses. </summary>
        ///
        /// <remarks>   Don, 29-11-2012. </remarks>
        ///
        /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
        ///                                         illegal values. </exception>
        ///
        /// <param name="cronExpression">   The cron expression. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static CronSchedule Parse(string cronExpression)
        {
            if (string.IsNullOrEmpty(cronExpression))
            {
                throw new ArgumentException("cronExpression");
            }
            string[] parts = cronExpression.Split(' ');
            if (parts.Length != 5)
            {
                throw new ArgumentException("cronExpression");
            }
            return Parse(parts[0], parts[1], parts[2], parts[3], parts[4]);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Parses. </summary>
        ///
        /// <remarks>   Don, 29-11-2012. </remarks>
        ///
        /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
        ///                                         illegal values. </exception>
        ///
        /// <param name="minutes">      The minutes. </param>
        /// <param name="hours">        The hours. </param>
        /// <param name="days">         The days. </param>
        /// <param name="months">       The months. </param>
        /// <param name="daysOfWeek">   The days of week. </param>
        ///
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static CronSchedule Parse(string minutes, string hours, string days, string months, string daysOfWeek)
        {
            if (string.IsNullOrEmpty(minutes))
            {
                throw new ArgumentException("minutes");
            }
            if (string.IsNullOrEmpty(hours))
            {
                throw new ArgumentException("hours");
            }
            if (string.IsNullOrEmpty(days))
            {
                throw new ArgumentException("days");
            }
            if (string.IsNullOrEmpty(months))
            {
                throw new ArgumentException("months");
            }
            if (string.IsNullOrEmpty(daysOfWeek))
            {
                throw new ArgumentException("daysOfWeek");
            }
            return new CronSchedule(minutes, hours, days, months, daysOfWeek);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Don, 29-11-2012. </remarks>
        ///
        /// <param name="minutes">      The minutes. </param>
        /// <param name="hours">        The hours. </param>
        /// <param name="days">         The days. </param>
        /// <param name="months">       The months. </param>
        /// <param name="daysOfWeek">   The days of week. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private CronSchedule(string minutes, string hours, string days, string months, string daysOfWeek)
        {
            _minutes = new MinutesCronEntry(minutes);
            _hours = new HoursCronEntry(hours);
            _days = new DaysCronEntry(days);
            _months = new MonthsCronEntry(months);
            _daysOfWeek = new DaysOfWeekCronEntry(daysOfWeek); // 0 = Sunday
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets all. </summary>
        ///
        /// <remarks>   Don, 29-11-2012. </remarks>
        ///
        /// <param name="start">    Date/Time of the start. </param>
        /// <param name="end">      Date/Time of the end. </param>
        ///
        /// <returns>   all. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public List<DateTime> GetAll(DateTime start, DateTime end)
        {
            List<DateTime> result = new List<DateTime>();

            DateTime current = start;
            while (current <= end)
            {
                DateTime next;
                if (!GetNext(current, end, out next))
                {
                    // Did not find any new ones...return what we have
                    //
                    break;
                }
                result.Add(next);
                current = next;
            }
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the next item. </summary>
        ///
        /// <remarks>   Don, 29-11-2012. </remarks>
        ///
        /// <param name="start">    Date/Time of the start. </param>
        /// <param name="next">     [out] The next. </param>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool GetNext(DateTime start, out DateTime next)
        {
            return GetNext(start, DateTime.MaxValue, out next);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the next item. </summary>
        ///
        /// <remarks>   Don, 29-11-2012. </remarks>
        ///
        /// <param name="start">    Date/Time of the start. </param>
        /// <param name="end">      Date/Time of the end. </param>
        /// <param name="next">     [out] The next. </param>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool GetNext(DateTime start, DateTime end, out DateTime next)
        {
            // Initialize the next output
            //
            next = DateTime.MinValue;

            // Don't want to select the actual start date.
            //
            DateTime baseSearch = start.AddMinutes(1.0);
            int baseMinute = baseSearch.Minute;
            int baseHour = baseSearch.Hour;
            int baseDay = baseSearch.Day;
            int baseMonth = baseSearch.Month;
            int baseYear = baseSearch.Year;

            // Get the next minute value
            //
            int minute = _minutes.Next(baseMinute);
            if (minute == CronEntryBase.RolledOver)
            {
                // We need to roll forward to the next hour.
                //
                minute = _minutes.First;
                baseHour++;
                // Don't need to worry about baseHour>23 case because
                //	that will roll off our list in the next step.
            }

            // Get the next hour value
            //
            int hour = _hours.Next(baseHour);
            if (hour == CronEntryBase.RolledOver)
            {
                // Roll forward to the next day.
                //
                minute = _minutes.First;
                hour = _hours.First;
                baseDay++;
                // Don't need to worry about baseDay>31 case because
                //	that will roll off our list in the next step.
            }
            else if (hour > baseHour)
            {
                // Original hour must not have been in the list.
                //	Reset the minutes.
                //
                minute = _minutes.First;
            }

            // Get the next day value.
            //
            int day = _days.Next(baseDay);
            if (day == CronEntryBase.RolledOver)
            {
                // Roll forward to the next month
                //
                minute = _minutes.First;
                hour = _hours.First;
                day = _days.First;
                baseMonth++;
                // Need to worry about rolling over to the next year here
                //	because we need to know the number of days in a month
                //	and that is year dependent (leap year).
                //
                if (baseMonth > 12)
                {
                    // Roll over to next year.
                    //
                    baseMonth = 1;
                    baseYear++;
                }
            }
            else if (day > baseDay)
            {
                // Original day no in the value list...reset.
                //
                minute = _minutes.First;
                hour = _hours.First;
            }
            while (day > DateTime.DaysInMonth(baseYear, baseMonth))
            {
                // Have a value for the day that is not a valid day
                //	in the current month. Move to the next month.
                //
                minute = _minutes.First;
                hour = _hours.First;
                day = _days.First;
                baseMonth++;
                // This original month could not be December because
                //	it can handle the maximum value of days (31). So
                //	we do not have to worry about baseMonth == 13 case.
            }

            // Get the next month value.
            //
            int month = _months.Next(baseMonth);
            if (month == CronEntryBase.RolledOver)
            {
                // Roll forward to the next year.
                //
                minute = _minutes.First;
                hour = _hours.First;
                day = _days.First;
                month = _months.First;
                baseYear++;
            }
            else if (month > baseMonth)
            {
                // Original month not in the value list...reset.
                //
                minute = _minutes.First;
                hour = _hours.First;
                day = _days.First;
            }
            while (day > DateTime.DaysInMonth(baseYear, month))
            {
                // Have a value for the day that is not a valid day
                //	in the current month. Move to the next month.
                //
                minute = _minutes.First;
                hour = _hours.First;
                day = _days.First;
                month = _months.Next(month + 1);
                if (month == CronEntryBase.RolledOver)
                {
                    // Roll forward to the next year.
                    //
                    minute = _minutes.First;
                    hour = _hours.First;
                    day = _days.First;
                    month = _months.First;
                    baseYear++;
                }
            }

            // Is the date / time we found beyond the end search contraint?
            //
            DateTime suggested = new DateTime(baseYear, month, day, hour, minute, 0, 0);
            if (suggested >= end)
            {
                return false;
            }

            // Does the date / time we found satisfy the day of the week contraint?
            //
            if (_daysOfWeek.Values.Contains((int)suggested.DayOfWeek))
            {
                // We have a good date.
                //
                next = suggested;
                return true;
            }

            // We need to recursively look for a date in the future. Because this
            //	search resulted in a day that does not satisfy the day of week 
            //	contraint, start the search on the next day.
            //
            return GetNext(new DateTime(baseYear, month, day, 23, 59, 0, 0), out next);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns a string that represents the current object. </summary>
        ///
        /// <remarks>   Don, 29-11-2012. </remarks>
        ///
        /// <returns>   A string that represents the current object. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3} {4}", _minutes.Expression, _hours.Expression, _days.Expression, _months.Expression, _daysOfWeek.Expression);
        }
    }
}
