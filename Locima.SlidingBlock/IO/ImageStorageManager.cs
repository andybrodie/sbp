using Locima.SlidingBlock.IO.IsolatedStorage;

namespace Locima.SlidingBlock.IO
{
    public class ImageStorageManager
    {
        public static IImageStorageManager Instance { get; private set; }

        static ImageStorageManager()
        {
            Instance = new ImageIsolatedStorageManager();
        }
    }
}