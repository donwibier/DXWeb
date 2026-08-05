using System;
using System.Threading;
using System.Threading.Tasks;
using DX.Utils.CronJobs;
using Xunit;

namespace DX.Utils.CronJobs.Tests
{
    public class CronObjectTests
    {
        // A schedule that (for all practical test purposes) will never naturally fire:
        // only at 00:00 on Jan 1st, and only if that day is also a Sunday.
        private static CronSchedule NeverSoonSchedule()
            => CronSchedule.Parse("0", "0", "1", "1", "0");

        private static CronObjectDataContext MakeContext(string id, params CronSchedule[] schedules)
            => new CronObjectDataContext(id, dataContext: null, schedules);

        [Fact]
        public void Constructor_Throws_WhenDataContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CronObject(null));
        }

        [Fact]
        public void Constructor_Throws_WhenNoSchedulesProvided()
        {
            var context = new CronObjectDataContext("job", null);
            Assert.Throws<ArgumentException>(() => new CronObject(context));
        }

        [Fact]
        public void Start_SetsIsStarted_AndRaisesOnStarted()
        {
            var cron = new CronObject(MakeContext("job-start", NeverSoonSchedule()));
            var started = new ManualResetEventSlim(false);
            cron.OnStarted += _ => started.Set();

            try
            {
                Assert.True(cron.Start());
                Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
                Assert.True(cron.IsStarted);
            }
            finally
            {
                cron.Stop();
            }
        }

        [Fact]
        public void Start_ReturnsFalse_WhenAlreadyStarted()
        {
            var cron = new CronObject(MakeContext("job-double-start", NeverSoonSchedule()));
            try
            {
                Assert.True(cron.Start());
                Assert.False(cron.Start());
            }
            finally
            {
                cron.Stop();
            }
        }

        [Fact]
        public void Stop_ReturnsFalse_WhenNotStarted()
        {
            var cron = new CronObject(MakeContext("job-not-started", NeverSoonSchedule()));
            Assert.False(cron.Stop());
        }

        [Fact]
        public void Stop_ClearsIsStarted_AndRaisesOnStopped()
        {
            var cron = new CronObject(MakeContext("job-stop", NeverSoonSchedule()));
            var stopped = new ManualResetEventSlim(false);
            cron.OnStopped += _ => stopped.Set();

            cron.Start();
            Assert.True(cron.Stop());

            Assert.True(stopped.Wait(TimeSpan.FromSeconds(2)));
            Assert.False(cron.IsStarted);
        }

        [Fact]
        public async Task TriggerOnApplicationStart_FiresImmediately_EvenWithADistantSchedule()
        {
            var cron = new CronObject(MakeContext("job-fire-now", NeverSoonSchedule()))
            {
                TriggerOnApplicationStart = true
            };
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            cron.OnCronTrigger += _ => tcs.TrySetResult(true);

            try
            {
                cron.Start();

                var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));

                Assert.Same(tcs.Task, completed);
                Assert.True(await tcs.Task);
            }
            finally
            {
                cron.Stop();
            }
        }

        [Fact]
        public async Task IsExecuting_IsTrueWhileHandlerRuns_AndStopWaitsForItToFinish()
        {
            var handlerStarted = new ManualResetEventSlim(false);
            var releaseHandler = new ManualResetEventSlim(false);
            var cron = new CronObject(MakeContext("job-executing", NeverSoonSchedule()))
            {
                TriggerOnApplicationStart = true
            };
            cron.OnCronTrigger += _ =>
            {
                handlerStarted.Set();
                releaseHandler.Wait(TimeSpan.FromSeconds(5));
            };

            cron.Start();
            Assert.True(handlerStarted.Wait(TimeSpan.FromSeconds(5)));
            Assert.True(cron.IsExecuting);

            // Release the handler on a background thread, mid-Stop(), so Stop() observes
            // it running and then waits for it to complete gracefully (no Thread.Abort).
            var stopTask = Task.Run(() => cron.Stop());
            await Task.Delay(200);
            releaseHandler.Set();

            Assert.True(await stopTask);
            Assert.False(cron.IsExecuting);
        }

        [Fact]
        public async Task RealTimeScheduling_ActuallyFiresAtTheNextMinuteBoundary()
        {
            // End-to-end check that the scheduler fires against the real wall clock,
            // not just via the TriggerOnApplicationStart shortcut used by the other tests.
            var schedule = CronSchedule.Create(CronBuilder.CreateMinutelyTrigger());
            var cron = new CronObject(MakeContext("job-realtime", schedule));
            var tcs = new TaskCompletionSource<DateTime>(TaskCreationOptions.RunContinuationsAsynchronously);
            cron.OnCronTrigger += o => tcs.TrySetResult(DateTime.Now);

            var startedAt = DateTime.Now;
            try
            {
                cron.Start();

                var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(65)));

                Assert.Same(tcs.Task, completed);
                var firedAt = await tcs.Task;
                Assert.True(firedAt - startedAt <= TimeSpan.FromSeconds(61),
                    $"Expected a minutely trigger within 61s, started={startedAt:O} fired={firedAt:O}");
            }
            finally
            {
                cron.Stop();
            }
        }
    }
}
