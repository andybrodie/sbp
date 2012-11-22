using System;
using System.Collections.Generic;
using System.Diagnostics;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Scrambles;

namespace Locima.SlidingBlock.GameTemplates.SinglePlayer
{

    /// <summary>
    /// This programatically creates the default single player game within the application.
    /// </summary>
    /// <remarks>
    /// A "cleaner" way to do this would have been to generate the XML for the single player game "off-line" and just embed it as content within the XAP,
    /// however this way is simpler and the overhead of the creation (done during <see cref="IGameTemplateManager.Initialise"/>) is minimal.
    /// </remarks>
    public class SinglePlayerGame
    {
        private const string TemplatePath = "GameTemplates/SinglePlayer/";

        /// <summary>
        /// A fixed ID for this game as it only needs to get created once, so we need a reliable way of checking it when the application first starts
        /// </summary>
        public static readonly string SinglePlayerGamePersistentId = "33F575EC-5205-456E-81B9-E1CEABE9AEDF";

        /// <summary>
        /// Creates the game template for the default single player game.
        /// </summary>
        /// <remarks>
        /// Creates 3 levels, using the JPG files contained within the same directory (GameTemplates/SinglePlayer),
        /// adds the licensing information and sets up the <see cref="Scrambler.ScrambleType"/></remarks>
        public static GameTemplate Create()
        {
            const string andy = "Andy Brodie";
            Uri andyLink = new Uri("http://www.locima.co.uk");

            // If debugging then use a scramble that allows the level to be finished within one move, to make it easier to test stuff out
            Scrambler.ScrambleType scramble = Debugger.IsAttached
                                                  ? Scrambler.ScrambleType.OneMoveToFinish
                                                  : Scrambler.ScrambleType.XyFlip;

            GameTemplate gm = new GameTemplate
                {
                    IsReadOnly = true,
                    AppId = SinglePlayerGamePersistentId,  // We fix the ID here as we only want one instance of this definition
                    Title = LocalizationHelper.GetString("SinglePlayerGameTitle"),
                    Author = "Andy Brodie",
                    Levels = new List<LevelDefinition>(3)
                        {
                            new LevelDefinition
                                {
                                    XapImageUri = new Uri(TemplatePath + "1.jpg", UriKind.Relative),
                                    ImageTitle = "Dax the Labrador",
                                    ImageText =
                                        "My wife's dog, \"Dax\", an adorable, dopey yellow labrador",
                                    OwnerName = andy,
                                    OwnerUri = andyLink,
                                    License = LicenseDefinition.CcBy30,
                                    ScrambleType = scramble
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = new Uri(TemplatePath + "2.jpg", UriKind.Relative),
                                    ImageTitle = "Roman ruin",
                                    ImageText = "A holiday snap from when I went to Rome in 2010",
                                    OwnerName = andy,
                                    OwnerUri = andyLink,
                                    License = LicenseDefinition.CcBy30,
                                    ScrambleType = scramble
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = new Uri(TemplatePath + "3.jpg", UriKind.Relative),
                                    ImageTitle = "Flower",
                                    ImageText =
                                        "A flower in Exbury Gardens, Hampshire",
                                    OwnerName = andy,
                                    OwnerUri = andyLink,
                                    License = LicenseDefinition.CcBy30,
                                    ScrambleType = scramble
                                }
                        }
                };

            return gm;
        }
    }
}