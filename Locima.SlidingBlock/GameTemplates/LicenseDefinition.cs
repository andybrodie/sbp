using System;
using System.Collections.Generic;
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

        #region Built-in License Definitions

        /// <summary>
        /// License for Creative Common Attribution 3.0 (http://ccby3.0.org/licenses/by/3.0/)
        /// </summary>
        public static readonly LicenseDefinition CcBy30 = new LicenseDefinition
                                                              {
                                                                  Link = new Uri("http://ccby3.0.org/licenses/by/3.0/"),
                                                                  Title = "Creative Commons Attribution 3.0 (CC BY 3.0)"
                                                              };


        /// <summary>
        /// The set of built-in licenses
        /// </summary>
        public static List<LicenseDefinition> Licenses = new List<LicenseDefinition>
                                                             {
                                                                 CcBy30
                                                             };

        #endregion
    }
}