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
        [DataMember]
        public Position Position { get; set; }

        [DataMember]
        public string PlayerDetailsId { get; set; }
    }
}
 