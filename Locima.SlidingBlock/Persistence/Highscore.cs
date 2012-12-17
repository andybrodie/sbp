using System;
using System.Runtime.Serialization;

namespace Locima.SlidingBlock.Persistence
{

    /// <summary>
    /// A single high score in the <see cref="HighScoreTable"/>.  A high score is achieved when the player has finished all the levels.
    /// </summary>
    [DataContract]
    public class HighScore
    {

        /// <summary>
        /// The name of the player who achieved the high score
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The total time taken 
        /// </summary>
        [DataMember]
        public TimeSpan TotalTime { get; set; }

        /// <summary>
        /// When the high score was achieved
        /// </summary>
        [DataMember]
        public DateTime When { get; set; }

        /// <summary>
        /// The ID of the player (if a local player). 
        /// </summary>
        /// <remarks>
        /// Currently unused, but in future we could use this to keep the name consistent if the player changes their name</remarks>
        [DataMember]
        public string PlayerId { get; set; }

        /// <summary>
        /// The ID of the <see cref="SaveGame"/> that led to this high score
        /// </summary>
        /// <remarks>
        /// TODO This is currently used to determine which high score to highlight.  This shouldn't be here, persisted forever
        /// </remarks>
        [DataMember]
        public string GameId { get; set; }


        /// <summary>
        /// The total number of moves used to finish the game
        /// </summary>
        [DataMember]
        public int TotalMoves { get; set; }
    }
}