using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Locima.SlidingBlock.Persistence;
using NLog;

namespace Locima.SlidingBlock.IO.IsolatedStorage
{

    /// <summary>
    /// Mnaages the persistence of a single high score table in isolated storage
    /// </summary>
    public class HighScoreIsolatedStorageManager : IHighScoresStorageManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string HighScoresFilename = "HighScores";

        /// <summary>
        /// Creates an empty high score table if one doesn't already exist
        /// </summary>
        public void Initialise()
        {
            if (!IOHelper.FileExists(HighScoresFilename))
            {
                Logger.Info("Initialising empty high scores table in isolated storage file {0}", HighScoresFilename);
                // Set up an empty high scores file
                HighScoreTable defaultTable = new HighScoreTable
                                                  {
                                                      Id = HighScoresFilename,
                                                      Scores = new List<HighScore>()
                                                  };
                Save(defaultTable);
            } else
            {
                Logger.Info("Found existing high score table in isolated storage file {0}", HighScoresFilename);
            }
        }


        /// <inheritdoc/>
        public HighScoreTable Load()
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return IOHelper.LoadObject<HighScoreTable>(store, HighScoresFilename);
            }
        }


        /// <inheritdoc/>
        public void Save(HighScoreTable highScoreTable)
        {
            IOHelper.SaveObject(highScoreTable);
        }


    }
}