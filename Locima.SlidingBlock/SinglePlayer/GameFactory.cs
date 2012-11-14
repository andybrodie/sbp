using System;
using System.Collections.Generic;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.GameTemplates.SinglePlayer;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Model;
using Locima.SlidingBlock.Persistence;
using Locima.SlidingBlock.Scrambles;
using NLog;

namespace Locima.SlidingBlock.SinglePlayer
{

    /// <summary>
    /// Creates a new <see cref="SinglePlayerGame"/> for the current player
    /// </summary>
    public class GameFactory
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Creates a new <see cref="SaveGame"/> instance for the <see cref="IPlayerStorageManager.CurrentPlayer"/>
        /// </summary>
        /// <remarks>
        /// TODO Don't pass in the <paramref name="tilesAcross"/> and <paramref name="tilesHigh"/>, instead pass a difficulty object and leave it up to the <see cref="SinglePlayerGame"/> to determine
        /// </remarks>
        /// <param name="tilesAcross">Number of tiles across in each puzzle</param>
        /// <param name="tilesHigh">Number of tiles high in each puzzle</param>
        /// <returns>A new save game</returns>
        public static SaveGame CreateSinglePlayerGame(int tilesAcross, int tilesHigh)
        {
            SaveGame sg = new SaveGame {GameType = SaveGame.GameTypes.SinglePlayer};

            // TODO ALlow the level definition to set the player tile
            PlayerStorageManager.Instance.EnsureCurrentPlayer();
            sg.LocalPlayer = new PlayerLink
                {
                    PlayerDetailsId = PlayerStorageManager.Instance.CurrentPlayer.Id,
                    Position = new Position {X = 0, Y = 0}
                };

            Logger.Info("Creating a new SaveGame for {0} using {1}x{2}", PlayerStorageManager.Instance.CurrentPlayer.Name, tilesAcross, tilesHigh);

            GameTemplate spgGame = SinglePlayerGame.Create();

            sg.Levels = new List<LevelState>(spgGame.Levels.Count);
            foreach (LevelDefinition levelDefinition in spgGame.Levels)
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
                    SolvedTilePositions = Scrambler.Instance.Scramble(levelDefinition.ScrambleType, tilesAcross, tilesHigh)
                };
            return level;
        }
    }
}