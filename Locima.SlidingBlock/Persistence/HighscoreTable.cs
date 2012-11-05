using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Locima.SlidingBlock.IO;

namespace Locima.SlidingBlock.Persistence
{

    /// <summary>
    /// The data contract for the high score table
    /// </summary>
    [DataContract]
    public class HighScoreTable : IPersistedObject
    {

        /// <summary>
        /// A list of all the scores in the table
        /// </summary>
        [DataMember]
        public List<Highscore> Scores { get; set; }

        /// <summary>
        /// Unused
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Unused
        /// </summary>
        public DateTimeOffset LastUpdate { get; set; }

        /// <summary>
        /// Sorts the high score table according to the <see cref="Highscore.TotalTime"/>
        /// </summary>
        public void Sort()
        {
            Scores.Sort((first, second) => first.TotalTime.CompareTo(second.TotalTime));
        }
    }
}