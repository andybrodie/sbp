namespace Locima.SlidingBlock.Messaging
{
    public delegate void MessageHandler<TEventArgs>(object sender, TEventArgs e) where TEventArgs : MessageArgs;
}