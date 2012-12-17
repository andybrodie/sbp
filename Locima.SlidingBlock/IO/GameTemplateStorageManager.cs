using Locima.SlidingBlock.IO.IsolatedStorage;

namespace Locima.SlidingBlock.IO
{

    /// <summary>
    /// Acts as a single broker for the <see cref="IGameTemplateManager"/> singleton.
    /// </summary>
    public class GameTemplateStorageManager
    {
        static GameTemplateStorageManager()
        {
            Instance = new GameTemplateIsolatedStorageManager();
        }


        /// <summary>
        /// The singleton instance of the game template manager for this app
        /// </summary>
        public static IGameTemplateManager Instance { get; private set; }
    }
}