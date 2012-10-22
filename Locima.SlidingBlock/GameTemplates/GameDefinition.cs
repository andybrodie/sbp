using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Locima.SlidingBlock.GameTemplates
{
    [DataContract]
    public class GameDefinition
    {
        [DataMember]
        public List<LevelDefinition> Levels { get; set; }
        
    }
}