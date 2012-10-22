using System;
using System.Runtime.Serialization;

namespace Locima.SlidingBlock.GameTemplates
{
    [DataContract]
    public class LicenseDefinition
    {
        [DataMember]
        public Uri Link { get; set; }

        [DataMember]
        public string Title { get; set; }
    }
}