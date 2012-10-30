using System.ComponentModel;
using System.Windows;
using Locima.SlidingBlock.Messaging;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    ///   Combines <see cref="ViewModelBase" /> with <see cref="DependencyObject" /> as a base class for view models that require dependency property support
    /// </summary>
    /// <see cref="TileViewModel" />
    public abstract class DependencyViewModelBase : DependencyObject, IViewModelBase
    {
        private readonly ViewModelBase _viewModelBase;

        protected DependencyViewModelBase()
        {
            _viewModelBase = new ViewModelBase();
        }

        #region IViewModelBase Members

        public bool IsDesignTime
        {
            get { return _viewModelBase.IsDesignTime; }
        }

        public void OnNotifyPropertyChanged(string changedPropertyName)
        {
            _viewModelBase.OnNotifyPropertyChanged(changedPropertyName);
        }

        public void ShareMessageHandlers(ViewModelBase syncViewModel)
        {
            _viewModelBase.ShareMessageHandlers(syncViewModel);
        }

        public void RegisterMessageHandler<TMessageArgs>(MessageHandler<TMessageArgs> handler)
            where TMessageArgs : MessageArgs
        {
            _viewModelBase.RegisterMessageHandler(handler);
        }

        public bool SendViewMessage(MessageArgs args)
        {
            return _viewModelBase.SendViewMessage(args);
        }

        /// <summary>
        /// Implementation of <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}