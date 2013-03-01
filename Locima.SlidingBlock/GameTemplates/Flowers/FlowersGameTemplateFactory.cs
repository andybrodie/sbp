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
                            Create("Flower 1", "Flower 1", "3.jpg"),
                            Create("Flower 2", "Flower 2", "Lillies.jpg"),
                            Create("Flower 3", "Flower 3", "Lillies2.jpg"),
                            Create("Flower 4", "Flower 4", "Purple.jpg"),
                            Create("Flower 5", "Flower 5", "Rose.jpg"),
                            Create("Flower 6", "Flower 6", "Roses.jpg"),
                            Create("Flower 7", "Flower 7", "YellowFlower.jpg")
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
        private LevelDefinition Create(string title, string description, string filename)
        {
            return new LevelDefinition
                {
                    XapImageUri = CreateImageUri(filename),
                    ImageTitle = title,
                    ImageText = description,
                    OwnerName = "Stephanie Brodie",
                    OwnerUri = null,
                    License = LicenseDefinition.ByNcNd30,
                    ScrambleType = Scrambler.ScrambleType.Shuffle
                };
        }

    }

}
