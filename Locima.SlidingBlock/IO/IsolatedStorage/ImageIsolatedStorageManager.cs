using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
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
        /// The directory within which all temporary images will be stored
        /// </summary>
        public static string ImageTempDirectory = "TempImages";


        /// <summary>
        /// Ensures that <see cref="ImageDirectory"/> exists
        /// </summary>
        public void Initialise()
        {
            IOHelper.EnsureDirectory(ImageDirectory);
            if (!IOHelper.EnsureDirectory(ImageTempDirectory))
            {
                DeleteAllTemporary();
            }
        }



        /// <summary>
        /// Loads the image specified by <paramref name="imageId"/> from Isolated Storage and return it as a <see cref="WriteableBitmap"/>
        /// </summary>
        /// <param name="imageId">The image to load, must not be null</param>
        /// <returns>A bitmap version of the image</returns>
        public WriteableBitmap Load(string imageId)
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
            Logger.Info("Loaded image {0} from isolated storage successfully", imageId);
            return new WriteableBitmap(bitmap);
        }

        /// <summary>
        /// Loads the image from the Uri passed in <paramref name="xapImageUri"/> and returns it as a <see cref="WriteableBitmap"/>
        /// </summary>
        /// <param name="xapImageUri">The Uri of the image contained within the XAP for this app</param>
        /// <returns>A bitmap</returns>
        public WriteableBitmap Load(Uri xapImageUri)
        {
            return ImageHelper.LoadBitmapFromXapContent(xapImageUri);
        }

        public string Save(Stream imageStream)
        {
            string filename = Path.Combine(ImageDirectory, Guid.NewGuid().ToString());
            IOHelper.Save(filename, imageStream);
            return filename;
        }

        
        public string Save(WriteableBitmap image)
        {
            string filename = Path.Combine(ImageDirectory, Guid.NewGuid().ToString());
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = store.OpenFile(filename, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    image.SaveJpeg(stream, image.PixelWidth, image.PixelHeight, 0, ImageHelper.JpegQuality);
                }

            }
            return filename;
        }


        public string SaveTemporary(Stream imageStream)
        {
            string filename = Path.Combine(ImageTempDirectory, Guid.NewGuid().ToString());
            Logger.Info("Saving temporary image {0}", filename);
            IOHelper.Save(filename, imageStream);
            return filename;
        }


        public bool Delete(string imageId)
        {
            return IOHelper.DeleteFile(imageId);
        }

        public int DeleteAllTemporary()
        {
            Logger.Info("Deleting all existing files within {0}", ImageTempDirectory);
            return IOHelper.DeleteFiles(ImageTempDirectory);
        }

        public void Save(string imageId, WriteableBitmap image)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Logger.Info("{0} file {1} and saving image ({2},{3}) to it in JPEG format",
                            store.FileExists(imageId) ? "Overwriting existing" : "Creating",
                            imageId, image.PixelWidth, image.PixelHeight);
                using (IsolatedStorageFileStream fileStream = store.OpenFile(imageId, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    image.SaveJpeg(fileStream, image.PixelWidth, image.PixelHeight, 0, 100);
                }
                Logger.Info("Saved image {0} successfully", imageId);
            }
        }
    }
}