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

        /// <inheritdoc/>
        public string AppId { get; set; }

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

        /// <summary>
        /// Compares based on <see cref="Id"/> only
        /// </summary>
        /// <param name="obj">The object to compare with</param>
        /// <returns>True if <see cref="Id"/> matches on <paramref name="obj"/>, false otherwise</returns>
        public override bool Equals(object obj)
        {
            PlayerDetails other = obj as PlayerDetails;
            return other != null && other.Id == Id;
        }


        /// <summary>
        /// Returns hash code based on <see cref="Id"/> or 0 if not set
        /// </summary>
        /// <returns>Hash code based on <see cref="Id"/> or 0 if not set</returns>
        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.GetHashCode();
        }
    }

}