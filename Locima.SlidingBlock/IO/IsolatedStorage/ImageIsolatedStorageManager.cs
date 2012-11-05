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
        /// Synchronously downloads an image from the XAP content (built-in content) to the image store in isolated storage
        /// </summary>
        /// <param name="uri"></param>
        public string DownloadFileFromXapContent(Uri uri)
        {
            string filename = ImageCatalogueIsolatedStorageManager.AddToCatalogue(uri);
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fileStream = store.CreateFile(filename))
                {
                    IOHelper.DownloadStream(uri, fileStream);
                }
            }
            return filename;
        }


        /// <summary>
        /// Loads the image specified by <paramref name="imageFilename"/> from Isolated Storage and return it as a <see cref="WriteableBitmap"/>
        /// </summary>
        /// <param name="imageFilename">The image to load, must not be null</param>
        /// <returns>A bitmap version of the image</returns>
        public WriteableBitmap LoadImage(string imageFilename)
        {
            Logger.Info("Loading {0}", imageFilename);
            BitmapImage bitmap = new BitmapImage();

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(imageFilename))
                {
                    throw new InvalidStateException("File {0} does not exist", imageFilename);
                }
                using (
                    IsolatedStorageFileStream fileStream = store.OpenFile(imageFilename, FileMode.Open, FileAccess.Read,
                                                                          FileShare.Read))
                {
                    bitmap.SetSource(fileStream);
                }
            }
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
            BitmapImage bitmap = new BitmapImage();
            bitmap.CreateOptions = BitmapCreateOptions.None;
            bitmap.UriSource = xapImageUri;
            WriteableBitmap wbitmap = new WriteableBitmap(bitmap);
            return wbitmap;
        }
    }
}