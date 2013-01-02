using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Messaging;
using Microsoft.Phone.Shell;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// Base class for all View Models, or if another base class is already in use, then this can be used as an encapsulated member of that view model, e.g. <see cref="DependencyViewModelBase"/>
    /// </summary>
    /// <remarks>
    /// <para>This contains functionality for:
    /// <list type="number">
    /// <item><description>Notifying property changes to the view: <see cref="OnNotifyPropertyChanged"/></description></item>
    /// <item><description>Sending messages to the view (e.g. requests to navigate to a new page): <see cref="SendViewMessage"/></description></item>
    /// </list></para>
    /// <para>
    /// TODO: Allow multiple message handlers for a <see cref="MessageArgs"/> type
    /// </para></remarks>
    public class ViewModelBase : IViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Convenience method to determine whether we are executing in a design-time environment (e.g. Visual Studio 2010)
        /// </summary>
        public bool IsDesignTime
        {
            get { return (Application.Current == null) || (Application.Current.GetType() == typeof (Application)); }
        }

        #region INotifyPropertyChanged Members

        /// <see cref="INotifyPropertyChanged.PropertyChanged"/>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        
        /// <summary>
        /// Convenience method to notify the view that an observed property has changed.  Suppresses any exceptions from handlers.
        /// </summary>
        /// <param name="changedPropertyName">The name of hte property that changes</param>
        public void OnNotifyPropertyChanged(string changedPropertyName)
        {
//            Logger.Debug("{0} notifying that {1} has changed", this, changedPropertyName);
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
                        Logger.ErrorException(
                            string.Format("Event subscriber {0} threw an exception when notifying change in {1}",
                                          currentSubscriber, changedPropertyName), e);
                    }
                }
            }
        }

        #region View Message Registration and Notification

        private Dictionary<Type, Delegate> _messageHandlers = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Use this to set the message handlers of this view model to the same message handlers of the view model passed in <paramref name="syncViewModel"/>
        /// </summary>
        /// <remarks>
        /// Note that this sets the list of message handlers to the same object.  Therefore any changes will be reflected in both view models.
        /// Note that this erases any message handlers in this object.</remarks>
        /// <param name="syncViewModel">The view model to share message handlers with</param>
        public void ShareMessageHandlers(ViewModelBase syncViewModel)
        {
            Logger.Info("Synchronising message handlers in {0} with message handlers from {1}, they are now sharing the same Dictionary", this, syncViewModel);
            _messageHandlers = syncViewModel._messageHandlers;
        }


        /// <summary>
        ///   Registers a handler for a particular message that can be sent back to the view
        /// </summary>
        /// <typeparam name="TMessageArgs"> If a message is fired with arguments of this type, this handler will be invoked </typeparam>
        /// <param name="handler"> The hander code to invoke when a message matching <typeparamref name="TMessageArgs" /> is sent from the view model </param>
        public void RegisterMessageHandler<TMessageArgs>(MessageHandler<TMessageArgs> handler)
            where TMessageArgs : MessageArgs
        {
            Type messageType = typeof (TMessageArgs);
            Logger.Debug("Registering view message handler for {0}", messageType.Name);
            _messageHandlers[typeof (TMessageArgs)] = handler;
        }


        /// <summary>
        ///   Invoke the message handler for the message argument type passed
        /// </summary>
        /// <param name="args">The payload of the message.  The type of this object will determine the event handler that is called</param>
        /// <returns> </returns>
        public bool SendViewMessage(MessageArgs args)
        {
            bool result;
            Delegate handler;
            Type handlerType = args.GetType();
            if (_messageHandlers.TryGetValue(handlerType, out handler))
            {
                Logger.Info("Invoking message handler {2} on {1} for {0} on the UI thread", handlerType, handler.Target, handler.Method);
                Deployment.Current.Dispatcher.BeginInvoke(() => handler.DynamicInvoke(this, args));
                result = true;
            }
            else
            {
                Logger.Error("No handler registered for {0} on {1}", args.GetType(), this);
                result = false;
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Sets the property in <see cref="PhoneApplicationService.State"/> with the name
        /// </summary>
        /// <param name="propertyName">The name of the property to retrieve</param>
        /// <param name="propertyValue">The value to set.</param>
        protected void SetState(string propertyName, object propertyValue)
        {
            StateManagementHelper.SetState(GetType().Name, propertyName, propertyValue);
        }

        /// <summary>
        /// Removes a <see cref="KeyValuePair{TKey,TValue}"/> from the <see cref="PhoneApplicationService.State"/> dictionary.
        /// </summary>
        /// <param name="propertyName">The name of the property to remove</param>
        /// <returns><c>true</c> if an object was found to delete, <c>false</c> otherwise</returns>
        protected bool ClearState(string propertyName)
        {
            return StateManagementHelper.ClearState(GetType().Name, propertyName);
        }
 

        /// <summary>
        /// Attempts to retrieve a property from the <see cref="PhoneApplicationService.Current"/>'s <see cref="PhoneApplicationService.State"/> dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the output parameter</typeparam>
        /// <param name="propertyName">The name of the property to retrieve</param>
        /// <param name="outputValue">The value to populate with the property.  If the property doesn't exist or is of the wrong type then <c>default(<typeparamref name="T"/>) will be set</c>.</param>
        /// <returns><c>true</c> if a property value was successfully retrieved and <paramref name="outputValue"/> has been set successfully, <c>false</c> otherwise</returns>
        protected bool TryGetState<T>(string propertyName, out T outputValue)
        {
            return StateManagementHelper.TryGetState(GetType().Name, propertyName, out outputValue);
        }
 
        /// <summary>
        /// Remove all keys from the <see cref="PhoneApplicationService.State"/> dictionary set by this page
        /// </summary>
        protected void ClearState()
        {
            StateManagementHelper.ClearState(GetType().Name);
        }


    }
}