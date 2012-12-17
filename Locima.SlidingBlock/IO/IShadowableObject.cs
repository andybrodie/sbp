using System.Runtime.Serialization;
using Locima.SlidingBlock.GameTemplates;

namespace Locima.SlidingBlock.IO
{

    /// <summary>
    /// Any persistent object (e.g. <see cref="GameTemplate"/>) that is a shadow of another object.
    /// </summary>
    /// <remarks>
    /// <para>When persisted object has a shadow the shadow is used to store a new version of the object which can be used as a "temporary" workspace for the object.
    /// Once the shadow is finished with, it is either discarded (deleted) or promoted, in which case it replaces the object it was shadowing.</para>
    /// <para>This is useful for editing <see cref="GameTemplate"/>s specifically because it allows us to save the object in an invalid state between
    /// <see cref="GameEditor"/>, <see cref="LevelEditor"/> and <see cref="ImageAreaChooser"/>, keeping the state safe in case the user exits mid-edit, but
    /// not overwriting the orignal until the user has confirmed that they're happy with their changes.  In the mean-time, when selecting a game template
    /// to start a new game with, shadows game templates are ignored.</para>
    /// <para>You cannot create shadows of objects which do not have IDs, or rather, if you do, then a new ID will be allocated on promotion.</para>
    /// </remarks>
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