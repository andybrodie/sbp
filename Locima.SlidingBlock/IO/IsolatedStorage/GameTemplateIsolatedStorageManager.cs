using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.GameTemplates.SinglePlayer;
using NLog;

namespace Locima.SlidingBlock.IO.IsolatedStorage
{
    public class GameTemplateIsolatedStorageManager : IGameTemplateManager
    {
        public const string GameDefinitionDirectory = "GameDefinitions";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Initialise()
        {
            IOHelper.EnsureDirectory(GameDefinitionDirectory);
            EnsureSinglePlayerGame();
        }


        /// <summary>
        /// Ensure that the default single player game exists
        /// </summary>
        private void EnsureSinglePlayerGame()
        {
            GameDefinition singlePlayerGame = IOHelper.LoadFileByAppId<GameDefinition>(GameDefinitionDirectory,
                                                                       SinglePlayerGame.SinglePlayerGamePersistentId);
            if (singlePlayerGame == null)
            {
                Logger.Info("Creating default game definition");
                Save(SinglePlayerGame.Create());
            }
            else
            {
                Logger.Info("Verified that default game definition exists");
            }
        }


        public List<GameDefinition> GetCustomGameDefinitions()
        {
            List<GameDefinition> games = new List<GameDefinition>();
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                List<string> filenames = IOHelper.GetFileNames(GameDefinitionDirectory, store);
                foreach (string filename in filenames)
                {
                    games.Add(IOHelper.LoadObject<GameDefinition>(store, filename));
                }
            }
            return games;
        }

        /// <summary>
        /// Loads the game definition with the ID of <paramref name="id"/>
        /// </summary>
        /// <param name="id">The ID of the game definition</param>
        /// <returns>The loaded game definition object</returns>
        public GameDefinition Load(string id)
        {
            return IOHelper.LoadObject<GameDefinition>(id);
        }

        public void Save(GameDefinition game)
        {
            if (game == null) throw new ArgumentException("puzzle");
            if (string.IsNullOrEmpty(game.Id))
            {
                game.Id = IOHelper.CreatePath(GameDefinitionDirectory, Guid.NewGuid().ToString());
                Logger.Info("Saving new game definition {0} by {1}", game.Title, game.Author);
            }
            else
            {
                Logger.Info("Saving existing game definition {0} by {1}", game.Title, game.Author);
            }

            if (string.IsNullOrEmpty(game.Title))
            {
                game.Title = GameDefinition.DefaultTitle;
            }

            if (string.IsNullOrEmpty(game.Author))
            {
                PlayerStorageManager.Instance.EnsureCurrentPlayer();
                game.Author = PlayerStorageManager.Instance.CurrentPlayer.Name;
            }

            if (game.Levels ==null)
            {
                game.Levels = new List<LevelDefinition>(0);
            }

            IOHelper.SaveObject(game);

        }
    }
}