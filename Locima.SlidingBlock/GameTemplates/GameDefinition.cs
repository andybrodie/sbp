using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Locima.SlidingBlock.GameTemplates
{

    /// <summary>
    /// Used to define a game, which consists of a sequence of level (<see cref="LevelDefinition"/> instances)
    /// </summary>
    [DataContract]
    public class GameDefinition
    {
        /// <summary>
        /// A sequence of levels that make up the game
        /// </summary>
        [DataMember]
        public List<LevelDefinition> Levels { get; set; }
        
    }
}