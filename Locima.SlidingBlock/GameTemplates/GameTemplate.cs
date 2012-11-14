using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.IO;

namespace Locima.SlidingBlock.GameTemplates
{

    /// <summary>
    /// Used to define a game, which consists of a sequence of level (<see cref="LevelDefinition"/> instances)
    /// </summary>
    [DataContract]
    public class GameTemplate : IPersistedObject
    {

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
    }
}