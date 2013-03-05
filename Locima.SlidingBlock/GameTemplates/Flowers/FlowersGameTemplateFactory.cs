using System;
using System.Collections.Generic;
using Locima.SlidingBlock.Scrambles;

namespace Locima.SlidingBlock.GameTemplates.Flowers
{

    /// <summary>
    /// Creates the game template for the Animals collection
    /// </summary>
    public class FlowersGameTemplateFactory : GameTemplateFactory
    {
        /// <inheritdoc/>
        protected override string TemplatePath
        {
            get { return "GameTemplates/Flowers/"; }
        }

        /// <inheritdoc/>
        public override string PersistentId
        {
            get { return "B6F0C646-5051-4E62-B9BE-FE3D5D8DB789"; }
        }

        /// <inheritdoc/>
        public override GameTemplate Create()
        {
            GameTemplate gm = new GameTemplate
            {
                IsReadOnly = true,
                AppId = PersistentId,  // We fix the ID here as we only want one instance of this definition
                Title = "flowers",
                Author = "flowers from the garden",
                Levels = new List<LevelDefinition>
                        {
                            Create("Rhododendron", "Rhododendron from Exbury Gardens, taken in 2009", "Rhodedendron.jpg", "http://en.wikipedia.org/wiki/Rhododendron"),
                            Create("Virginia Water Lily", "A hardy Virginia Water Lily (Nymphaeaceae)", "Lillies.jpg", "http://en.wikipedia.org/wiki/Nymphaeaceae"),
                            Create("Mixed Water Lilies", "Albeda and Attraction Water Lily", "Lillies2.jpg", "http://en.wikipedia.org/wiki/Nymphaeaceae"),
                            Create("Blue Cloud Water Lily", "Despite being purple, this lily is classified as blue!", "Purple.jpg", "http://en.wikipedia.org/wiki/Nymphaeaceae"),
                            Create("Begonia", "Looks like a rose, but is actually a begonia", "RedBegonia.jpg","http://en.wikipedia.org/wiki/Begonia"),
                            Create("Begonias", "A selection of begonias", "Begonias.jpg","http://en.wikipedia.org/wiki/Begonia"),
                            Create("Hibiscus", "Hibiscus plant found in Portugal", "Hibiscus.jpg", "http://en.wikipedia.org/wiki/Hibiscus")
                        }
            };
            return gm;
        }


        /// <summary>
        /// Creates a level definition specific to this collection
        /// </summary>
        /// <param name="title">Image title</param>
        /// <param name="description">Image description</param>
        /// <param name="filename">Image filename</param>
        /// <returns>Level definition using the parameters passed and default owner, cliense and scramble information</returns>
        private LevelDefinition Create(string title, string description, string filename, string uriString)
        {
            return new LevelDefinition
                {
                    XapImageUri = CreateImageUri(filename),
                    ImageTitle = title,
                    ImageText = description,
                    OwnerName = "Stephanie Brodie",
                    ImageLink = new Uri(uriString, UriKind.Absolute),
                    License = LicenseDefinition.ByNcNd30,
                    ScrambleType = Scrambler.ScrambleType.Shuffle
                };
        }

    }

}
