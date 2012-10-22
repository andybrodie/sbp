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
    public class GameFactory
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static SaveGame CreateSinglePlayerGame(int tilesAcross, int tilesHigh)
        {
            SaveGame sg = new SaveGame();
            sg.GameType = SaveGame.GameTypes.SinglePlayer;

            // TODO ALlow the level definition to set the player tile
            PlayerStorageManager.Instance.EnsureCurrentPlayer();
            sg.LocalPlayer = new PlayerLink
                {
                    PlayerDetailsId = PlayerStorageManager.Instance.CurrentPlayer.Id,
                    Position = new Position {X = 0, Y = 0}
                };

            GameDefinition spgGame = SinglePlayerGame.Create();

            sg.Levels = new List<LevelState>(spgGame.Levels.Count);
            foreach (LevelDefinition levelDefinition in spgGame.Levels)
            {
                sg.Levels.Add(CreateLevel(levelDefinition, tilesAcross, tilesHigh));
            }

            SaveGameStorageManager.Instance.SaveGame(sg);
            return sg;
        }


        private static LevelState CreateLevel(LevelDefinition levelDefinition, int tilesAcross, int tilesHigh)
        {
            LevelState level = new LevelState
                {
                    IsolatedStorageFilename = levelDefinition.IsolatedStorageFilename,
                    ElapsedTime = new TimeSpan(0),
                    XapImageUri = levelDefinition.XapImageUri,
                    SolvedTilePositions =
                        Scrambler.Instance.Scramble(levelDefinition.ScrambleType, tilesAcross, tilesHigh)
                };
            return level;
        }
    }
}