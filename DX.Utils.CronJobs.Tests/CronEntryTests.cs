using System.Linq;
using DX.Utils.CronJobs;
using Xunit;

namespace DX.Utils.CronJobs.Tests
{
    public class CronEntryTests
    {
        [Fact]
        public void SingleValue_ParsesToOneEntry()
        {
            var entry = new MinutesCronEntry("5");
            Assert.Equal(new[] { 5 }, entry.Values);
        }

        [Fact]
        public void CommaList_ParsesEachEntry()
        {
            var entry = new MinutesCronEntry("2,3,9");
            Assert.Equal(new[] { 2, 3, 9 }, entry.Values);
        }

        [Fact]
        public void Range_ExpandsToAllValuesInclusive()
        {
            var entry = new MinutesCronEntry("1-5");
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, entry.Values);
        }

        [Fact]
        public void RangeWithInterval_StepsThroughRange()
        {
            var entry = new MinutesCronEntry("1-10/3");
            Assert.Equal(new[] { 1, 4, 7, 10 }, entry.Values);
        }

        [Fact]
        public void Wildcard_CoversWholeMinAndMax()
        {
            var entry = new HoursCronEntry("*");
            Assert.Equal(24, entry.Values.Count);
            Assert.Equal(0, entry.Values.First());
            Assert.Equal(23, entry.Values.Last());
        }

        [Fact]
        public void WildcardWithInterval_StepsFromMin()
        {
            var entry = new MinutesCronEntry("*/15");
            Assert.Equal(new[] { 0, 15, 30, 45 }, entry.Values);
        }

        [Fact]
        public void SingleValueWithInterval_StepsFromThatValueToMax()
        {
            // '2/5' -> 2,7,12,17,...<=59
            var entry = new MinutesCronEntry("2/5");
            Assert.Equal(2, entry.Values.First());
            Assert.Equal(57, entry.Values.Last());
            Assert.All(entry.Values, v => Assert.Equal(0, (v - 2) % 5));
        }

        [Fact]
        public void MixedCommaRangeAndInterval_CombinesWithoutDuplicates()
        {
            var entry = new MinutesCronEntry("2,3,4-10/2");
            Assert.Equal(new[] { 2, 3, 4, 6, 8, 10 }, entry.Values);
        }

        [Fact]
        public void First_ReturnsSmallestParsedValue()
        {
            var entry = new MinutesCronEntry("30,5,45");
            Assert.Equal(30, entry.First); // First reflects parse order, not sorted order
        }

        [Theory]
        [InlineData(10, 10)]   // exact match returned as-is
        [InlineData(15, 20)]   // rounds up to the next value in the list
        [InlineData(5, 10)]    // start below the smallest value
        public void Next_ReturnsFirstValueAtOrAfterStart(int start, int expected)
        {
            var entry = new MinutesCronEntry("10,20,30");
            Assert.Equal(expected, entry.Next(start));
        }

        [Fact]
        public void Next_ReturnsRolledOver_WhenStartIsPastAllValues()
        {
            var entry = new MinutesCronEntry("10,20,30");
            Assert.Equal(CronEntryBase.RolledOver, entry.Next(31));
        }

        [Fact]
        public void EmptyExpression_Throws()
        {
            Assert.Throws<CronEntryException>(() => new MinutesCronEntry(""));
        }

        [Fact]
        public void ValueAboveMax_Throws()
        {
            Assert.Throws<CronEntryException>(() => new MinutesCronEntry("60"));
        }

        [Fact]
        public void ValueBelowMin_Throws()
        {
            Assert.Throws<CronEntryException>(() => new DaysCronEntry("0"));
        }

        [Fact]
        public void ReversedRange_Throws()
        {
            Assert.Throws<CronEntryException>(() => new HoursCronEntry("5-3"));
        }

        [Fact]
        public void NonNumericEntry_Throws()
        {
            Assert.Throws<CronEntryException>(() => new HoursCronEntry("x"));
        }

        [Fact]
        public void DaysOfWeekCronEntry_AllowsSundayToSaturdayAsZeroToSix()
        {
            var entry = new DaysOfWeekCronEntry("*");
            Assert.Equal(0, entry.Values.First());
            Assert.Equal(6, entry.Values.Last());
        }
    }
}
