using Locima.SlidingBlock.IO.IsolatedStorage;

namespace Locima.SlidingBlock.IO
{
    /// <summary>
    /// Acts as a single broker for the <see cref="IHighScoresStorageManager"/> singleton.
    /// </summary>
    public class HighScoresStorageManager
    {
        /// <summary>
        /// Return the <see cref="IHighScoresStorageManager"/> singleton
        /// </summary>
        public static IHighScoresStorageManager Instance { get; private set; }

        /// <summary>
        /// Set up <see cref="Instance"/> so the <see cref="IStorageManager.Initialise"/> method can be invoked during application initialisation
        /// </summary>
        static HighScoresStorageManager()
        {
            Instance = new HighScoreIsolatedStorageManager();
        }
    }
}