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
        /// License for Creative Commons: Attribution-NonCommercial-NoDerivs 3.0 Unported (see http://creativecommons.org/licenses/by-nc-nd/3.0/legalcode)
        /// </summary>        
        public static readonly LicenseDefinition CcByNcNd30 = new LicenseDefinition
                                                              {
                                                                  Link = new Uri("http://creativecommons.org/licenses/by-nc-nd/3.0/deed.en_US"),
                                                                  Title = "CC BY-NC-ND 3.0"
                                                              };


        /// <summary>
        /// The set of built-in licenses
        /// </summary>
        public static List<LicenseDefinition> Licenses = new List<LicenseDefinition>
                                                             {
                                                                 CcByNcNd30
                                                             };

        #endregion
    }
}