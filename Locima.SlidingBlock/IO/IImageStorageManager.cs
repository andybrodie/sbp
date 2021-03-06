using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;

namespace Locima.SlidingBlock.IO
{

    /// <summary>
    /// Currently unused!
    /// </summary>
    public interface IImageStorageManager : IStorageManager
    {
        /// <summary>
        /// Loads an image based on a previously assigned ID, return from an earlier call to BUG
        /// </summary>
        /// <param name="imageId">The identity of the image to load</param>
        WriteableBitmap Load(string imageId);

        /// <summary>
        /// Loads an image from either a local or absolute <see cref="Uri"/>
        /// </summary>
        WriteableBitmap Load(Uri uri);

        /// <summary>
        /// Saves the stream as an image and returns the ID
        /// </summary>
        /// <param name="imageStream">The image stream to save</param>
        /// <returns>The Id of the image</returns>
        string Save(Stream imageStream);

        /// <summary>
        /// Saves the photo 
        /// </summary>
        /// <param name="image">The image to save</param>
        /// <returns>The Id of the saved image</returns>
        string Save(WriteableBitmap image);

        /// <summary>
        /// Saves the photo to a temporary file
        /// </summary>
        /// <remarks>
        /// Temporary files are cleared out when the store is initialised on application load</remarks>
        /// <param name="imageStream">A stream containing the image, typically from <see cref="PhotoChooserTask"/> or a web client</param>
        /// <returns>The ID of the image, to be used with <see cref="Load(string)"/></returns>
        string SaveTemporary(Stream imageStream);

        /// <summary>
        /// Deletes an image given its ID
        /// </summary>
        /// <param name="imageId"></param>
        /// <returns></returns>
        bool Delete(string imageId);


        /// <summary>
        /// Delete all temporary images
        /// </summary>
        /// <returns>The number of images deleted</returns>
        int DeleteAllTemporary();

        /// <summary>
        /// Saves an image with a specific ID
        /// </summary>
        /// <remarks>
        /// This is used for updating an existing image held in the catalogue.  Do NOT use this method for creating new images</remarks>
        /// <param name="imageId">The ID of the image to update</param>
        /// <param name="image">The new data to replace the existing image with</param>
        void Save(string imageId, WriteableBitmap image);

        /// <summary>
        /// Determines whether an image is temporary or not
        /// </summary>
        /// <param name="imageId">The image ID to check</param>
        /// <returns><c>true</c> if the image is a temporary image, <c>false</c> otherwise</returns>
        bool IsTemporary(string imageId);

        /// <summary>
        /// Promotes a temporary image to a permanent one (which changes the ID)
        /// </summary>
        /// <param name="imageId">The ID of the image to update</param>
        /// <returns>Temporary and permanent images have different IDs.  This is the new ID for the image.</returns>
        string Promote(string imageId);

        /// <summary>
        /// Obtains a list of all the image IDs known by the system.
        /// </summary>
        /// <remarks>
        /// Does not include images located within the XAP for this app</remarks>
        /// <param name="includeTemporaryImages">If <c>true</c> include temporary images, false otherwise</param>
        /// <returns>A list (potentially empty) of all images stored by this storage manager.</returns>
        List<string> ListImages(bool includeTemporaryImages);
    }
}