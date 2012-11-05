using System;
using System.Runtime.Serialization;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Persistence;

namespace Locima.SlidingBlock.Model
{

    /// <summary>
    /// Links a player to a game in progress by containing a reference to the <see cref="PlayerDetails.Id"/> member as well as including the <see cref="Position"/> of the player on the grid
    /// </summary>
    [DataContract]
    public class PlayerLink
    {

        /// <summary>
        /// The tile position the player occupies
        /// </summary>
        [DataMember]
        public Position Position { get; set; }

        /// <summary>
        /// The ID of a player in this position
        /// </summary>
        [DataMember]
        public string PlayerDetailsId { get; set; }
    }
}
 