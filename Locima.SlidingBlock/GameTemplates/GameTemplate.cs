using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.IO;
using NLog;

namespace Locima.SlidingBlock.GameTemplates
{

    /// <summary>
    /// Used to define a game, which consists of a sequence of level (<see cref="LevelDefinition"/> instances)
    /// </summary>
    /// <remarks>
    /// <para>There are two types of <see cref="GameTemplate"/>, "real" and "shadows".  Shadows are used for game templates which are being edited by the user.
    /// As the process of editing a game template occurs over several different pages, it's useful to be able to save a "point in time" version of the template.
    /// However, the user may decide to discard this point in time object (i.e. the users perception of the persistence of the object is very different
    /// to what the application requires) and also the game template may be in an inconsistent or invalid state between page invocations.</para>
    /// <para>To mitigate this problem, any game template in the process of being edited is a a shadow, i.e. <see cref="IsShadow"/> is set to <c>true</c>
    /// and <see cref="ShadowOf"/> is set to the ID of the game template that this instance is shadowing.  In the case of creating a new level from
    /// scratch, <see cref="IsShadow"/> is <c>true</c> but <see cref="ShadowOf"/> is <c>null</c>.</para>
    /// <para>When the user has finished editing and wishes to "save their changes", then shadow object replaces the one being shadowed using
    /// <see cref="IGameTemplateManager.PromoteShadow"/>, unless <see cref="ShadowOf"/> is null, in which case we simply set <see cref="IsShadow"/> to <c>false</c>.</para>
    /// <para>Note that when selecting a game template to edit, shadows are always used; but when selecting a game template to play all shadows are ignored.</para>
    /// </remarks>
    [DataContract]
    public class GameTemplate : IPersistedObject, IShadowableObject
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// If this flag is set then the game cannot be deleted.
        /// </summary>
        /// <remarks>This is only used for built-in games, it avoid us having to cater for the situation where no game templates exist at all</remarks>
        [DataMember]
        public bool IsReadOnly { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string AppId { get; set; }

        /// <summary>
        /// The default name given to a game template when the user doesn't give it a title
        /// </summary>
        public static readonly string DefaultTitle = LocalizationHelper.GetString("DefaultGameTemplateTitle");

        /// <summary>
        /// A sequence of levels that make up the game
        /// </summary>
        [DataMember]
        public List<LevelDefinition> Levels { get; set; }

        /// <summary>
        /// Who created this game template
        /// </summary>
        [DataMember]
        public string Author { get;  set; }

        /// <summary>
        /// A name for this custom game
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset LastUpdate { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public bool IsShadow { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string ShadowOf { get; set; }

        /// <summary>
        /// Make this template a shadow
        /// </summary>
        public void MakeShadow()
        {
            if (IsShadow)
            {
                Logger.Info("Ignoring call to make shadow as this is already a shadow {0}", this);
            }
            else
            {
                Logger.Info("Making {0} a shadow", this);
                IsShadow = true;
                ShadowOf = Id;
                Id = null;
            }
        }


        /// <summary>
        /// More readable implementation to include <see cref="Id"/>, <see cref="Title"/> and shadow information
        /// </summary>
        public override string ToString()
        {
            return string.Format("GameTemplate({0}, \"{1}\", {2}, {3})", Id, Title, IsShadow, ShadowOf);
        }
    }
}