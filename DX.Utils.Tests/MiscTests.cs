using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using Xunit;

namespace DX.Utils.Tests
{
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void DateMin_StripsTime()
        {
            var d = new DateTime(2024, 2, 29, 23, 59, 59, 999);
            Assert.Equal(new DateTime(2024, 2, 29), d.DateMin());
        }

        [Fact]
        public void DateMax_IsNextMidnight()
        {
            Assert.Equal(new DateTime(2024, 3, 1), new DateTime(2024, 2, 29, 10, 0, 0).DateMax());
            Assert.Equal(new DateTime(2025, 1, 1), new DateTime(2024, 12, 31, 0, 0, 0).DateMax());
        }

        [Fact]
        public void DateMax_OfMaxValue_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DateTime.MaxValue.DateMax());
        }

        [Fact]
        public void DateMin_OfMinValue()
        {
            Assert.Equal(DateTime.MinValue, DateTime.MinValue.DateMin());
        }

        [Fact(Skip = "BUG: DateMin builds 'new DateTime(y, m, d, 0, 0, 0)' and drops DateTimeKind, so a UTC date comes back as Unspecified (date.Date would preserve it)")]
        public void DateMinMax_PreserveKind()
        {
            var utc = new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Utc);
            Assert.Equal(DateTimeKind.Utc, utc.DateMin().Kind);
            Assert.Equal(DateTimeKind.Utc, utc.DateMax().Kind);
        }
    }

    public class ExceptionExtensionsTests
    {
        [Fact]
        public void NoInner_ReturnsSelf()
        {
            var e = new Exception("x");
            Assert.Same(e, e.GetInnerException());
        }

        [Fact]
        public void ReturnsDeepestInner()
        {
            var deepest = new ArgumentException("deep");
            var e = new Exception("a", new InvalidOperationException("b", deepest));
            Assert.Same(deepest, e.GetInnerException());
        }

        [Fact]
        public void AggregateException_FollowsInnerExceptionProperty()
        {
            var first = new Exception("first");
            var agg = new AggregateException(first, new Exception("second"));
            Assert.Same(first, agg.GetInnerException());
        }

        [Fact]
        public void Null_Throws()
        {
            Assert.Throws<NullReferenceException>(() => ((Exception)null).GetInnerException());
        }
    }

    // Trace.Listeners is process-global; keep log tests in one non-parallel collection.
    [CollectionDefinition("Trace", DisableParallelization = true)]
    public class TraceCollection { }

    internal sealed class CapturingListener : TraceListener
    {
        private readonly StringBuilder _sb = new StringBuilder();
        public override void Write(string message) { lock (_sb) _sb.Append(message); }
        public override void WriteLine(string message) { lock (_sb) _sb.AppendLine(message); }
        public string Text { get { lock (_sb) return _sb.ToString(); } }
    }

    [Collection("Trace")]
    public class LogTests
    {
        [Fact]
        public void Write_AppendsToThreadLog()
        {
            Log.ClearLog();
            Log.Write("hello-log", false);
            Assert.Contains("hello-log", Log.GetLog());
            Log.ClearLog();
            Assert.Equal(string.Empty, Log.GetLog());
        }

        [Fact]
        public void Log_IsPerThread()
        {
            Log.ClearLog();
            Log.Write("main-thread-entry", false);
            string other = null;
            var t = new Thread(() => { Log.Write("other-thread-entry", false); other = Log.GetLog(); });
            t.Start();
            t.Join();
            Assert.DoesNotContain("main-thread-entry", other);
            Assert.Contains("other-thread-entry", other);
            Assert.DoesNotContain("other-thread-entry", Log.GetLog());
        }

        [Fact]
        public void GetLog_OnFreshThread_IsEmpty()
        {
            string log = "not set";
            var t = new Thread(() => log = Log.GetLog());
            t.Start();
            t.Join();
            Assert.Equal(string.Empty, log);
        }

        [Fact]
        public void TraceWrite_GoesToTraceListeners()
        {
            var l = new CapturingListener();
            Trace.Listeners.Add(l);
            try
            {
                Log.Write("traced-" + Guid.NewGuid());
                Assert.Contains("traced-", l.Text);
            }
            finally
            {
                Trace.Listeners.Remove(l);
            }
        }

        [Fact(Skip = "BUG: Log timestamp uses 12-hour 'hh' without AM/PM ('{0: dd-MM-yyyy hh:mm:ss}'), so 17:30 is logged as 05:30")]
        public void Write_UsesTwentyFourHourClock()
        {
            Log.ClearLog();
            var before = DateTime.Now;
            Log.Write("clock", false);
            var after = DateTime.Now;
            var log = Log.GetLog();
            // Only observable in the afternoon (hh == HH before noon), but the assertion is always correct.
            Assert.True(log.Contains(before.ToString(" dd-MM-yyyy HH:mm")) || log.Contains(after.ToString(" dd-MM-yyyy HH:mm")), log);
        }

        [Fact(Skip = "BUG: Log.Write passes logType.GetType() (=RuntimeType) and the Type instead of the logged object to DoLog, so traces read '[RuntimeType/System.InvalidOperationException]' and the exception message is lost")]
        public void Exception_TraceContainsTypeNameAndMessage()
        {
            var l = new CapturingListener();
            Trace.Listeners.Add(l);
            try
            {
                Log.Exception(new InvalidOperationException("boom-message"));
                Assert.DoesNotContain("RuntimeType", l.Text);
                Assert.Contains("[InvalidOperationException/boom-message]", l.Text);
            }
            finally
            {
                Trace.Listeners.Remove(l);
            }
        }

        [Fact]
        public void Exception_DoesNotThrow()
        {
            Log.Exception(new Exception("x"), Log.LogType.Warning);
            Log.Write("y", false, Log.LogType.Error, null);
        }

        [Fact]
        public void Write_IsCultureIndependentEnough_NotThrowingUnderArabic()
        {
            using (new CultureScope("ar-SA"))
            {
                Log.ClearLog();
                Log.Write("ar", false);
                Assert.Contains("ar", Log.GetLog());
            }
        }
    }

    public class UtilsConfigTests
    {
        [Fact]
        public void Current_WithoutConfigSection_IsDefaultInstance()
        {
            Assert.NotNull(UtilsConfig.Current);
            Assert.Null(UtilsConfig.Current.LoggerType);
            Assert.Same(UtilsConfig.Current, UtilsConfig.Current);
        }
    }
}
