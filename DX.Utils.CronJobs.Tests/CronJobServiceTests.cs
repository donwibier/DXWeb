using System;
using System.Threading;
using System.Threading.Tasks;
using DX.Utils.CronJobs;
using Xunit;

namespace DX.Utils.CronJobs.Tests
{
    // A schedule that (for all practical test purposes) will never naturally fire within a test run:
    // only at 00:00 on Jan 1st, and only in years where that day is also a Sunday.
    file static class NeverSoon
    {
        public static CronSchedule Schedule => CronSchedule.Parse("0 0 1 1 0");
        public const string Expression = "0 0 1 1 0";
    }

    public class CronJobServiceTests : IDisposable
    {
        private readonly CronJobService _service = new CronJobService();

        public void Dispose() => _service.Dispose();

        [Fact]
        public void AddJob_FromExpressionString_RegistersAndStartsTheJob()
        {
            var job = _service.AddJob("job-1", NeverSoon.Expression, _ => { });

            Assert.Equal(1, _service.Count);
            Assert.Contains("job-1", _service.JobIds);
            Assert.True(job.IsStarted);
        }

        [Fact]
        public void AddJob_FromCronExpressionObject_RegistersAndStartsTheJob()
        {
            var expression = CronBuilder.CreateDailyTrigger(3);

            var job = _service.AddJob("job-2", expression, _ => { });

            Assert.True(job.IsStarted);
            Assert.Equal("0 3 * * *", expression.ToString());
        }

        [Fact]
        public void AddJob_WithStartImmediatelyFalse_RegistersWithoutStarting()
        {
            var job = _service.AddJob("job-3", NeverSoon.Expression, _ => { }, startImmediately: false);

            Assert.False(job.IsStarted);
            Assert.True(_service.StartJob("job-3"));
            Assert.True(job.IsStarted);
        }

        [Fact]
        public void AddJob_DuplicateId_Throws()
        {
            _service.AddJob("dup", NeverSoon.Expression, _ => { });

            Assert.Throws<InvalidOperationException>(() => _service.AddJob("dup", NeverSoon.Expression, _ => { }));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void AddJob_InvalidId_Throws(string jobId)
        {
            Assert.ThrowsAny<ArgumentException>(() => _service.AddJob(jobId, NeverSoon.Expression, _ => { }));
        }

        [Fact]
        public void AddJob_NullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _service.AddJob("job-null-handler", NeverSoon.Expression, null));
        }

        [Fact]
        public void AddJob_NoSchedules_Throws()
        {
            Assert.Throws<ArgumentException>(() => _service.AddJob("job-no-schedule", _ => { }, true, false));
        }

        [Fact]
        public void RemoveJob_StopsAndForgetsTheJob()
        {
            var job = _service.AddJob("job-4", NeverSoon.Expression, _ => { });

            Assert.True(_service.RemoveJob("job-4"));

            Assert.Equal(0, _service.Count);
            Assert.False(job.IsStarted);
            Assert.False(_service.TryGetJob("job-4", out _));
        }

        [Fact]
        public void RemoveJob_UnknownId_ReturnsFalse()
        {
            Assert.False(_service.RemoveJob("does-not-exist"));
        }

        [Fact]
        public void StopAll_StopsEveryRegisteredJob_ButKeepsThemRegistered()
        {
            _service.AddJob("job-a", NeverSoon.Expression, _ => { });
            _service.AddJob("job-b", NeverSoon.Expression, _ => { });

            _service.StopAll();

            Assert.Equal(2, _service.Count);
            Assert.True(_service.TryGetJob("job-a", out CronObject a));
            Assert.True(_service.TryGetJob("job-b", out CronObject b));
            Assert.False(a.IsStarted);
            Assert.False(b.IsStarted);
        }

        [Fact]
        public void StartAll_StartsEveryRegisteredJob()
        {
            _service.AddJob("job-c", NeverSoon.Expression, _ => { }, startImmediately: false);
            _service.AddJob("job-d", NeverSoon.Expression, _ => { }, startImmediately: false);

            _service.StartAll();

            Assert.True(_service.TryGetJob("job-c", out CronObject c));
            Assert.True(_service.TryGetJob("job-d", out CronObject d));
            Assert.True(c.IsStarted);
            Assert.True(d.IsStarted);
        }

        [Fact]
        public async Task TriggerOnRegister_FiresTheHandlerImmediately()
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            _service.AddJob("job-immediate", NeverSoon.Expression, _ => tcs.TrySetResult(true), triggerOnRegister: true);

            var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));
            Assert.Same(tcs.Task, completed);
        }

        [Fact]
        public async Task OnJobError_IsRaised_AndJobKeepsRunning_WhenHandlerThrows()
        {
            var errorTcs = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
            string erroredJobId = null;
            _service.OnJobError += (jobId, ex) =>
            {
                erroredJobId = jobId;
                errorTcs.TrySetResult(ex);
            };

            var job = _service.AddJob("job-throws", NeverSoon.Expression, _ => throw new InvalidOperationException("boom"), triggerOnRegister: true);

            var completed = await Task.WhenAny(errorTcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));

            Assert.Same(errorTcs.Task, completed);
            Assert.Equal("job-throws", erroredJobId);
            Assert.IsType<InvalidOperationException>(await errorTcs.Task);
            Assert.True(job.IsStarted); // the job's loop is still running, not dead
        }

        [Fact]
        public void Dispose_StopsAndClearsAllJobs()
        {
            var service = new CronJobService();
            var job = service.AddJob("job-disposed", NeverSoon.Expression, _ => { });

            service.Dispose();

            Assert.Equal(0, service.Count);
            Assert.False(job.IsStarted);
        }

        [Fact]
        public void AddJob_AfterDispose_Throws()
        {
            var service = new CronJobService();
            service.Dispose();

            Assert.Throws<ObjectDisposedException>(() => service.AddJob("job-after-dispose", NeverSoon.Expression, _ => { }));
        }

        [Fact]
        public void Default_ReturnsTheSameInstanceEveryTime()
        {
            Assert.Same(CronJobService.Default, CronJobService.Default);
        }
    }
}
