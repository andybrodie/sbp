using System;
using System.IO.IsolatedStorage;

namespace Locima.SlidingBlock.IO
{
    /// <summary>
    ///   All persisted objects have these two members, regardless of the method of the persistence
    /// </summary>
    /// <remarks>
    /// This instance ensures that all persisted objects (regardless of persistence mechanism) have two fields: <see cref="Id"/> and <see cref="LastUpdate"/>.  These fields are not set
    /// by the caller, they are set to an <see cref="IStorageManager"/>-specific value.</remarks>
    public interface IPersistedObject
    {
        /// <summary>
        /// A unique identity for this object that can be used to retrieve the object back from storage later
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The date and time the object was last persisted
        /// </summary>
        /// <remarks>
        /// I originally made this an instance of <see cref="DateTime"/>, however as I was creating an Isolated Storage implementation of these interfaces first, I went with
        /// <see cref="DateTimeOffset"/> as that is what's returned by <see cref="IsolatedStorageFile.GetLastWriteTime"/>.</remarks>
        DateTimeOffset LastUpdate { get; set; }
    }
}