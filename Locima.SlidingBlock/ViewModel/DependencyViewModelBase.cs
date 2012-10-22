using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    public class DependencyViewModelBase : DependencyObject, INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public bool IsDesignTime
        {
            get { return (Application.Current == null) || (Application.Current.GetType() == typeof (Application)); }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void OnNotifyPropertyChanged(string changedPropertyName)
        {
            Logger.Debug("{0} notifying that {1} has changed", this, changedPropertyName);
            if (PropertyChanged != null)
            {
                Delegate[] eventHandlers = PropertyChanged.GetInvocationList();

                foreach (
                    PropertyChangedEventHandler currentSubscriber in eventHandlers.Cast<PropertyChangedEventHandler>())
                {
                    try
                    {
                        currentSubscriber(this, new PropertyChangedEventArgs(changedPropertyName));
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorException(string.Format("Event subscriber {0} threw an exception", currentSubscriber), e);
                    }
                }
            }
        }
    }
}