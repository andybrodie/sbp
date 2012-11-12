using System;
using System.IO;
using System.Windows.Media.Imaging;

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
        WriteableBitmap LoadImage(string imageId);

        /// <summary>
        /// Loads an image from either a local or absolute <see cref="Uri"/>
        /// </summary>
        WriteableBitmap LoadImage(Uri uri);

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
    }
}