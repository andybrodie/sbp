using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.IsolatedStorage;
using System.Text;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.GameTemplates.SinglePlayer;
using NLog;

namespace Locima.SlidingBlock.IO.IsolatedStorage
{
    public class GameTemplateIsolatedStorageManager : IGameTemplateManager
    {
        public const string GameTemplateDirectory = "GameTemplates";
        protected const string ShadowSuffix = ".shadow";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Initialise()
        {
            IOHelper.EnsureDirectory(GameTemplateDirectory);
            EnsureSinglePlayerGame();
        }


        public List<GameTemplate> GetGameTemplates(bool includeShadows, bool collapseShadows)
        {
            List<GameTemplate> games = new List<GameTemplate>();
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                List<string> filenames = IOHelper.GetFileNames(GameTemplateDirectory, store);
                foreach (string filename in filenames)
                {
                    GameTemplate gameTemplate = IOHelper.LoadObject<GameTemplate>(store, filename);
                    games.Add(gameTemplate);
                }
            }
            if (!includeShadows)
            {
                games = games.Where(template => !template.IsShadow).ToList();
            }
            // Only worth collapsing shadows if shadows are included, so using an "else" here
            else if (collapseShadows)
            {
                // Remove any games which have shadows, so only the shadows are shown
                games = CollapseShadows(games);
            }
            return games;
        }


        /// <summary>
        /// Remove all game templates for which a shadow exists
        /// </summary>
        /// <param name="games"></param>
        private List<GameTemplate> CollapseShadows(List<GameTemplate> games)
        {
            DebugList("Original set", games);
            // TODO I'm not very good at LINQ, can someone improve this?
            // I want to return a list of all the games which don't have a shadow, i.e. a game template T should be in the list if and only if
            // there isn't another game template U where U.ShadowOf==T.Id
            // Get a list of all the objects which shouldn't appear in the list
            List<GameTemplate> removeList = games.Where(game => games.Any(template => game.Id == template.ShadowOf)).ToList();
            DebugList("Removing these", removeList);

            // Return the set difference between that list and the original list
            List<GameTemplate> collapsedList = games.Except(removeList).ToList();
            DebugList("Collapsed list", collapsedList);
            return collapsedList;
        }


        /// <summary>
        /// Dumps out the <see cref="GameTemplate.Id"/> and <see cref="GameTemplate.ShadowOf"/> values for the set of <see cref="GameTemplate"/> instances in <paramref name="templates"/>
        /// to the debug trace
        /// </summary>
        /// <param name="title">A string to write out first</param>
        /// <param name="templates">The templates to enumerate</param>
        private static void DebugList(string title, IEnumerable<GameTemplate> templates)
        {
            StringBuilder buffer = new StringBuilder();
            buffer.AppendFormat("{0}:\n", title);
            foreach (GameTemplate template in templates)
            {
                buffer.AppendFormat("{0} - {1}\n", template.Id, template.ShadowOf);
            }
            Logger.Debug(buffer.ToString());
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


        private string CreateId(bool isShadow)
        {
            string Id = IOHelper.CreatePath(GameTemplateDirectory, Guid.NewGuid().ToString());
            if (isShadow)
            {
                Id += ShadowSuffix;
            }
            return Id;
        }


        public void Save(GameTemplate gameTemplate)
        {
            if (gameTemplate == null) throw new ArgumentException("puzzle");
            if (string.IsNullOrEmpty(gameTemplate.Id))
            {
                gameTemplate.Id = CreateId(gameTemplate.IsShadow);
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

            if (gameTemplate.Levels == null)
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


        public string PromoteShadow(GameTemplate shadowTemplate)
        {
            if (!shadowTemplate.IsShadow)
            {
                // This isn't a shadow, so you can't promote it!
                throw new InvalidStateException("Passed non-shadow to PromoteShadow {0}", shadowTemplate);
            }

            // This object is no longer a shadow
            shadowTemplate.IsShadow = false;

            if (!string.IsNullOrEmpty(shadowTemplate.ShadowOf))
            {
                // If we are a shadow of an existing object, then replace it
                string shadowId = shadowTemplate.Id;
                if (string.IsNullOrEmpty(shadowId))
                {
                    // If we were never saved, then we won't have our own ID, so there's nothing to clean up
                    Logger.Info("Promoting unsaved shadow to {0}", shadowTemplate.ShadowOf);
                    shadowTemplate.Id = shadowTemplate.ShadowOf;
                    shadowTemplate.ShadowOf = null;
                    Save(shadowTemplate);
                }
                else
                {
                    // If we were saved, then same logic, but have to delete the shadow once we've promoted
                    Logger.Info("Promoting shadow {0} to {1}, deleting the shadow", shadowId, shadowTemplate.ShadowOf);
                    shadowTemplate.Id = shadowTemplate.ShadowOf;
                    shadowTemplate.ShadowOf = null;
                    Save(shadowTemplate);
                    Delete(shadowId);
                }
            }
            else
            {
                /* We aren't shadowing an existing object, so just reset the IsShadow and save as normal, allocating an ID if we've never
                 * saved this shadow before automatically
                 */
                Logger.Info("Promoting new shadow {0}", shadowTemplate);
                shadowTemplate.IsShadow = false;
                Save(shadowTemplate);
            }
            return shadowTemplate.Id;
        }


        /// <summary>
        /// Ensure that the default single player game exists
        /// </summary>
        private void EnsureSinglePlayerGame()
        {
            GameTemplate singlePlayerGame = IOHelper.LoadFileByAppId<GameTemplate>(GameTemplateDirectory,
                                                                                   SinglePlayerGame
                                                                                       .SinglePlayerGamePersistentId);
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
    }
}