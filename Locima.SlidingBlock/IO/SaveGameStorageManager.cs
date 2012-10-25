using Locima.SlidingBlock.IO.IsolatedStorage;

namespace Locima.SlidingBlock.IO
{

    /// <summary>
    /// Acts as a single broker for the <see cref="ISaveGameStorageManager"/> singleton.
    /// </summary>
    public class SaveGameStorageManager
    {
        /// <summary>
        /// Return the <see cref="ISaveGameStorageManager"/> singleton
        /// </summary>
        public static ISaveGameStorageManager Instance { get; private set; }

        /// <summary>
        /// Set up <see cref="Instance"/> so the <see cref="ISaveGameStorageManager.Initialise"/> method can be invoked during application initialisation
        /// </summary>
        static SaveGameStorageManager()
        {
            Instance = new SaveGameIsolatedStorageManager();
        }
    }
}