using System;
using System.Collections.Generic;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Scrambles;

namespace Locima.SlidingBlock.GameTemplates.Toys
{

    /// <summary>
    /// Creates the game template for the Toys game template
    /// </summary>
    public class ToysGameTemplate : GameTemplateFactory
    {

        /// <inheritdoc/>
        public override string PersistentId
        {
            get { return "ACE8AEDE-B4D9-4A8F-9713-00D88FED504E"; }
        }

        /// <inheritdoc/>
        protected override string TemplatePath { get { return "/GameTemplates/Toys"; } }

        /// Creates the game template for the toys on a black background collection
        public override GameTemplate Create()
        {
            const string robin = "Robin Kearney";
            Uri robinLink = new Uri("http://www.riviera.org.uk/",UriKind.Absolute);
            GameTemplate gm = new GameTemplate
            {
                IsReadOnly = true,
                AppId = PersistentId,  // We fix the ID here as we only want one instance of this definition
                Title = "Robin's Toys",
                Author = "Robin Kearney",
                Levels = new List<LevelDefinition>(3)
                        {
                            new LevelDefinition
                                {
                                    XapImageUri = CreateImageUri("Stormtooper.jpg"),
                                    ImageTitle = "Stormtrooper",
                                    ImageText =
                                        "Toy stormtrooper helmet",
                                    OwnerName = robin,
                                    OwnerUri = robinLink,
                                    License = LicenseDefinition.CcByNcNd30,
                                    ScrambleType = Scrambler.ScrambleType.Shuffle
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = CreateImageUri("Mushroom.jpg"),
                                    ImageTitle = "Mario Mushroom",
                                    ImageText = "A mushroom from Mario",
                                    OwnerName = robin,
                                    OwnerUri = robinLink,
                                    License = LicenseDefinition.CcByNcNd30,
                                    ScrambleType = Scrambler.ScrambleType.Shuffle
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = CreateImageUri("Viking.jpg"),
                                    ImageTitle = "Toy Viking",
                                    ImageText =
                                        "A toy viking",
                                    OwnerName = robin,
                                    OwnerUri = robinLink,
                                    License = LicenseDefinition.CcByNcNd30,
                                    ScrambleType = Scrambler.ScrambleType.Shuffle
                                }
                        }
            };

            return gm;
        }
    }
}
