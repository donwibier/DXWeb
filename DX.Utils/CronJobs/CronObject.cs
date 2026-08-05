////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	Scheduler\Cron\CronObject.cs
//
// summary:	Implements the cron object class
////////////////////////////////////////////////////////////////////////////////////////////////////

using DX.Utils;
using DX.Utils.CronJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace DX.Utils.CronJobs
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Cron object data context. </summary>
    ///
    /// <remarks>   Don, 29-11-2012. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class CronObjectDataContext
    {
        private readonly List<CronSchedule> _cronSchedules;

        public CronObjectDataContext(string cronID, object dataContext, bool startOnMisfire, params CronSchedule[] cronSchedules)
        {
            CronID = cronID;
            Object = dataContext;
            _cronSchedules = new List<CronSchedule>(cronSchedules);
            LastTrigger = startOnMisfire ? DateTime.MinValue : DateTime.Now;
        }

        public CronObjectDataContext(string cronID, object dataContext, params CronSchedule[] cronSchedules)
            : this(cronID, dataContext, false, cronSchedules)
        {
    
        }

        public string CronID { get; private set; }
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the object. </summary>
        ///
        /// <value> The object. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public object Object { get; private set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the Date/Time of the last trigger. </summary>
        ///
        /// <value> The last trigger. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public DateTime LastTrigger { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the cron schedules. </summary>
        ///
        /// <value> The cron schedules. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public IEnumerable<CronSchedule> CronSchedules { get { return _cronSchedules; } }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Cron object. </summary>
    ///
    /// <remarks>   Don, 29-11-2012. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class CronObject
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Cron event. </summary>
        ///
        /// <remarks>   Don, 29-11-2012. </remarks>
        ///
        /// <param name="cronObject">   The cron object. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public delegate void CronEvent(CronObject cronObject);
        /// <summary>   Cron error event, raised when an OnCronTrigger handler throws. </summary>
        ///
        /// <param name="cronObject">   The cron object. </param>
        /// <param name="error">        The exception the handler threw. </param>
        public delegate void CronErrorEvent(CronObject cronObject, Exception error);
        /// <summary>   Event queue for all listeners interested in OnCronTrigger events. </summary>
        public event CronEvent OnCronTrigger;
        /// <summary>   Event queue for all listeners interested in OnStarted events. </summary>
        public event CronEvent OnStarted;
        /// <summary>   Event queue for all listeners interested in OnStopped events. </summary>
        public event CronEvent OnStopped;
        /// <summary>   Event queue for all listeners interested in OnThreadAbort events. </summary>
        public event CronEvent OnThreadAbort;
        /// <summary>   Event queue for all listeners interested in OnError events, raised when an
        ///             OnCronTrigger handler throws. The scheduler loop keeps running afterwards -
        ///             a misbehaving handler only skips that one firing, it does not stop the job. </summary>
        public event CronErrorEvent OnError;

        /// <summary>   Context for the cron object data. </summary>
        private readonly CronObjectDataContext _cronObjectDataContext;
        /// <summary>   The identifier. </summary>
        private readonly Guid _id = Guid.NewGuid();
        /// <summary>   The start stop lock. </summary>
        private readonly object _startStopLock = new object();
        /// <summary>   Cancellation source for the currently running scheduler loop. </summary>
        private CancellationTokenSource _cts;
        /// <summary>   The task running the scheduler loop. </summary>
        private Task _runTask;
        /// <summary>   true if this object is started. </summary>
        private bool _isStarted;
        /// <summary>   true if this object is stop requested. </summary>
        private bool _isStopRequested;
        /// <summary>   true if this object is currently executing it's trigger. </summary>
        private bool _isExecuting;
        /// <summary>   Date/Time of the next cron trigger. </summary>
        private DateTime _nextCronTrigger;
		/// <summary>   To set the Task Creation option (could be: AttachedToParent, LongRunning, None, PreferFairness). </summary>
		public TaskCreationOptions TaskOption = TaskCreationOptions.None;
		/// <summary>   When set to true this cronjob will fire once immediately when Start() is called, in addition to its regular schedule. </summary>
		public bool TriggerOnApplicationStart = false;
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the identifier. </summary>
        ///
        /// <value> The identifier. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public Guid Id { get { return _id; } }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the identifier of the cron. </summary>
        ///
        /// <value> The identifier of the cron. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public string CronID { get { return _cronObjectDataContext.CronID; } }
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the object. </summary>
        ///
        /// <value> The object. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public object Object { get { return _cronObjectDataContext.Object; } }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the Date/Time of the last tigger. </summary>
        ///
        /// <value> The last tigger. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public DateTime LastTigger { get { return _cronObjectDataContext.LastTrigger; } }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets a value indicating whether a stop is requested. </summary>
        ///
        /// <value> true if this job has to stop, false if not. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsStopRequested { get { return _isStopRequested; } }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets a value indicating whether this job has started. </summary>
        ///
        /// <value> true if this job has started, false if not. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsStarted { get { return _isStarted; } }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets a value indicating whether this object is executing. </summary>
        ///
        /// <value> true if this object is executing, false if not. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsExecuting { get { return _isExecuting; } }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Initializes a new instance of the <see cref="CronObject"/> class. </summary>
        ///
        /// <remarks>   Don, 29-11-2012. </remarks>
        ///
        /// <exception cref="ArgumentNullException">    Thrown when one or more required arguments are
        ///                                             null. </exception>
        /// <exception cref="ArgumentException">        Thrown when one or more arguments have
        ///                                             unsupported or illegal values. </exception>
        ///
        /// <param name="cronObjectDataContext">    The cron object data context. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public CronObject(CronObjectDataContext cronObjectDataContext)
        {
            if (cronObjectDataContext == null)
            {
                throw new ArgumentNullException("cronObjectDataContext");
            }
            //if (cronObjectDataContext.Object == null)
            //{
            //    throw new ArgumentException("cronObjectDataContext.Object");
            //}
            if (cronObjectDataContext.CronSchedules == null || cronObjectDataContext.CronSchedules.Count() == 0)
            {
                throw new ArgumentException("cronObjectDataContext.CronSchedules");
            }
            _cronObjectDataContext = cronObjectDataContext;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Starts this instance. </summary>
        ///
        /// <remarks>   Don, 29-11-2012. </remarks>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool Start()
        {
            lock (_startStopLock)
            {
                // Can't start if already started.
                //
                if (_isStarted)
                {
                    return false;
                }
                _isStarted = true;
                _isStopRequested = false;
                _cts = new CancellationTokenSource();

                // This is a long running process. Need to run on a thread
                //	outside the thread pool.
                //
                CancellationToken token = _cts.Token;
                _runTask = Task.Factory.StartNew(() => RunLoopAsync(token), token, TaskOption, TaskScheduler.Default).Unwrap();
            }

            // Raise the started event.
            //
            if (OnStarted != null)
            {
                OnStarted(this);
            }

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Stops this instance. </summary>
        ///
        /// <remarks>   Don, 29-11-2012. </remarks>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool Stop()
        {
            Task runTask;
            lock (_startStopLock)
            {
                // Can't stop if not started.
                //
                if (!_isStarted)
                {
                    return false;
                }
                _isStarted = false;
                _isStopRequested = true;
                runTask = _runTask;

                // Signal the loop to wake up early.
                //
                _cts.Cancel();

                // Wait for the loop to finish. Unlike the old Thread.Abort() based
                //	implementation, a still-running OnCronTrigger handler cannot be
                //	force-killed; we just stop waiting for it and let it finish on
                //	its own. Raise OnThreadAbort to signal that case to listeners.
                //
                if (runTask != null)
                {
                    try
                    {
                        if (!runTask.Wait(5000))
                        {
                            if (OnThreadAbort != null)
                            {
                                OnThreadAbort(this);
                            }
                        }
                    }
                    catch (AggregateException)
                    {
                        // Loop task ended due to cancellation or a faulted trigger handler;
                        //	either way it has finished, nothing more to wait for.
                    }
                }
            }


            // Raise the stopped event.
            //
            if (OnStopped != null)
            {
                OnStopped(this);
            }
            _isExecuting = false;
            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Cron object thread routine. </summary>
        ///
        /// <remarks>   Don, 29-11-2012. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task RunLoopAsync(CancellationToken token)
        {
            // Continue until stop is requested.
            //
            while (!token.IsCancellationRequested)
            {
                // Determine the next cron trigger
                //
                DetermineNextCronTrigger(out _nextCronTrigger);

                TimeSpan sleepSpan = _nextCronTrigger - DateTime.Now;
                if (sleepSpan.TotalMilliseconds < 0)
                {
                    // Next trigger is in the past. Trigger the right away.
                    //
                    sleepSpan = new TimeSpan(0, 0, 0, 0, 50);
                }
                else if (sleepSpan.TotalMilliseconds > int.MaxValue)
                {
                    // Task.Delay cannot wait longer than ~24.8 days at once (and
                    //	_nextCronTrigger may be DateTime.MaxValue if no schedule
                    //	ever matches again). Cap it; the loop will just recompute
                    //	and wait again.
                    //
                    sleepSpan = TimeSpan.FromMilliseconds(int.MaxValue);
                }

                // Wait here for the timespan or until a stop is requested.
                //
                bool triggerNow = TriggerOnApplicationStart;
                TriggerOnApplicationStart = false; // only force the trigger once so set it to false here!

                if (!triggerNow)
                {
                    try
                    {
                        await Task.Delay(sleepSpan, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // Stop() was called while we were waiting.
                        //
                        break;
                    }
                }

                if (token.IsCancellationRequested)
                {
                    break;
                }

                // Timespan is up (or an immediate trigger was requested)...raise the trigger event
                //
                if (OnCronTrigger != null)
                {
                    Log.Write(String.Format("CRON: Executing job [{0}]", CronID));
                    _isExecuting = true;
                    try
                    {
                        OnCronTrigger(this);
                    }
                    catch (Exception ex)
                    {
                        // A misbehaving handler must not permanently kill this job's
                        //	scheduler loop - log it, notify listeners, and keep going
                        //	so the next scheduled firing still happens.
                        //
                        Log.Exception(ex);
                        if (OnError != null)
                        {
                            OnError(this, ex);
                        }
                    }
                    finally
                    {
                        _isExecuting = false;
                    }
                }

                // Update the last trigger time.
                //
                _cronObjectDataContext.LastTrigger = DateTime.Now;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Determines the next cron trigger. </summary>
        ///
        /// <remarks>   Don, 29-11-2012. </remarks>
        ///
        /// <param name="nextTrigger">  The next trigger. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void DetermineNextCronTrigger(out DateTime nextTrigger)
        {
            nextTrigger = DateTime.MaxValue;
            foreach (CronSchedule cronSchedule in _cronObjectDataContext.CronSchedules)
            {
                DateTime thisTrigger;
                if (cronSchedule.GetNext(LastTigger, out thisTrigger))
                {
                    if (thisTrigger < nextTrigger)
                    {
                        nextTrigger = thisTrigger;
                    }
                }
            }
        }
    }

}
