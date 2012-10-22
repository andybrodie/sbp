using Locima.SlidingBlock.IO.IsolatedStorage;

namespace Locima.SlidingBlock.IO
{
    public class SaveGameStorageManager
    {
        public static ISaveGameStorageManager Instance { get; private set; }

        static SaveGameStorageManager()
        {
            Instance = new SaveGameIsolatedStorageManager();
        }
    }
}