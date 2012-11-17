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
        public const string GameTemplateDirectory = "GameTemplates";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Initialise()
        {
            IOHelper.EnsureDirectory(GameTemplateDirectory);
            EnsureSinglePlayerGame();
        }


        /// <summary>
        /// Ensure that the default single player game exists
        /// </summary>
        private void EnsureSinglePlayerGame()
        {
            GameTemplate singlePlayerGame = IOHelper.LoadFileByAppId<GameTemplate>(GameTemplateDirectory,
                                                                       SinglePlayerGame.SinglePlayerGamePersistentId);
            if (singlePlayerGame == null)
            {
                Logger.Info("Creating default game template");
                Save(SinglePlayerGame.Create());
            }
            else
            {
                Logger.Info("Verified that default game template exists");
            }
        }


        public List<GameTemplate> GetGameTemplates()
        {
            List<GameTemplate> games = new List<GameTemplate>();
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                List<string> filenames = IOHelper.GetFileNames(GameTemplateDirectory, store);
                foreach (string filename in filenames)
                {
                    games.Add(IOHelper.LoadObject<GameTemplate>(store, filename));
                }
            }
            return games;
        }

        /// <summary>
        /// Loads the game template with the ID of <paramref name="id"/>
        /// </summary>
        /// <param name="id">The ID of the game template</param>
        /// <returns>The loaded game template object</returns>
        public GameTemplate Load(string id)
        {
            return IOHelper.LoadObject<GameTemplate>(id);
        }


        public void Save(GameTemplate gameTemplate)
        {
            if (gameTemplate == null) throw new ArgumentException("puzzle");
            if (string.IsNullOrEmpty(gameTemplate.Id))
            {
                gameTemplate.Id = IOHelper.CreatePath(GameTemplateDirectory, Guid.NewGuid().ToString());
                Logger.Info("Saving new game template {0} by {1}", gameTemplate.Title, gameTemplate.Author);
            }
            else
            {
                Logger.Info("Saving existing game template {0} by {1}", gameTemplate.Title, gameTemplate.Author);
            }

            if (string.IsNullOrEmpty(gameTemplate.Title))
            {
                gameTemplate.Title = GameTemplate.DefaultTitle;
            }

            if (string.IsNullOrEmpty(gameTemplate.Author))
            {
                gameTemplate.Author = PlayerStorageManager.Instance.CurrentPlayer.Name;
            }

            if (gameTemplate.Levels ==null)
            {
                gameTemplate.Levels = new List<LevelDefinition>(0);
            }

            IOHelper.SaveObject(gameTemplate);

        }

        public List<string> GetGameTemplateIds()
        {
            return IOHelper.GetFileNames(GameTemplateDirectory);
        }

        /// <summary>
        /// Deletes a game template
        /// </summary>
        /// <param name="id">The identity of the game template</param>
        public void Delete(string id)
        {
            IOHelper.DeleteFile(id);
        }
    }
}