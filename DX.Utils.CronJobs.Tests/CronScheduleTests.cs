using System;
using System.Linq;
using DX.Utils.CronJobs;
using Xunit;

namespace DX.Utils.CronJobs.Tests
{
    public class CronScheduleTests
    {
        [Fact]
        public void GetNext_DailyAtNoon_FindsNextOccurrenceSameDay()
        {
            var schedule = CronSchedule.Parse("0 12 * * *");

            Assert.True(schedule.GetNext(new DateTime(2026, 8, 5, 8, 0, 0), out DateTime next));

            Assert.Equal(new DateTime(2026, 8, 5, 12, 0, 0), next);
        }

        [Fact]
        public void GetNext_DailyAtNoon_RollsToNextDay_WhenStartIsAtTheTrigger()
        {
            // GetNext always looks strictly after "start" (it searches from start + 1 minute),
            // so asking from exactly 12:00 rolls to the next day's occurrence.
            var schedule = CronSchedule.Parse("0 12 * * *");

            Assert.True(schedule.GetNext(new DateTime(2026, 8, 5, 12, 0, 0), out DateTime next));

            Assert.Equal(new DateTime(2026, 8, 6, 12, 0, 0), next);
        }

        [Fact]
        public void GetNext_EveryFifteenMinutes_RollsToNextHour()
        {
            var schedule = CronSchedule.Parse("*/15 * * * *");

            Assert.True(schedule.GetNext(new DateTime(2026, 8, 5, 8, 7, 0), out DateTime next));

            Assert.Equal(new DateTime(2026, 8, 5, 8, 15, 0), next);
        }

        [Fact]
        public void GetNext_EveryFifteenMinutes_RollsToNextDay_AtEndOfDay()
        {
            var schedule = CronSchedule.Parse("*/15 * * * *");

            Assert.True(schedule.GetNext(new DateTime(2026, 8, 5, 23, 50, 0), out DateTime next));

            Assert.Equal(new DateTime(2026, 8, 6, 0, 0, 0), next);
        }

        [Fact]
        public void GetNext_DayOfWeekConstraint_SkipsAheadToMatchingWeekday()
        {
            // Days-of-week: 0=Sunday..6=Saturday. "1" = Monday.
            // 2026-08-05 is a Wednesday; the next Monday is 2026-08-10.
            var schedule = CronSchedule.Parse("0 9 * * 1");

            Assert.True(schedule.GetNext(new DateTime(2026, 8, 5, 8, 0, 0), out DateTime next));

            Assert.Equal(new DateTime(2026, 8, 10, 9, 0, 0), next);
            Assert.Equal(DayOfWeek.Monday, next.DayOfWeek);
        }

        [Fact]
        public void GetNext_DayOfWeekConstraint_CanFireLaterTheSameMatchingDay()
        {
            var schedule = CronSchedule.Parse("0 9 * * 1");

            Assert.True(schedule.GetNext(new DateTime(2026, 8, 10, 8, 0, 0), out DateTime next));

            Assert.Equal(new DateTime(2026, 8, 10, 9, 0, 0), next);
        }

        [Fact]
        public void GetNext_ReturnsFalse_WhenResultWouldBeAtOrAfterEnd()
        {
            var schedule = CronSchedule.Parse("0 12 * * *");

            bool found = schedule.GetNext(new DateTime(2026, 8, 5, 8, 0, 0), new DateTime(2026, 8, 5, 12, 0, 0), out DateTime next);

            Assert.False(found);
        }

        [Fact]
        public void GetAll_ReturnsEveryOccurrenceStrictlyBetweenStartAndEnd()
        {
            var schedule = CronSchedule.Parse("0 * * * *"); // top of every hour

            var all = schedule.GetAll(new DateTime(2026, 8, 5, 0, 0, 0), new DateTime(2026, 8, 5, 3, 0, 0));

            Assert.Equal(
                new[]
                {
                    new DateTime(2026, 8, 5, 1, 0, 0),
                    new DateTime(2026, 8, 5, 2, 0, 0)
                },
                all);
        }

        [Fact]
        public void Create_FromCronExpression_MatchesParseFromString()
        {
            var fromExpression = CronSchedule.Create(new CronExpression("0", "12", "*", "*", "*"));
            var fromString = CronSchedule.Parse("0 12 * * *");

            Assert.Equal(fromString.ToString(), fromExpression.ToString());
        }

        [Fact]
        public void Parse_Throws_ForWrongFieldCount()
        {
            Assert.Throws<ArgumentException>(() => CronSchedule.Parse("0 12 * *"));
        }

        [Fact]
        public void Create_Throws_ForNullCronExpression()
        {
            Assert.Throws<ArgumentNullException>(() => CronSchedule.Create(null));
        }
    }
}
