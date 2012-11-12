using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.Common;
using NLog;

namespace Locima.SlidingBlock.IO.IsolatedStorage
{
    /// <summary>
    /// Currently unused, this is used for managing the persistence of downloaded image files (either from the Internet or from within XAP content)
    /// </summary>
    public class ImageIsolatedStorageManager : IImageStorageManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// The directory within which all downloaded images will be stored
        /// </summary>
        public static string ImageDirectory = "Images";


        /// <summary>
        /// Ensures that <see cref="ImageDirectory"/> exists
        /// </summary>
        public void Initialise()
        {
            IOHelper.EnsureDirectory(ImageDirectory);
        }



        /// <summary>
        /// Loads the image specified by <paramref name="imageId"/> from Isolated Storage and return it as a <see cref="WriteableBitmap"/>
        /// </summary>
        /// <param name="imageId">The image to load, must not be null</param>
        /// <returns>A bitmap version of the image</returns>
        public WriteableBitmap LoadImage(string imageId)
        {
            Logger.Info("Loading image {0} from isolated storage", imageId);
            BitmapImage bitmap = new BitmapImage();

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(imageId))
                {
                    throw new InvalidStateException("File {0} does not exist", imageId);
                }
                using (
                    IsolatedStorageFileStream fileStream = store.OpenFile(imageId, FileMode.Open, FileAccess.Read,
                                                                          FileShare.Read))
                {
                    bitmap.SetSource(fileStream);
                }
            }
            Logger.Info("Loaded image {0} from isolated storage successfully");
            return new WriteableBitmap(bitmap);
        }

        /// <summary>
        /// Loads the image from the Uri passed in <paramref name="xapImageUri"/> and returns it as a <see cref="WriteableBitmap"/>
        /// </summary>
        /// <param name="xapImageUri">The Uri of the image contained within the XAP for this app</param>
        /// <returns>A bitmap</returns>
        public WriteableBitmap LoadImage(Uri xapImageUri)
        {
            // TODO Check performance to make sure that sync loading is ok.  Should be quick as the image is small and it's local
            Logger.Info("Loading image from XAP content {0}", xapImageUri);
            BitmapImage bitmap = new BitmapImage();
            bitmap.CreateOptions = BitmapCreateOptions.None;
            bitmap.UriSource = xapImageUri;
            WriteableBitmap wbitmap = new WriteableBitmap(bitmap);
            Logger.Debug("Loaded image from XAP content {0} successfully", xapImageUri);
            return wbitmap;
        }

        public string Save(Stream imageStream)
        {
            string filename = Path.Combine(ImageDirectory, Guid.NewGuid().ToString());
            byte[] data = IOHelper.DownloadStream(imageStream);
            IOHelper.Save(filename, data);
            return filename;
        }

        public string Save(WriteableBitmap image)
        {
            throw new NotImplementedException();
        }
    }
}