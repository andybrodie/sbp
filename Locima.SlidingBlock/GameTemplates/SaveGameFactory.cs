using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// Creates a new <see cref="SaveGame"/> based on the <see cref="GameTemplate"/> identified by it's ID in <paramref name="gameTemplateId"/>
        /// </summary>
        /// <remarks>Delegates to <see cref="CreateSaveGame(GameTemplate,int,int)"/></remarks>
        /// <param name="gameTemplateId">The ID of the game template to use to create the game</param>
        /// <param name="tilesAcross">Number of tiles across in each puzzle</param>
        /// <param name="tilesHigh">Number of tiles high in each puzzle</param>
        /// <returns>A new instance of the game, ready for use on <see cref="GamePage"/></returns>
        public static SaveGame CreateSaveGame(string gameTemplateId, int tilesAcross, int tilesHigh)
        {
            GameTemplate gameTemplate = GameTemplateStorageManager.Instance.Load(gameTemplateId);
            return CreateSaveGame(gameTemplate, tilesAcross, tilesHigh);
        }

        /// <summary>
        /// Creates a new <see cref="SaveGame"/> based on the <see cref="GameTemplate"/> passed in <paramref name="gameTemplate"/>
        /// </summary>
        /// <param name="gameTemplate">The template to use to create the game</param>
        /// <param name="tilesAcross">Number of tiles across in each puzzle</param>
        /// <param name="tilesHigh">Number of tiles high in each puzzle</param>
        /// <returns>A new instance of the game, ready for use on <see cref="GamePage"/></returns>
        public static SaveGame CreateSaveGame(GameTemplate gameTemplate, int tilesAcross, int tilesHigh)
        {
            SaveGame sg = new SaveGame {GameType = SaveGame.GameTypes.SinglePlayer, GameDefinitionId = gameTemplate.Id};

            // TODO Allow the level definition to set the player tile
            sg.LocalPlayer = new PlayerLink
                                 {
                                     PlayerDetailsId = PlayerStorageManager.Instance.CurrentPlayer.Id,
                                     Position = new Position {X = 0, Y = 0}
                                 };

            Logger.Info("Creating a new SaveGame for {0} using {1} x {2} tile grid",
                        PlayerStorageManager.Instance.CurrentPlayer.Name, tilesAcross, tilesHigh);

            sg.Levels = new List<LevelState>(gameTemplate.Levels.Count);
            foreach (LevelDefinition levelDefinition in gameTemplate.Levels)
            {
                sg.Levels.Add(CreateLevel(levelDefinition, tilesAcross, tilesHigh));
            }

            SaveGameStorageManager.Instance.SaveGame(sg);
            Logger.Info("Created a new SaveGame ({0}) with {1} levels", sg.Id, sg.Levels.Count);
            return sg;
        }


        private static LevelState CreateLevel(LevelDefinition levelDefinition, int tilesAcross, int tilesHigh)
        {
            Scrambler.ScrambleType scrambleType = Debugger.IsAttached
                                         ? Scrambler.ScrambleType.OneMoveToFinish
                                         : levelDefinition.ScrambleType;
            Logger.Info("Creating LevelState from LevelDefinition {0} for {1}x{2} using scramble {3}", levelDefinition, tilesAcross, tilesHigh, scrambleType);

            LevelState level = new LevelState
                                   {
                                       ImageId = levelDefinition.ImageId,
                                       XapImageUri = levelDefinition.XapImageUri,
                                       ElapsedTime = new TimeSpan(0),
                                       SolvedTilePositions = Scrambler.Instance.Scramble(scrambleType, tilesAcross, tilesHigh),
                                       Title = levelDefinition.ImageTitle,
                                       Text = levelDefinition.ImageText,
                                       License = levelDefinition.License
                                   };
            return level;
        }

    }
}