using System;
using System.Collections.Generic;
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
                    IsolatedStorageFileStream fileStream = store.OpenFile(imageId, FileMode.Open, FileAccess.Read))
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


        /// <summary>
        /// Saves a new image, creating an ID
        /// </summary>
        /// <param name="imageStream">The stream of bytes that makes up the image</param>
        /// <returns>The ID of the newly saved image</returns>
        public string Save(Stream imageStream)
        {
            string filename = Path.Combine(ImageDirectory, Guid.NewGuid().ToString());
            IOHelper.Save(filename, imageStream);
            return filename;
        }

        
        /// <summary>
        /// Saves a new image, creating an ID
        /// </summary>
        /// <param name="image">The image to save</param>
        /// <returns>The ID of the newly saved image</returns>
        public string Save(WriteableBitmap image)
        {
            string filename = Path.Combine(ImageDirectory, Guid.NewGuid().ToString());
            Save(filename, image);
            return filename;
        }


        /// <summary>
        /// Saves the bitmap passed in <paramref name="image"/> uusing the ID specified by <paramref name="imageId"/>
        /// </summary>
        /// <param name="imageId">The ID of the image to replace, or create if it doesn't exist</param>
        /// <param name="image">The image to save</param>
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


        /// <summary>
        /// Saves a "temporary" image, created as part of level creation
        /// </summary>
        /// <param name="imageStream">The stream of bytes that makes up the image</param>
        /// <returns>The ID of the new image</returns>
        public string SaveTemporary(Stream imageStream)
        {
            string filename = Path.Combine(ImageTempDirectory, Guid.NewGuid().ToString());
            Logger.Info("Saving temporary image {0}", filename);
            IOHelper.Save(filename, imageStream);
            return filename;
        }


        /// <summary>
        /// Deletes an image or temporary image
        /// </summary>
        /// <param name="imageId">The ID of the image</param>
        /// <returns><c>true</c> if the image was deleted, <c>false</c>if it didn't exist</returns>
        public bool Delete(string imageId)
        {
            return IOHelper.DeleteFile(imageId);
        }


        /// <summary>
        /// Delete all temporary images
        /// </summary>
        /// <returns></returns>
        public int DeleteAllTemporary()
        {
            Logger.Info("Deleting all existing files within {0}", ImageTempDirectory);
            return IOHelper.DeleteFiles(ImageTempDirectory);
        }


        /// <summary>
        /// Determines whether an image is temporary or not by examining the ID
        /// </summary>
        /// <param name="imageId">The of the image</param>
        /// <returns></returns>
        public bool IsTemporary(string imageId)
        {
            if (string.IsNullOrEmpty(imageId)) throw new ArgumentNullException("imageId");
            return imageId.StartsWith(ImageTempDirectory);
        }



        /// <summary>
        /// Promote the image moving the file from <see cref="ImageTempDirectory"/> to <see cref="ImageDirectory"/>
        /// </summary>
        /// <param name="temporaryImageId">The temporary image ID</param>
        /// <returns>The new ID for the image as a permanent image</returns>
        public string Promote(string temporaryImageId)
        {
            if (!IsTemporary(temporaryImageId)) throw new InvalidStateException(string.Format("Attempt to promote non-temporary image {0}", temporaryImageId));
            string promotedImageId = Path.Combine(ImageDirectory, temporaryImageId.Substring(ImageTempDirectory.Length + 1));
            Logger.Info("Promoting temporary image {0} to {1}", temporaryImageId, promotedImageId);
            IOHelper.MoveFile(temporaryImageId, promotedImageId);
            return promotedImageId;
        }


        /// <inheritDoc/>
        public List<string> ListImages(bool includeTemporaryImages)
        {
            List<string> imageIds = IOHelper.GetFileNames(ImageDirectory);
            if (includeTemporaryImages)
            {
                imageIds.AddRange(IOHelper.GetFileNames(ImageTempDirectory));
            }
            return imageIds;
        }
    }
}