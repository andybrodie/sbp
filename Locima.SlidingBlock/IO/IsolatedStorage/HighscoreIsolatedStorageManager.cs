using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Locima.SlidingBlock.Persistence;
using NLog;

namespace Locima.SlidingBlock.IO.IsolatedStorage
{

    /// <summary>
    /// Mnaages the persistence of a single high score table in isolated storage
    /// </summary>
    public class HighscoreIsolatedStorageManager : IHighscoresStorageManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string HighscoreFilename = "Highscores";

        public void Initialise()
        {
            Logger.Info("Initialising empty high scores table in isolated storage file {0}", HighscoreFilename);
            // Set up an empty highscore file
            HighScoreTable defaultTable = new HighScoreTable
                {
                    Id = HighscoreFilename,
                    Scores = new List<Highscore>()
                };
            Save(defaultTable);
        }


        public HighScoreTable Load()
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return IOHelper.LoadObject<HighScoreTable>(store, HighscoreFilename);
            }
        }


        public void Save(HighScoreTable highScoreTable)
        {
            IOHelper.SaveObject(highScoreTable);
        }


    }
}