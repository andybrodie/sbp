using System;
using System.Runtime.Serialization;
using Locima.SlidingBlock.Scrambles;

namespace Locima.SlidingBlock.GameTemplates
{


    /// <summary>
    /// Defines a single level of a <see cref="GameDefinition"/></summary>
    [DataContract]
    public class LevelDefinition
    {
        /// <summary>
        /// If the image for the level is in isolated storage (e.g. if a download image has been cropped), then this is set
        /// </summary>
        [DataMember]
        public string IsolatedStorageFilename { get; set; }

        /// <summary>
        /// If the image for the level is contained as content of the XAP file (i.e. it's a built-in level), then this is set
        /// </summary>
        [DataMember]
        public Uri XapImageUri { get; set; }

        /// <summary>
        /// The license covering the image
        /// </summary>
        [DataMember]
        public LicenseDefinition License { get; set; }


        /// <summary>
        /// A link associated with image (.e.g to the license holders home page)
        /// </summary>
        [DataMember]
        public Uri OwnerUri { get; set; }

        /// <summary>
        /// The name of the license holder for the image
        /// </summary>
        [DataMember]
        public string OwnerName { get; set; }

        /// <summary>
        /// A description that goes with the image (i.e. what's the image a picture of, e.g. "My pet dog")
        /// </summary>
        [DataMember]
        public string ImageText { get; set; }

        /// <summary>
        /// A title for the image
        /// </summary>
        [DataMember]
        public string ImageTitle { get; set; }

        /// <summary>
        /// THe type of scramble to be applied to the image, i.e. how will the tiles be jumbled up at the beginning of the level.  Different scrambles can make
        /// the level easier or more difficult.
        /// </summary>
        [DataMember]
        public Scrambler.ScrambleType ScrambleType { get; set; }
    }
}