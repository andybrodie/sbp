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

        /// <summary>
        /// Initialises <see cref="_viewModelBase"/>
        /// </summary>
        protected DependencyViewModelBase()
        {
            _viewModelBase = new ViewModelBase();
        }

        #region IViewModelBase Members

        /// <summary>
        /// Delegates to <see cref="ViewModelBase.IsDesignTime"/>
        /// </summary>
        public bool IsDesignTime
        {
            get { return _viewModelBase.IsDesignTime; }
        }

        /// <summary>
        /// Delegates to <see cref="ViewModelBase.OnNotifyPropertyChanged"/>
        /// </summary>
        public void OnNotifyPropertyChanged(string changedPropertyName)
        {
            _viewModelBase.OnNotifyPropertyChanged(changedPropertyName);
        }

        /// <summary>
        /// Delegates to <see cref="ViewModelBase.ShareMessageHandlers"/>
        /// </summary>
        public void ShareMessageHandlers(ViewModelBase syncViewModel)
        {
            _viewModelBase.ShareMessageHandlers(syncViewModel);
        }

        /// <summary>
        /// Delegates to <see cref="ViewModelBase.RegisterMessageHandler{TMessageArgs}"/>
        /// </summary>

        public void RegisterMessageHandler<TMessageArgs>(MessageHandler<TMessageArgs> handler)
            where TMessageArgs : MessageArgs
        {
            _viewModelBase.RegisterMessageHandler(handler);
        }

        /// <summary>
        /// Delegates to <see cref="ViewModelBase.SendViewMessage"/>
        /// </summary>
        public bool SendViewMessage(MessageArgs args)
        {
            return _viewModelBase.SendViewMessage(args);
        }

        /// <summary>
        /// Delegates to <see cref="ViewModelBase.PropertyChanged"/>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { _viewModelBase.PropertyChanged += value; }
            remove { _viewModelBase.PropertyChanged -= value; }
        }

        #endregion
    }
}