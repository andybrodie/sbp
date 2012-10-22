using System;
using System.Runtime.Serialization;
using System.Windows.Media;
using Locima.SlidingBlock.IO;

namespace Locima.SlidingBlock.Persistence
{

    [DataContract]
    public class PlayerDetails : IPersistedObject
    {
        [DataMember]
        public string Name { get; set; }

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

        public override string ToString()
        {
            return string.Format("{0}(Name={1}, Filename={2})", base.ToString(), Name, Id);
        }
    }

}