using Locima.SlidingBlock.IO.IsolatedStorage;

namespace Locima.SlidingBlock.IO
{
    public class PlayerStorageManager
    {
        public static IPlayerStorageManager Instance { get; private set; }

        static PlayerStorageManager()
        {
            Instance = new PlayerIsolatedStorageManager();
        }       
    }
}