using System;
using System.Windows.Threading;
using Locima.SlidingBlock.Common;

namespace Locima.SlidingBlock.Model
{
    public class StopwatchModel : IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private DateTime _lastStartTime;
        private DispatcherTimer _timerDispatcher;

        public event EventHandler Tick;
        public event EventHandler Pause;
        public event EventHandler Resume;

        public StopwatchModel()
        {
            _timerDispatcher = new DispatcherTimer { Interval = new TimeSpan(TimeSpan.TicksPerSecond/10) };
            _timerDispatcher.Tick += TimerDispatcherOnTick;
            BaseElapsedTime = new TimeSpan(0);
        }

        public TimeSpan BaseElapsedTime { get; set; }

        public TimeSpan ElapsedTimeSinceLastStart
        {
            get { return DateTime.Now - _lastStartTime; }
        }

        public TimeSpan ElapsedTime
        {
            get { return IsRunning ? BaseElapsedTime + ElapsedTimeSinceLastStart : BaseElapsedTime; }
        }

        public bool IsRunning
        {
            get { return _timerDispatcher.IsEnabled; }
        }

        public TimeSpan UpdateInterval
        {
            get { return _timerDispatcher.Interval; }
            set { _timerDispatcher.Interval = value; }
        }


        private void TimerDispatcherOnTick(object sender, EventArgs e)
        {
            SafeRaise.Raise(Tick, this);
        }


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