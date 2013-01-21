using System;
using System.Collections.Generic;
using Locima.SlidingBlock.Scrambles;

namespace Locima.SlidingBlock.GameTemplates.Animals
{

    /// <summary>
    /// Creates the game template for the Animals collection
    /// </summary>
    public class AnimalGameTemplateFactory : GameTemplateFactory
    {
        /// <inheritdoc/>
        protected override string TemplatePath
        {
            get { return "GameTemplates/Animals/"; }
        }

        /// <inheritdoc/>
        public override string PersistentId
        {
            get { return "77BAA2BF-0F5B-4EA9-9B00-9C52D2159005"; }
        }

        /// <inheritdoc/>
        public override GameTemplate Create()
        {
            const string robin = "Robin Kearney";
            Uri robinLink = new Uri("http://www.riviera.org.uk/",UriKind.Absolute);
            GameTemplate gm = new GameTemplate
            {
                IsReadOnly = true,
                AppId = PersistentId,  // We fix the ID here as we only want one instance of this definition
                Title = "animals",
                Author = "various shots of animals",
                Levels = new List<LevelDefinition>(3)
                        {
                            new LevelDefinition
                                {
                                    XapImageUri = CreateImageUri("Lion.jpg"),
                                    ImageTitle = "Lion",
                                    ImageText =
                                        "Lion against a blue sky",
                                    OwnerName = robin,
                                    OwnerUri = robinLink,
                                    License = LicenseDefinition.CcByNcNd30,
                                    ScrambleType = Scrambler.ScrambleType.Shuffle
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = CreateImageUri("Tiger.jpg"),
                                    ImageTitle = "Tiger",
                                    ImageText = "Hungry tiger!",
                                    OwnerName = robin,
                                    OwnerUri = robinLink,
                                    License = LicenseDefinition.CcByNcNd30,
                                    ScrambleType = Scrambler.ScrambleType.Shuffle
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = CreateImageUri("Monkey.jpg"),
                                    ImageTitle = "Monkey",
                                    ImageText = "And a monkey!",
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
