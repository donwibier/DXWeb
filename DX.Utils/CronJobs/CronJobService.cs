using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DX.Utils.CronJobs
{
    /// <summary>   Raised by <see cref="CronJobService"/> whenever a registered job's trigger
    ///             handler throws. The job itself keeps running - see
    ///             <see cref="CronObject.OnError"/>. </summary>
    ///
    /// <param name="jobId">    The id of the job whose handler threw. </param>
    /// <param name="error">    The exception the handler threw. </param>
    public delegate void CronJobErrorEvent(string jobId, Exception error);

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A singleton-friendly registry of <see cref="CronObject"/> instances, keyed by job id, that lets
    /// callers add and remove cron jobs at runtime instead of wiring every <see cref="CronObject"/> up
    /// by hand.
    ///
    /// Use <see cref="Default"/> for a process-wide singleton without DI, or register
    /// <see cref="CronJobService"/> itself as a singleton in an <c>IServiceCollection</c> (see
    /// <c>CronJobServiceExtensions.AddCronJobService</c> on non-.NET-Framework targets) and inject it
    /// wherever jobs need to be added.
    /// </summary>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public sealed class CronJobService : IDisposable
    {
        private static readonly Lazy<CronJobService> _default = new Lazy<CronJobService>(() => new CronJobService());

        /// <summary>   A process-wide default instance, for callers not using dependency injection. </summary>
        public static CronJobService Default => _default.Value;

        private readonly ConcurrentDictionary<string, CronObject> _jobs = new ConcurrentDictionary<string, CronObject>(StringComparer.OrdinalIgnoreCase);
        private bool _disposed;

        /// <summary>   Raised whenever any registered job's trigger handler throws. </summary>
        public event CronJobErrorEvent OnJobError;

        /// <summary>   The ids of all currently registered jobs. </summary>
        public IReadOnlyCollection<string> JobIds => _jobs.Keys.ToArray();

        /// <summary>   The number of currently registered jobs. </summary>
        public int Count => _jobs.Count;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Registers and starts a new cron job parsed from a single 5-field cron
        ///             expression string (minutes hours days months daysOfWeek). </summary>
        ///
        /// <param name="jobId">            A unique id for the job. Registering a second job with the
        ///                                 same id (case-insensitively) throws. </param>
        /// <param name="cronExpression">   A 5-field cron expression, eg. "0 12 * * *". </param>
        /// <param name="onTrigger">        Invoked on every firing. </param>
        /// <param name="startImmediately"> Whether to start the job right away (the default) or leave
        ///                                 it registered-but-stopped until <see cref="StartJob"/> or
        ///                                 <see cref="StartAll"/> is called. </param>
        /// <param name="triggerOnRegister">When true, the job also fires once immediately when
        ///                                 started, in addition to its regular schedule. </param>
        ///
        /// <returns>   The <see cref="CronObject"/> backing the new job. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public CronObject AddJob(string jobId, string cronExpression, Action<CronObject> onTrigger, bool startImmediately = true, bool triggerOnRegister = false)
        {
            if (string.IsNullOrEmpty(cronExpression))
            {
                throw new ArgumentNullException(nameof(cronExpression));
            }
            return AddJob(jobId, onTrigger, startImmediately, triggerOnRegister, CronSchedule.Parse(cronExpression));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Registers and starts a new cron job from a <see cref="CronExpression"/> (eg. one
        ///             built with <see cref="CronBuilder"/>). </summary>
        ///
        /// <param name="jobId">            A unique id for the job. Registering a second job with the
        ///                                 same id (case-insensitively) throws. </param>
        /// <param name="cronExpression">   The cron expression. </param>
        /// <param name="onTrigger">        Invoked on every firing. </param>
        /// <param name="startImmediately"> Whether to start the job right away (the default) or leave
        ///                                 it registered-but-stopped until <see cref="StartJob"/> or
        ///                                 <see cref="StartAll"/> is called. </param>
        /// <param name="triggerOnRegister">When true, the job also fires once immediately when
        ///                                 started, in addition to its regular schedule. </param>
        ///
        /// <returns>   The <see cref="CronObject"/> backing the new job. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public CronObject AddJob(string jobId, CronExpression cronExpression, Action<CronObject> onTrigger, bool startImmediately = true, bool triggerOnRegister = false)
        {
            if (cronExpression == null)
            {
                throw new ArgumentNullException(nameof(cronExpression));
            }
            return AddJob(jobId, onTrigger, startImmediately, triggerOnRegister, CronSchedule.Create(cronExpression));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Registers and starts a new cron job driven by one or more schedules (the job
        ///             fires when any of them match - eg. "9am on weekdays" OR "noon on weekends" as
        ///             two separate <see cref="CronSchedule"/> instances). </summary>
        ///
        /// <param name="jobId">            A unique id for the job. Registering a second job with the
        ///                                 same id (case-insensitively) throws. </param>
        /// <param name="onTrigger">        Invoked on every firing. </param>
        /// <param name="startImmediately"> Whether to start the job right away (the default) or leave
        ///                                 it registered-but-stopped until <see cref="StartJob"/> or
        ///                                 <see cref="StartAll"/> is called. </param>
        /// <param name="triggerOnRegister">When true, the job also fires once immediately when
        ///                                 started, in addition to its regular schedule. </param>
        /// <param name="schedules">        One or more schedules for the job. </param>
        ///
        /// <returns>   The <see cref="CronObject"/> backing the new job. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public CronObject AddJob(string jobId, Action<CronObject> onTrigger, bool startImmediately, bool triggerOnRegister, params CronSchedule[] schedules)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                throw new ArgumentNullException(nameof(jobId));
            }
            if (onTrigger == null)
            {
                throw new ArgumentNullException(nameof(onTrigger));
            }
            if (schedules == null || schedules.Length == 0)
            {
                throw new ArgumentException("At least one CronSchedule is required.", nameof(schedules));
            }
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(CronJobService));
            }

            var context = new CronObjectDataContext(jobId, null, schedules);
            var job = new CronObject(context) { TriggerOnApplicationStart = triggerOnRegister };
            job.OnCronTrigger += o => onTrigger(o);
            job.OnError += (o, ex) => OnJobError?.Invoke(jobId, ex);

            if (!_jobs.TryAdd(jobId, job))
            {
                throw new InvalidOperationException($"A cron job with id '{jobId}' is already registered.");
            }

            if (startImmediately)
            {
                job.Start();
            }
            return job;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Stops (if running) and removes a job. </summary>
        ///
        /// <param name="jobId">    The id of the job to remove. </param>
        ///
        /// <returns>   true if a job with that id was found and removed, false otherwise. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool RemoveJob(string jobId)
        {
            if (jobId != null && _jobs.TryRemove(jobId, out CronObject job))
            {
                job.Stop();
                return true;
            }
            return false;
        }

        /// <summary>   Looks up a registered job by id. </summary>
        public bool TryGetJob(string jobId, out CronObject job)
        {
            return _jobs.TryGetValue(jobId ?? string.Empty, out job);
        }

        /// <summary>   Starts a single registered job that isn't currently running. </summary>
        public bool StartJob(string jobId)
        {
            return _jobs.TryGetValue(jobId ?? string.Empty, out CronObject job) && job.Start();
        }

        /// <summary>   Stops a single registered job without removing it. </summary>
        public bool StopJob(string jobId)
        {
            return _jobs.TryGetValue(jobId ?? string.Empty, out CronObject job) && job.Stop();
        }

        /// <summary>   Starts every registered job that isn't already running. </summary>
        public void StartAll()
        {
            foreach (CronObject job in _jobs.Values)
            {
                if (!job.IsStarted)
                {
                    job.Start();
                }
            }
        }

        /// <summary>   Stops every registered job, without removing them. </summary>
        public void StopAll()
        {
            foreach (CronObject job in _jobs.Values)
            {
                if (job.IsStarted)
                {
                    job.Stop();
                }
            }
        }

        /// <summary>   Stops and removes every registered job. </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            StopAll();
            _jobs.Clear();
        }
    }
}
