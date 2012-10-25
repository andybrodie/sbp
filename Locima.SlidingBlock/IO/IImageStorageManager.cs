using System;
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
    }
}