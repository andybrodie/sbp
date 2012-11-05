using System;
using System.Runtime.Serialization;
using System.Windows.Media;
using Locima.SlidingBlock.IO;

namespace Locima.SlidingBlock.Persistence
{


    /// <summary>
    /// The details of a player represented within a <see cref="SaveGame"/> object.
    /// </summary>
    [DataContract]
    public class PlayerDetails : IPersistedObject
    {

        /// <summary>
        /// The name of the player
        /// </summary>
        [DataMember]
        public string Name { get; set; }


        /// <summary>
        /// The preferred colour the player plays with
        /// </summary>
        [DataMember]
        public Color PreferredColor { get; set; }

        /// <summary>
        /// The filename representing this player in the user application store
        /// </summary>
        /// <remarks>This is not serialised because it's metadata on the save file</remarks>
        public string Id { get; set; }

        /// <summary>
        /// The timestamp of when the player was last used
        /// </summary>
        public DateTimeOffset LastUpdate { get; set; }

        /// <summary>
        /// Override to append the <see cref="Name"/> and <see cref="Id"/> (filename) of the player
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}(Name={1}, Filename={2})", base.ToString(), Name, Id);
        }
    }

}