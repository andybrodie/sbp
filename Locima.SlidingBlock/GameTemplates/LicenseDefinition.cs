using System;
using System.Runtime.Serialization;

namespace Locima.SlidingBlock.GameTemplates
{

    /// <summary>
    /// Used to model the license applied to a <see cref="LevelDefinition"/>
    /// </summary>
    [DataContract]
    public class LicenseDefinition
    {
        /// <summary>
        /// A link to the full definition/text of the license
        /// </summary>
        [DataMember]
        public Uri Link { get; set; }

        /// <summary>
        /// The title of the license
        /// </summary>
        [DataMember]
        public string Title { get; set; }
    }
}