using Locima.SlidingBlock.IO.IsolatedStorage;

namespace Locima.SlidingBlock.IO
{
    public class GameTemplateStorageManager
    {
        static GameTemplateStorageManager()
        {
            Instance = new GameTemplateIsolatedStorageManager();
        }

        public static IGameTemplateManager Instance { get; private set; }
    }
}