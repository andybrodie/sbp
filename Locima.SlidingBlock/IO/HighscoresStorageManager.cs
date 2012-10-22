using Locima.SlidingBlock.IO.IsolatedStorage;

namespace Locima.SlidingBlock.IO
{
    public class HighScoresStorageManager
    {
        public static IHighscoresStorageManager Instance { get; private set; }

        static HighScoresStorageManager()
        {
            Instance = new HighscoreIsolatedStorageManager();
        }
    }
}