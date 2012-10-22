using System;
using System.Runtime.Serialization;

namespace Locima.SlidingBlock.Persistence
{
    [DataContract]
    public class Highscore
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public TimeSpan TotalTime { get; set; }

        [DataMember]
        public DateTime When { get; set; }

        [DataMember]
        public string PlayerId { get; set; }

        [DataMember]
        public string GameId { get; set; }
    }
}