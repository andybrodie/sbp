using System;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;

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
        /// <remarks>
        /// The Id should be treated as a black-box by all application code.  The contract for this member only states that the ID is persistent.  No guarantees of structure,
        /// size or content are guaranteed.  Do NOT set this in application code outside of the persistence interface implementation.  If you require persistent static 
        /// identity for objects that is available and settable in application code, use <see cref="AppId"/></remarks>
        string Id { get; set; }

        /// <summary>
        /// The date and time the object was last persisted
        /// </summary>
        /// <remarks>
        /// I originally made this an instance of <see cref="DateTime"/>, however as I was creating an Isolated Storage implementation of these interfaces first, I went with
        /// <see cref="DateTimeOffset"/> as that is what's returned by <see cref="IsolatedStorageFile.GetLastWriteTime"/>.</remarks>
        DateTimeOffset LastUpdate { get; set; }

        /// <summary>
        /// An identity for the object set and managed by the application code, rather than the persistence mechanism.
        /// </summary>
        /// <remarks>
        /// <para>Use this if you want to create singleton persistent objects with an identity.  Identity uniqueness and management is the responsibility of the application code, the
        /// persistence mechanism will not touch it (except to persist it)</para>
        /// <para>If you don't need persistent application IDs for objects in implementations, then just declare this member but don't mark using the <see cref="DataMemberAttribute"/>.</para></remarks>
        string AppId { get; set; }

    }
} 