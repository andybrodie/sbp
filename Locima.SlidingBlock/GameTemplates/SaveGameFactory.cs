using System;
using System.Collections.Generic;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.GameTemplates.SinglePlayer;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Model;
using Locima.SlidingBlock.Persistence;
using Locima.SlidingBlock.Scrambles;
using NLog;

namespace Locima.SlidingBlock.GameTemplates
{
    /// <summary>
    /// Creates a new <see cref="SinglePlayerGame"/> for the current player
    /// </summary>
    public class SaveGameFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Creates a new <see cref="SaveGame"/> based on the <see cref="GameTemplate"/> identified by it's ID in <paramref name="template"/>
        /// </summary>
        /// <param name="template">The template to use to create the game</param>
        /// <param name="tilesAcross">Number of tiles across in each puzzle</param>
        /// <param name="tilesHigh">Number of tiles high in each puzzle</param>
        /// <returns>A new instance of the game, ready for use on <see cref="GamePage"/></returns>
        public static SaveGame CreateSaveGame(GameTemplate template, int tilesAcross, int tilesHigh)
        {
            SaveGame sg = new SaveGame {GameType = SaveGame.GameTypes.SinglePlayer, GameDefinitionId = template.Id};

            // TODO Allow the level definition to set the player tile
            sg.LocalPlayer = new PlayerLink
                                 {
                                     PlayerDetailsId = PlayerStorageManager.Instance.CurrentPlayer.Id,
                                     Position = new Position {X = 0, Y = 0}
                                 };

            Logger.Info("Creating a new SaveGame for {0} using {1} x {2} tile grid",
                        PlayerStorageManager.Instance.CurrentPlayer.Name, tilesAcross, tilesHigh);

            sg.Levels = new List<LevelState>(template.Levels.Count);
            foreach (LevelDefinition levelDefinition in template.Levels)
            {
                sg.Levels.Add(CreateLevel(levelDefinition, tilesAcross, tilesHigh));
            }

            SaveGameStorageManager.Instance.SaveGame(sg);
            Logger.Info("Created a new SaveGame ({0}) with {1} levels", sg.Id, sg.Levels.Count);
            return sg;
        }


        private static LevelState CreateLevel(LevelDefinition levelDefinition, int tilesAcross, int tilesHigh)
        {
            LevelState level = new LevelState
                                   {
                                       ImageId = levelDefinition.ImageId,
                                       XapImageUri = levelDefinition.XapImageUri,
                                       ElapsedTime = new TimeSpan(0),
                                       SolvedTilePositions =
                                           Scrambler.Instance.Scramble(levelDefinition.ScrambleType, tilesAcross,
                                                                       tilesHigh),
                                       Title = levelDefinition.ImageText,
                                       Text = levelDefinition.ImageText,
                                       License = levelDefinition.License
                                   };
            return level;
        }

    }
}