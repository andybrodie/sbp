using System;

namespace Locima.SlidingBlock.IO
{
    /// <summary>
    ///   All persisted objects have these two members, regardless of the method of the persistence
    /// </summary>
    public interface IPersistedObject
    {
        /// <summary>
        /// A unique identity for this object
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The date and time the object wsas last persisted
        /// </summary>
        DateTimeOffset LastUpdate { get; set; }
    }
}