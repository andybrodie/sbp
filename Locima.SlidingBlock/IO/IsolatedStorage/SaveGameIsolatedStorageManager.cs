using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using Locima.SlidingBlock.Persistence;
using NLog;

namespace Locima.SlidingBlock.IO.IsolatedStorage
{
    public class SaveGameIsolatedStorageManager : ISaveGameStorageManager
    {
        private const string SaveGameDirectory = "SaveGames";
        private const string PathSeparator = "\\";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region ISaveGameStorageManager Members

        /// <summary>
        ///   Loads a puzzle from disk using only the filename
        /// </summary>
        /// <param name="saveGameFilename"> </param>
        /// <returns> </returns>
        public SaveGame Load(string saveGameFilename)
        {
            Logger.Info("Loading {0}", saveGameFilename);
            SaveGame saveGame = IOHelper.LoadObject<SaveGame>(saveGameFilename);
            return saveGame;
        }


        public void Initialise()
        {
            IOHelper.EnsureDirectory(SaveGameDirectory);
        }


        public void DeleteGame(SaveGame saveGame)
        {
            DeleteGame(saveGame.Id);
        }

        public void DeleteGame(string saveGameId)
        {
            Logger.Info("Deleting save game {0}", saveGameId);
            bool result = IOHelper.DeleteFile(saveGameId);
            Logger.Info("Save game {0} has {1}", saveGameId, result ? "been deleted successfully" : "not been deleted");
        }

        #endregion

        #region Save Game Management

        /// <summary>
        ///   Save the current game
        /// </summary>
        /// <remarks>
        ///   If the game should be saved in to a new file, then ensure that <see cref="Persistence.SaveGame.Id" /> is null or <see
        ///    cref="string.Empty" /> , otherwise game will be saved to whatever filename is specified in <see
        ///    cref="Persistence.SaveGame.Id" /> .
        /// </remarks>
        /// <param name="saveGame"> The puzzle to save, must not be null </param>
        public void SaveGame(SaveGame saveGame)
        {
            if (saveGame == null) throw new ArgumentNullException("saveGame");
            if (string.IsNullOrEmpty(saveGame.Id))
            {
                saveGame.Id = string.Format("{0}{1}{2}.puzzle", SaveGameDirectory, PathSeparator, Guid.NewGuid());
                Logger.Debug("Setting filename, as it's blank, to {0}", saveGame.Id);
            }
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IOHelper.SaveObject(saveGame, store);
            }
        }


        /// <summary>
        ///   Loads all the save games in to memory so the user can pick the one they want to play
        /// </summary>
        /// <returns> A list (never null, but possibly empty) of all the save games available, ordered by last access time, descending (most recent first) TODO Check that IEnumerable is "permitted" to allow ordering </returns>
        public IEnumerable<SaveGame> LoadGames(string playerId)
        {
            ICollection<SaveGame> puzzles = new List<SaveGame>();
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Rather unintuitively, this returns just the filename, not the full path
                string[] fileNames =
                    store.GetFileNames(string.Format("{0}{1}*.puzzle", SaveGameDirectory, PathSeparator));
                Logger.Info("Found {0} save games to load", fileNames.Length);

                foreach (string filename in fileNames)
                {
                    // Remember to put the path prefix back on, or you'll get a FileNotFoundException
                    SaveGame wibble = Load(string.Format("{0}{1}{2}", SaveGameDirectory, PathSeparator, filename));
                    if (wibble.LocalPlayer.PlayerDetailsId == playerId)
                    {
                        puzzles.Add(wibble);
                    }
                }
            }
            return puzzles.OrderByDescending(f => f.LastUpdate).ToList();
        }


        /// <summary>
        ///   Delete all the games associated with a player
        /// </summary>
        /// <param name="playerId"> </param>
        public void DeleteGames(string playerId)
        {
            foreach (SaveGame sg in LoadGames(playerId))
            {
                DeleteGame(sg);
            }
        }

        public IEnumerable<SaveGame> LoadGames()
        {
            return PlayerStorageManager.Instance.CurrentPlayer == null
                       ? new List<SaveGame>()
                       : LoadGames(PlayerStorageManager.Instance.CurrentPlayer.Id);
        }


        public SaveGame GetContinuableGame()
        {
            return LoadGames().FirstOrDefault();
        }

        #endregion
    }
}