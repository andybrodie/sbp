using System;
using System.Runtime.Serialization;
using Locima.SlidingBlock.Scrambles;

namespace Locima.SlidingBlock.GameTemplates
{
    [DataContract]
    public class LevelDefinition
    {
        [DataMember]
        public string IsolatedStorageFilename { get; set; }

        [DataMember]
        public Uri XapImageUri { get; set; }

        [DataMember]
        public LicenseDefinition License { get; set; }

        [DataMember]
        public Uri OwnerUri { get; set; }

        [DataMember]
        public string OwnerName { get; set; }

        [DataMember]
        public string ImageText { get; set; }

        [DataMember]
        public string ImageTitle { get; set; }

        [DataMember]
        public Scrambler.ScrambleType ScrambleType { get; set; }
    }
}