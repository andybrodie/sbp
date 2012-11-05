using System;
using System.Windows.Threading;
using Locima.SlidingBlock.Common;

namespace Locima.SlidingBlock.Model
{

    /// <summary>
    /// The MVVM model for the stopwatch display (the timer counting how long a player has been working on a level)
    /// </summary>
    /// <remarks>
    /// <para>To maintain accuracy the stopwatch maintains a <see cref="BaseElapsedTime"/> which is the total amount of time
    /// captured by the stopwatch up to the last <see cref="Start"/>.  When the stopwatch is started, we record the exact
    /// time that the stopwatch was started in <see cref="_lastStartTime"/>.  Any client wanting to know the total elapsed time can use <see cref="ElapsedTime"/>.
    /// Any client wanting to know the elapsed time since last start can use <see cref="ElapsedTimeSinceLastStart"/>
    /// When the stopwatch <see cref="Stop"/> method is called, we update the <see cref="BaseElapsedTime"/> with the
    /// <see cref="TimeSpan"/> between the <see cref="Start"/> and <see cref="Stop"/> invocations.</para>
    /// <para>This makes use of the threading model for Windows Phone using a <see cref="DispatcherTimer"/></para></remarks>
    public class StopwatchModel : IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private DateTime _lastStartTime;
        private DispatcherTimer _timerDispatcher;

        private static readonly TimeSpan TickInterval = new TimeSpan(TimeSpan.TicksPerSecond /10);

        /// <summary>
        /// Raised every <see cref="TickInterval"/> to inform the VM that the stopwatch counter has changed significantly
        /// </summary>
        public event EventHandler Tick;

        /// <summary>
        /// Raised when the stopwatch is paused
        /// </summary>
        public event EventHandler Pause;

        /// <summary>
        /// Raised when the stopwatch is resumed
        /// </summary>
        public event EventHandler Resume;

        /// <summary>
        /// Initialise the <see cref="DispatcherTimer"/> and resets the total time to zero
        /// </summary>
        public StopwatchModel()
        {
            _timerDispatcher = new DispatcherTimer { Interval = TickInterval};
            _timerDispatcher.Tick += TimerDispatcherOnTick;
            BaseElapsedTime = new TimeSpan(0);
        }

        /// <summary>
        /// The amount of elapsed time on the stopwatch up to the most recent <see cref="Start"/>
        /// </summary>
        public TimeSpan BaseElapsedTime { get; set; }


        /// <summary>
        /// The amount of time elapsed since we lasted called <see cref="Start"/>
        /// </summary>
        public TimeSpan ElapsedTimeSinceLastStart
        {
            get { return DateTime.Now - _lastStartTime; }
        }

        /// <summary>
        /// The total elapsed time on the stopwatch
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get { return IsRunning ? BaseElapsedTime + ElapsedTimeSinceLastStart : BaseElapsedTime; }
        }

        /// <summary>
        /// <c>true</c> if the stopwatch is currently running, <c>false</c> otherwise
        /// </summary>
        public bool IsRunning
        {
            get { return _timerDispatcher.IsEnabled; }
        }


        /// <summary>
        /// Allows override of the default tick interval (see <see cref="TickInterval"/>) to get more or less frequent updates
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get { return _timerDispatcher.Interval; }
            set { _timerDispatcher.Interval = value; }
        }


        /// <summary>
        /// Notifies the VM that the stopwatch has changed by the amount in <see cref="UpdateInterval"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerDispatcherOnTick(object sender, EventArgs e)
        {
            SafeRaise.Raise(Tick, this);
        }


        /// <summary>
        /// Starts the stopwatch
        /// </summary>
        public void Start()
        {
            if (!_timerDispatcher.IsEnabled)
            {
                Logger.Debug("Starting timer from {0}", ElapsedTime.ToString("c"));
                _lastStartTime = DateTime.Now;
                _timerDispatcher.Start();
                SafeRaise.Raise(Resume, this);
            }
            else
            {
                Logger.Debug("Ignoring call to start stopwatch as it's already started");
            }
        }


        /// <summary>
        /// Stops the stopwatch
        /// </summary>
        public void Stop()
        {
            if (_timerDispatcher.IsEnabled)
            {
                Logger.Debug("Stopping Stopwatch");
                _timerDispatcher.Stop();
                BaseElapsedTime += ElapsedTimeSinceLastStart;
                Logger.Debug("Stopped timer at {0}", ElapsedTime.ToString("c"));
                SafeRaise.Raise(Pause, this);
            }
            else
            {
                Logger.Debug("Ignoring call to stop stopwatch as it's already stopped");
            }
        }


        /// <summary>
        /// Ensures that the timer dispatcher is stopped
        /// </summary>
        public void Dispose()
        {
            if (_timerDispatcher.IsEnabled)
            {
                _timerDispatcher.Stop();
            }
            _timerDispatcher = null;
        }

    }
}