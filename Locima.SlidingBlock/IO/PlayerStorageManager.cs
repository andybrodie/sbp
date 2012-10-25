using Locima.SlidingBlock.IO.IsolatedStorage;

namespace Locima.SlidingBlock.IO
{

    /// <summary>
    /// Acts as a single broker for the <see cref="IPlayerStorageManager"/> singleton.
    /// </summary>
    public class PlayerStorageManager
    {
        /// <summary>
        /// Return the <see cref="IPlayerStorageManager"/> singleton
        /// </summary>
        public static IPlayerStorageManager Instance { get; private set; }

        /// <summary>
        /// Set up <see cref="Instance"/> so the <see cref="IPlayerStorageManager.Initialise"/> method can be invoked during application initialisation
        /// </summary>
        static PlayerStorageManager()
        {
            Instance = new PlayerIsolatedStorageManager();
        }       
    }
}