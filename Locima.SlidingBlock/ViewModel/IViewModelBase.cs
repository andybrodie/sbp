using System.ComponentModel;
using Locima.SlidingBlock.Messaging;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    ///   Base interface for all View Models.
    /// </summary>
    /// <remarks>
    ///   This contains functionality for:
    ///   <list type="number">
    ///     <item>
    ///       <description>Notifying property changes to the view:
    ///         <see cref="OnNotifyPropertyChanged" />
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>Sending messages to the view (e.g. requests to navigate to a new page):
    ///         <see cref="SendViewMessage" />
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    public interface IViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        ///   Convenience method to determine whether we are executing in a design-time environment (e.g. Visual Studio 2010)
        /// </summary>
        bool IsDesignTime { get; }

        /// <summary>
        ///   Convenience method to notify the view that an observed property has changed.  Suppresses any exceptions from handlers.
        /// </summary>
        /// <param name="changedPropertyName"> The name of hte property that changes </param>
        void OnNotifyPropertyChanged(string changedPropertyName);

        /// <summary>
        ///   Use this to set the message handlers of this view model to the same message handlers of the view model passed in <paramref
        ///    name="syncViewModel" />
        /// </summary>
        /// <remarks>
        ///   Note that this sets the list of message handlers to the same object.  Therefore any changes will be reflected in both view models.
        ///   Note that this erases any message handlers in this object.
        /// </remarks>
        /// <param name="syncViewModel"> The view model to share message handlers with </param>
        void ShareMessageHandlers(ViewModelBase syncViewModel);

        /// <summary>
        ///   Registers a handler for a particular message that can be sent back to the view
        /// </summary>
        /// <typeparam name="TMessageArgs"> If a message is fired with arguments of this type, this handler will be invoked </typeparam>
        /// <param name="handler"> The hander code to invoke when a message matching <typeparamref name="TMessageArgs" /> is sent from the view model </param>
        void RegisterMessageHandler<TMessageArgs>(MessageHandler<TMessageArgs> handler) where TMessageArgs : MessageArgs;


        /// <summary>
        ///   Invoke the message handler for the message argument type passed
        /// </summary>
        /// <param name="args"> </param>
        /// <returns> </returns>
        bool SendViewMessage(MessageArgs args);
    }
}