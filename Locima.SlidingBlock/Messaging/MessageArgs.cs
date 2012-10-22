namespace Locima.SlidingBlock.Messaging
{
    public abstract class MessageArgs
    {
        public bool Success { get; set; }
        public bool Handled { get; set; }
    }
}