using System.Runtime.Serialization;

namespace Locima.SlidingBlock.IO
{
    public interface IShadowableObject
    {
        /// <summary>
        /// If <c>true</c> then this object is a shadow.
        /// </summary>
        bool IsShadow { get; set; }

        /// <summary>
        /// Set to the ID of the object that this is a shadow of.  
        /// </summary>
        /// <remarks>
        /// Shadow objects are used for "in progress" objects which usually have an object which they are shadowing (but not always).  They are used to easily facilitate the
        /// editing of objects over multiple pages.  I.e. where the app needs to save an object between pages and each state may or may not be valid.</remarks>
        string ShadowOf { get; set; }
    }
}