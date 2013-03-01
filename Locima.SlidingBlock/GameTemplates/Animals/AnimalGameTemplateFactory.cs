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
            const string ells = "Elliot Moore";
            const string emma = "Emma Brodie";
            GameTemplate gm = new GameTemplate
            {
                IsReadOnly = true,
                AppId = PersistentId,  // We fix the ID here as we only want one instance of this definition
                Title = "animals",
                Author = "various shots of animals",
                Levels = new List<LevelDefinition>
                        {
                            new LevelDefinition
                                {
                                    XapImageUri = CreateImageUri("Lion.jpg"),
                                    ImageTitle = "Lion",
                                    ImageText =
                                        "Lion against a blue sky",
                                    OwnerName = robin,
                                    ImageLink = new Uri("http://www.flickr.com/photos/robinkearney/5017237783/", UriKind.Absolute),
                                    License = LicenseDefinition.CcByNcNd30,
                                    ScrambleType = Scrambler.ScrambleType.Shuffle
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = CreateImageUri("Tiger.jpg"),
                                    ImageTitle = "Tiger",
                                    ImageText = "Tiger from the Isle of Wight Zoo",
                                    OwnerName = robin,
                                    ImageLink = new Uri("http://www.flickr.com/photos/robinkearney/4999324236/", UriKind.Absolute),
                                    License = LicenseDefinition.CcByNcNd30,
                                    ScrambleType = Scrambler.ScrambleType.Shuffle
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = CreateImageUri("Monkey.jpg"),
                                    ImageTitle = "Monkey",
                                    ImageText = "And a monkey!",
                                    OwnerName = emma,
                                    ImageLink = null,
                                    License = LicenseDefinition.CcByNcNd30,
                                    ScrambleType = Scrambler.ScrambleType.Shuffle
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = CreateImageUri("Rhino.jpg"),
                                    ImageTitle = "\"Over here!\"",
                                    ImageText = "A rhinoceros at Whipsnade Zoo",
                                    OwnerName = ells,
                                    ImageLink = new Uri("http://www.flickr.com/photos/elliotmoore/155906621/in/set-72157594149098888", UriKind.Absolute),
                                    License = LicenseDefinition.CcByNcNd30,
                                    ScrambleType = Scrambler.ScrambleType.Shuffle
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = CreateImageUri("Elephants.jpg"),
                                    ImageTitle = "Elephant Pals",
                                    ImageText = "Two elephants at Whipsnade",
                                    OwnerName = ells,
                                    ImageLink = new Uri("http://www.flickr.com/photos/elliotmoore/155918350/in/set-72157594149098888", UriKind.Absolute),
                                    License = LicenseDefinition.CcByNcNd30,
                                    ScrambleType = Scrambler.ScrambleType.Shuffle
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = CreateImageUri("Giraffes.jpg"),
                                    ImageTitle = "Giraffes",
                                    ImageText = "Two giraffes at Whipsnade Zoo",
                                    OwnerName = ells,
                                    ImageLink = new Uri("http://www.flickr.com/photos/elliotmoore/155913506/in/set-72157594149098888", UriKind.Absolute),
                                    License = LicenseDefinition.CcByNcNd30,
                                    ScrambleType = Scrambler.ScrambleType.Shuffle
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = CreateImageUri("Penguins.jpg"),
                                    ImageTitle = "Penguins!",
                                    ImageText = "Penguins at London Zoo",
                                    OwnerName = robin,
                                    ImageLink = new Uri("http://www.flickr.com/photos/robinkearney/92251928/",UriKind.Absolute),
                                    License = LicenseDefinition.CcByNcNd30,
                                    ScrambleType = Scrambler.ScrambleType.Shuffle
                                }
                        }
            };

            return gm;
        }
            
    }
}
