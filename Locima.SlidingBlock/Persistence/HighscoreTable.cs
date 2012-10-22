using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Locima.SlidingBlock.IO;

namespace Locima.SlidingBlock.Persistence
{
    [DataContract]
    public class HighScoreTable : IPersistedObject
    {
        [DataMember]
        public List<Highscore> Scores { get; set; }

        public string Id { get; set; }
        public DateTimeOffset LastUpdate { get; set; }

        public void Sort()
        {
            Scores.Sort((first, second) => first.TotalTime.CompareTo(second.TotalTime));
        }
    }
}