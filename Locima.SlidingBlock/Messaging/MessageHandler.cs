namespace Locima.SlidingBlock.Messaging
{

    /// <summary>
    /// Generic delegate used for all handlers of messages sent to the view
    /// </summary>
    /// <typeparam name="TEventArgs">The type of messages that this handler will process</typeparam>
    /// <param name="sender">The view model that sent the message</param>
    /// <param name="eventArgs">The parameters of the message</param>
    public delegate void MessageHandler<TEventArgs>(object sender, TEventArgs eventArgs) where TEventArgs : MessageArgs;
}