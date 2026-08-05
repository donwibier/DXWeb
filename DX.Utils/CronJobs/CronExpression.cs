using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DX.Utils.CronJobs
{
    /// <summary>
    /// A "Cron-Expression" is a string comprised of 6 or 7 fields separated by white space. The 6 mandatory and 1 optional fields are as follows:
    /// Field Name	Allowed Values	Allowed Special Characters
    /// Seconds	0-59	, - * /
    /// Minutes	0-59	, - * /
    /// Hours	0-23	, - * /
    /// Day-of-month	1-31	, - * ? / L W C
    /// Month	1-12 or JAN-DEC	, - * /
    /// Day-of-Week	1-7 or SUN-SAT	, - * ? / L C #
    /// Year (Optional)	empty, 1970-2099	, - * /
    /// 
    /// Cron expression explanation:
    /// *    *    *    *    *  command to be executed
    /// ┬    ┬    ┬    ┬    ┬
    /// │    │    │    │    │
    /// │    │    │    │    │
    /// │    │    │    │    └───── day of week (0 - 6) (Sunday=0 )
    /// │    │    │    └────────── month (1 - 12)
    /// │    │    └─────────────── day of month (1 - 31)
    /// │    └──────────────────── hour (0 - 23)
    /// └───────────────────────── min (0 - 59)
    /// 
    /// The '*' character is used to specify all values. For example, "*" in the minute field means "every minute".
    /// 
    /// The '?' character is allowed for the day-of-month and day-of-week fields. It is used to specify 'no specific value'. 
    /// This is useful when you need to specify something in one of the two fileds, but not the other. See the examples below for clarification.
    /// 
    /// The '-' character is used to specify ranges For example "10-12" in the hour field means "the hours 10, 11 and 12".
    /// 
    /// The ',' character is used to specify additional values. For example "MON,WED,FRI" in the day-of-week field means "the days Monday, Wednesday, and Friday".
    /// 
    /// The '/' character is used to specify increments. For example "0/15" in the seconds field means "the seconds 0, 15, 30, and 45". 
    /// And "5/15" in the seconds field means "the seconds 5, 20, 35, and 50". Specifying '*' before the '/' is equivalent to specifying 0 is the value to start with. 
    /// Essentially, for each field in the expression, there is a set of numbers that can be turned on or off. For seconds and minutes, the numbers range from 0 to 59. 
    /// For hours 0 to 23, for days of the month 0 to 31, and for months 1 to 12. The "/" character simply helps you turn on every "nth" value in the given set. 
    /// Thus "7/6" in the month field only turns on month "7", it does NOT mean every 6th month, please note that subtlety.
    /// 
    /// The 'L' character is allowed for the day-of-month and day-of-week fields. This character is short-hand for "last", but it has different meaning in each of the two fields. 
    /// For example, the value "L" in the day-of-month field means "the last day of the month" - day 31 for January, day 28 for February on non-leap years. 
    /// If used in the day-of-week field by itself, it simply means "7" or "SAT". But if used in the day-of-week field after another value, 
    /// it means "the last xxx day of the month" - for example "6L" means "the last friday of the month". When using the 'L' option, it is important not to specify lists, 
    /// or ranges of values, as you'll get confusing results.
    /// 
    /// The 'W' character is allowed for the day-of-month field. This character is used to specify the weekday (Monday-Friday) nearest the given day. 
    /// As an example, if you were to specify "15W" as the value for the day-of-month field, the meaning is: "the nearest weekday to the 15th of the month". 
    /// So if the 15th is a Saturday, the trigger will fire on Friday the 14th. If the 15th is a Sunday, the trigger will fire on Monday the 16th. 
    /// If the 15th is a Tuesday, then it will fire on Tuesday the 15th. However if you specify "1W" as the value for day-of-month, and the 1st is a Saturday, 
    /// the trigger will fire on Monday the 3rd, as it will not 'jump' over the boundary of a month's days. 
    /// The 'W' character can only be specified when the day-of-month is a single day, not a range or list of days.
    /// 
    /// The 'L' and 'W' characters can also be combined for the day-of-month expression to yield 'LW', which translates to "last weekday of the month".
    /// 
    /// The '#' character is allowed for the day-of-week field. This character is used to specify "the nth" XXX day of the month. 
    /// For example, the value of "6#3" in the day-of-week field means the third Friday of the month (day 6 = Friday and "#3" = the 3rd one in the month). 
    /// Other examples: "2#1" = the first Monday of the month and "4#5" = the fifth Wednesday of the month. Note that if you specify "#5" and there is 
    /// not 5 of the given day-of-week in the month, then no firing will occur that month.
    /// 
    /// The 'C' character is allowed for the day-of-month and day-of-week fields. This character is short-hand for "calendar". 
    /// This means values are calculated against the associated calendar, if any. If no calendar is associated, then it is equivalent to having an all-inclusive calendar. 
    /// A value of "5C" in the day-of-month field means "the first day included by the calendar on or after the 5th". 
    /// A value of "1C" in the day-of-week field means "the first day included by the calendar on or after sunday".
    /// 
    /// The legal characters and the names of months and days of the week are not case sensitive.
    /// 
    /// Examples
    /// Expression	               Meaning
    /// ==================================================================================================
    /// "0 0 12 * * ?"             Fire at 12pm (noon) every day
    /// "0 15 10 ? * *"            Fire at 10:15am every day
    /// "0 15 10 * * ?"            Fire at 10:15am every day
    /// "0 15 10 * * ? *"          Fire at 10:15am every day
    /// "0 15 10 * * ? 2005"       Fire at 10:15am every day during the year 2005
    /// "0 * 14 * * ?"             Fire every minute starting at 2pm and ending at 2:59pm, every day
    /// "0 0/5 14 * * ?"           Fire every five minutes starting at 2pm and ending at 2:55pm, every day
    /// "0 0/5 14,18 * * ?"        Fire every 5 minutes starting at 2pm and ending at 2:55pm, 
    ///                            AND fire every 5 minutes starting at 6pm and ending at 6:55pm, every day
    /// "0 0-5 14 * * ?"           Fire every minute starting at 2pm and ending at 2:05pm, every day
    /// "0 10,44 14 ? 3 WED"       Fire at 2:10pm and at 2:44pm every Wednesday in the month of March
    /// "0 15 10 ? * MON-FRI"      Fire at 10:15am every Monday, Tuesday, Wednesday, Thursday and Friday
    /// "0 15 10 15 * ?"           Fire at 10:15am on the 15th day of every month
    /// "0 15 10 L * ?"            Fire at 10:15am on the last day of every month
    /// "0 15 10 ? * 6L"           Fire at 10:15am on the last Friday of every month
    /// "0 15 10 ? * 6L 2002-2005" Fire at 10:15am on every last friday of every month during the years 2002, 2003, 2004 and 2005
    /// "0 15 10 ? * 6#3"	       Fire at 10:15am on the third Friday of every month
    /// 
    /// Pay attention to the effects of '?' and '*' in the day-of-week and day-of-month fields!
    /// 
    /// Some more expressions by don@concepts2go.com (for carerix import)
    /// 0 0 0-7/1 ? * MON-FRI *		-> Monday till Friday from  0:00 till  7.00 every hour
    /// 0 0 0-7/2 ? * MON-FRI *		-> Monday till Friday from  0:00 till  7.00 every 2 hours
    /// 0 0 20-23/2 ? * MON-FRI *		-> Monday till Friday from 20:00 till 23.00 every 2 hours
    /// 0 0/30 8-18 ? * MON-FRI *		-> Monday till Friday from  8.00 till 18.00 every halve hour
    ///
    /// 0 0 0-23/2 ? * SAT,SUN *      -> Saturday and Sunday from 0.00 till 23.00 every 2 hours
    /// 0 0/1 * 1/1 * ? *               -> Every one minute
    /// At http://www.cronmaker.com/ you can create and test cron expressions
    /// </summary>
    public class CronExpression
    {
        public string Minutes { get; set; }
        public string Hours { get; set; }
        public string Days { get; set; }
        public string Months { get; set; }
        public string DaysOfWeek { get; set; }

        public CronExpression()
            : this("*", "*", "*", "*", "*")
        {

        }
        public CronExpression(string minutes, string hours, string days, string months, string daysOfWeek)
        {
            if (string.IsNullOrEmpty(minutes))
            {
                throw new ArgumentNullException("minutes");
            }
            if (string.IsNullOrEmpty(hours))
            {
                throw new ArgumentNullException("hours");
            }
            if (string.IsNullOrEmpty(days))
            {
                throw new ArgumentNullException("days");
            }
            if (string.IsNullOrEmpty(months))
            {
                throw new ArgumentNullException("months");
            }
            if (string.IsNullOrEmpty(daysOfWeek))
            {
                throw new ArgumentNullException("daysOfWeek");
            }
            Minutes = minutes;
            Hours = hours;
            Days = days;
            Months = months;
            DaysOfWeek = daysOfWeek;
        }

        public override string ToString()
        {
            return Minutes + " " + Hours + " " + Days + " " + Months + " " + DaysOfWeek;
        }
    }
}
