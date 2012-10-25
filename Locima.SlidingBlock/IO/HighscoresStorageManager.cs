using Locima.SlidingBlock.IO.IsolatedStorage;

namespace Locima.SlidingBlock.IO
{
    /// <summary>
    /// Acts as a single broker for the <see cref="IHighscoresStorageManager"/> singleton.
    /// </summary>
    public class HighScoresStorageManager
    {
        /// <summary>
        /// Return the <see cref="IHighscoresStorageManager"/> singleton
        /// </summary>
        public static IHighscoresStorageManager Instance { get; private set; }

        /// <summary>
        /// Set up <see cref="Instance"/> so the <see cref="IHighscoresStorageManager.Initialise"/> method can be invoked during application initialisation
        /// </summary>
        static HighScoresStorageManager()
        {
            Instance = new HighscoreIsolatedStorageManager();
        }
    }
}