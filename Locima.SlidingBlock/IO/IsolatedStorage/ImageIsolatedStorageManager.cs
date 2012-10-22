using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.Common;
using NLog;

namespace Locima.SlidingBlock.IO.IsolatedStorage
{
    public class ImageIsolatedStorageManager : IImageStorageManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string ImageDirectory = "Images";

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