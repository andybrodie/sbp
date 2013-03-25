using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.IsolatedStorage;
using System.Text;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.GameTemplates;
using NLog;

namespace Locima.SlidingBlock.IO.IsolatedStorage
{

    /// <summary>
    /// Manages the storage of game templates in isolated storage
    /// </summary>
    public class GameTemplateIsolatedStorageManager : IGameTemplateManager
    {

        /// <summary>
        /// The directory that game templates will be stored in
        /// </summary>
        public const string GameTemplateDirectory = "GameTemplates";

        /// <summary>
        /// The suffix to all shadow files
        /// </summary>
        protected const string ShadowSuffix = ".shadow";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Ensures that <see cref="GameTemplateDirectory"/> exists and that the default games exist (<see cref="EnsureBuiltInTemplatesExist"/>
        /// </summary>
        public void Initialise()
        {
            IOHelper.EnsureDirectory(GameTemplateDirectory);
            CheckAndRemoveOrphanImages();
            EnsureBuiltInTemplatesExist();
        }

        /// <summary>
        /// Delete all images that aren't referred to by game template
        /// </summary>
        /// <remarks>
        /// This method is as quick as it realistically can be, but it's still a concern that once there are lots of images and templates it could significantly slow down
        /// the startup of the app which isn't a good idea</remarks>
        private void CheckAndRemoveOrphanImages()
        {
            // 1. Assemble a list of all the images that are in use by all the game templates
            List<string> imageIdsInUse =
                GetGameTemplates(true, false, true)
                    .SelectMany(template => template.Levels)
                    .Select(definition => definition.ImageId)
                    .Distinct().ToList();

            // 2. Assemble a list of all the image IDs
            List<string> imageIds = ImageStorageManager.Instance.ListImages(true).ToList();

            // 3. Create a list of all the images from (2) that aren't in (1) 
            // Use an array so we can use String.Join later for logging
            string[] imagesToDelete = imageIds.Except(imageIdsInUse).ToArray();

            if (imagesToDelete.Length > 0)
            {
                // 4. Delete the unused images
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Deleting {0} unused images: {1} leaving a total of {2} images in use", imagesToDelete.Length,
                                string.Join(", ", imagesToDelete), imageIds.Count - imagesToDelete.Length);
                }
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
                imagesToDelete.Select(IOHelper.DeleteFile);
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
            }
            else
            {
                Logger.Info("No orphan images found to remove.  Found {0} total images", imageIdsInUse.Count);
            }
        }


        /// <summary>
        /// Retrieve a list of all the game templates in isolated storage
        /// </summary>
        /// <param name="includeShadows">If <c>true</c> then shadow templates will be included in the list</param>
        /// <param name="collapseShadows">Only used if <paramref name="includeShadows"/> is <c>true</c>, if this is <c>true</c> then any templates which
        /// have shadows are ommitted from the list</param>
        /// <param name="includeUnplayable">If <c>true</c> then invlid game templates (e.g. ones with no levels) will be included. </param>
        /// <returns></returns>
        public List<GameTemplate> GetGameTemplates(bool includeShadows, bool collapseShadows, bool includeUnplayable)
        {
            List<GameTemplate> games = new List<GameTemplate>();
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                List<string> filenames = IOHelper.GetFileNames(GameTemplateDirectory, store);
                games.AddRange(filenames.Select(filename => IOHelper.LoadObject<GameTemplate>(store, filename)));
            }
            if (!includeUnplayable)
            {
                games = games.Where(template => template.Levels != null && template.Levels.Count > 0).ToList();
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
            string id = IOHelper.CreatePath(GameTemplateDirectory, Guid.NewGuid().ToString());
            if (isShadow)
            {
                id += ShadowSuffix;
            }
            return id;
        }


        /// <summary>
        /// Saves the game template passed in <paramref name="gameTemplate"/>
        /// </summary>
        /// <param name="gameTemplate">The template to save, must not be null</param>
        public void Save(GameTemplate gameTemplate)
        {
            if (gameTemplate == null) throw new ArgumentException("puzzle");
            if (string.IsNullOrEmpty(gameTemplate.Id))
            {
                gameTemplate.Id = CreateId(gameTemplate.IsShadow);
                if (gameTemplate.IsShadow)
                {
                    Logger.Info("Saving new game template {0} by {1}", gameTemplate.Title, gameTemplate.Author);
                }
                else
                {
                    Logger.Info("Saving new game template {0} by {1} as a shadow of {2}", gameTemplate.Title, gameTemplate.Author, gameTemplate.ShadowOf);
                }
            }
            else
            {
                Logger.Info("Saving existing game template {0} by {1}", gameTemplate.Title, gameTemplate.Author);
            }

            // Fill in some sensible defaults if the user hasn't
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


        /// <summary>
        /// Deletes a game template
        /// </summary>
        /// <remarks>
        /// Note that we don't tidy up images that might be orphaned by the deletion of the game template.  We do that in the initialiser of the GameTemplateSelector, as we're
        /// loading all the templates anyway at that stage. </remarks>
        /// <param name="id">The identity of the game template</param>
        public void Delete(string id)
        {
            Logger.Info("Deleting game template file {0}", id);
            IOHelper.DeleteFile(id);
            RemoveOrphanedImages();
        }


        /// <summary>
        /// Searches for all orphaned images (i.e. images which don't have a reference in a <see cref="LevelDefinition.ImageId"/> within a <see cref="GameTemplate"/>
        /// </summary>
        private void RemoveOrphanedImages()
        {
            // Load *all* the game templates, valid or not, shadow or not
            List<GameTemplate> allTemplates = GetGameTemplates(true, false, true);
            // Get a list of al the images used across all game templates
            IEnumerable<string> imagesReferenced = (from gameTemplate in allTemplates 
                                     from level in gameTemplate.Levels where !string.IsNullOrEmpty(level.ImageId)
                                     select level.ImageId).Distinct();

            // Get a list of all the images
            IEnumerable<string> imagesFound = ImageStorageManager.Instance.ListImages(false);

            // All the images found minus all the images used equals the list of orphaned images to delete
            List<string> unreferencedImages = imagesFound.Except(imagesReferenced).ToList();

            Logger.Info("Deleting {0} unreferenced images", unreferencedImages.Count);
            foreach (string imageId in unreferencedImages)
            {
                Logger.Info("Deleting unreferenced image: {0}", imageId);
                ImageStorageManager.Instance.Delete(imageId);
            }
        }


        /// <summary>
        /// Deletes a game template and all associated images
        /// </summary>
        /// <param name="gameTemplate">The game template to delete</param>
        public void Delete(GameTemplate gameTemplate)
        {
            Delete(gameTemplate.Id);
        }


        /// <summary>
        /// Promotes the shadow template passed over the original template (if it exists), so any changes made in <see cref="GameEditor"/> or <see cref="LevelEditor"/> are "committed"
        /// </summary>
        /// <param name="shadowTemplate">The template to promote, must not be null</param>
        /// <returns>The ID of the committed template</returns>
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
        /// Ensure that the passed game template is available
        /// </summary>
        private void EnsureGameTemplate(IGameTemplateFactory factory)
        {
            GameTemplate template = IOHelper.LoadFileByAppId<GameTemplate>(GameTemplateDirectory, factory.PersistentId);
            
            if (template == null)
            {
                Logger.Info("Could not find persisted version of {0} ({1}), therefore creating it", factory, factory.PersistentId);
                Save(factory.Create());
            }
            else
            {
                Logger.Info("Verified that {0} game template exists", factory);
            }
        }

        /// <summary>
        /// Ensures that all built in game templates are available
        /// </summary>
        private void EnsureBuiltInTemplatesExist()
        {
            Logger.Info("Finding built-in games");
            foreach (IGameTemplateFactory factory in ReflectionHelper.CreateInstancesOf<IGameTemplateFactory>())
            {
                EnsureGameTemplate(factory);
            }

        }


    }
}