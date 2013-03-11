using System;

namespace Locima.SlidingBlock.Common
{
    /// <summary>
    /// Thrown when a method or message arrives at an object that is in state incapable of acting on the incoming method invocation or message
    /// </summary>
    /// <remarks>
    /// If this is thrown it indicates a defect in the application
    /// </remarks>
    public class InvalidStateException : Exception
    {

        /// <summary>
        /// Default constructor passing just a message
        /// </summary>
        public InvalidStateException(string message) : base(message)
        {
        }

        /// <summary>
        /// Delegates to <see cref="InvalidStateException(string)"/> allowing the caller to use a <see cref="string.Format(string,object)"/>-style call
        /// </summary>
        public InvalidStateException(string format,  params object[] args) : base(string.Format(format, args))
        {
        }
    }
}