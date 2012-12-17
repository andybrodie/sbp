namespace Locima.SlidingBlock.Messaging
{

    /// <summary>
    /// Mandatory set of message argument for messages sent to the view
    /// </summary>
    public abstract class MessageArgs
    {

        /// <summary>
        /// Let's the view model know whether a handler was executed successfully
        /// </summary>
        public bool Success { get; set; }

    }
}