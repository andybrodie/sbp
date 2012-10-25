using Locima.SlidingBlock.IO.IsolatedStorage;

namespace Locima.SlidingBlock.IO
{
    /// <summary>
    /// Acts as a single broker for the <see cref="IImageStorageManager"/> singleton.
    /// </summary>
    public class ImageStorageManager
    {
        /// <summary>
        /// Return the <see cref="IImageStorageManager"/> singleton
        /// </summary>
        public static IImageStorageManager Instance { get; private set; }

        
        /// <summary>
        /// Set up <see cref="Instance"/> ready for by <see cref="StorageManagerManager.Initialise"/>.
        /// </summary>
        static ImageStorageManager()
        {
            Instance = new ImageIsolatedStorageManager();
        }
    }
}