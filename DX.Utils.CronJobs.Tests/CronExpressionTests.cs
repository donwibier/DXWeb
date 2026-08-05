using System;
using DX.Utils.CronJobs;
using Xunit;

namespace DX.Utils.CronJobs.Tests
{
    public class CronExpressionTests
    {
        [Fact]
        public void DefaultConstructor_IsAllWildcards()
        {
            var expr = new CronExpression();
            Assert.Equal("* * * * *", expr.ToString());
        }

        [Fact]
        public void Constructor_SetsAllFields()
        {
            var expr = new CronExpression("0", "12", "1", "1", "1");
            Assert.Equal("0", expr.Minutes);
            Assert.Equal("12", expr.Hours);
            Assert.Equal("1", expr.Days);
            Assert.Equal("1", expr.Months);
            Assert.Equal("1", expr.DaysOfWeek);
            Assert.Equal("0 12 1 1 1", expr.ToString());
        }

        [Theory]
        [InlineData(null, "*", "*", "*", "*")]
        [InlineData("*", null, "*", "*", "*")]
        [InlineData("*", "*", null, "*", "*")]
        [InlineData("*", "*", "*", null, "*")]
        [InlineData("*", "*", "*", "*", null)]
        [InlineData("", "*", "*", "*", "*")]
        public void Constructor_Throws_WhenAnyFieldIsNullOrEmpty(string minutes, string hours, string days, string months, string daysOfWeek)
        {
            Assert.Throws<ArgumentNullException>(() => new CronExpression(minutes, hours, days, months, daysOfWeek));
        }
    }
}
