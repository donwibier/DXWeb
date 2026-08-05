using DX.Utils.CronJobs;
using Xunit;

namespace DX.Utils.CronJobs.Tests
{
    public class CronBuilderTests
    {
        [Fact]
        public void CreateMinutelyTrigger_IsAllWildcards()
        {
            Assert.Equal("* * * * *", CronBuilder.CreateMinutelyTrigger().ToString());
        }

        [Fact]
        public void CreateHourlyTrigger_Default_FiresOnTheHour()
        {
            Assert.Equal("0 * * * *", CronBuilder.CreateHourlyTrigger().ToString());
        }

        [Fact]
        public void CreateHourlyTrigger_WithMinute_FiresAtThatMinuteEveryHour()
        {
            Assert.Equal("5 * * * *", CronBuilder.CreateHourlyTrigger(5).ToString());
        }

        [Fact]
        public void CreateHourlyTrigger_WithMinuteRangeAndInterval_BuildsRangeExpression()
        {
            Assert.Equal("0-30/10 * * * *", CronBuilder.CreateHourlyTrigger(0, 30, 10).ToString());
        }

        [Fact]
        public void CreateDailyTrigger_Default_FiresAtMidnight()
        {
            Assert.Equal("0 0 * * *", CronBuilder.CreateDailyTrigger().ToString());
        }

        [Fact]
        public void CreateDailyTrigger_WithHour_FiresAtThatHour()
        {
            Assert.Equal("0 6 * * *", CronBuilder.CreateDailyTrigger(6).ToString());
        }

        [Fact]
        public void CreateDailyOnlyWeekDayTrigger_FiltersToMondayThroughFriday()
        {
            Assert.Equal("0 8 * * 1,2,3,4,5", CronBuilder.CreateDailyOnlyWeekDayTrigger(8).ToString());
        }

        [Fact]
        public void CreateDailyOnlyWeekEndTrigger_FiltersToSaturdayAndSunday()
        {
            Assert.Equal("0 10 * * 0,6", CronBuilder.CreateDailyOnlyWeekEndTrigger(10).ToString());
        }

        [Fact]
        public void CreateMonthlyTrigger_Default_FiresOnDayZeroAtMidnight()
        {
            Assert.Equal("0 0 0 * *", CronBuilder.CreateMonthlyTrigger().ToString());
        }

        [Fact]
        public void CreateMonthlyTrigger_WithDay_FiresOnThatDay()
        {
            Assert.Equal("0 0 15 * *", CronBuilder.CreateMonthlyTrigger(15).ToString());
        }

        [Fact]
        public void CreateMonthlyTriggerOnHour_SetsBothDayAndHour()
        {
            Assert.Equal("0 6 15 * *", CronBuilder.CreateMonthlyTriggerOnHour(15, 6).ToString());
        }

        [Fact]
        public void CreateYearlyTrigger_Default_FiresInMonthZero()
        {
            Assert.Equal("0 0 0 0 *", CronBuilder.CreateYearlyTrigger().ToString());
        }

        [Fact]
        public void BuilderOutput_IsParseableByCronSchedule()
        {
            var expression = CronBuilder.CreateDailyOnlyWeekDayTrigger(9);

            var schedule = CronSchedule.Create(expression);

            Assert.Equal(expression.ToString(), schedule.ToString());
        }
    }
}
