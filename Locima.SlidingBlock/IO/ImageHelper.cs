using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using NLog;

namespace Locima.SlidingBlock.IO
{

    /// <summary>
    /// Helper methods for managing image data
    /// </summary>
    public class ImageHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The quality factor to apply to JPEGs when exporting them. 
        /// </summary>
        /// <remarks>
        /// Used for the <c>quality</c> parameter of <see cref="Extensions.SaveJpeg"/></remarks>
        public const int JpegQuality = 70;


        /// <summary>
        /// Converts a JPEG stored in a byte array in to a <see cref="WriteableBitmap"/> object
        /// </summary>
        /// <param name="jpegData">The raw JPEG data</param>
        /// <param name="width">The width of the image to deserialise to</param>
        /// <param name="height">The height of the image to deserialise to</param>
        /// <returns>Either a bitmap, or null if <paramref name="jpegData"/> was null or of length 0</returns>
        public static WriteableBitmap FromJpeg(byte[] jpegData, int width, int height)
        {
            WriteableBitmap bitmap;
            if (jpegData == null || jpegData.Length == 0)
            {
                bitmap = null;
            }
            else
            {
                bitmap = new WriteableBitmap(width, height);
                {
                    Logger.Info("Deserialising JPEG of {0} bytes in ThumbnailData in to ThumbnailData",
                                jpegData.Length);
                    using (MemoryStream ms = new MemoryStream(jpegData))
                    {
                        bitmap.LoadJpeg(ms);
                    }
                }
            }
            return bitmap;
        }


        /// <summary>
        /// Serialise <paramref name="bitmap"/> as a JPEG-encoded byte array of width <paramref name="width"/> and height <paramref name="height"/>
        /// </summary>
        /// <param name="bitmap">The bitmap to serialise</param>
        /// <param name="width">The width of the JPEG to create</param>
        /// <param name="height">The height of the JPEG to create</param>
        /// <returns>A byte array that, when passed to <see cref="FromJpeg"/> will allow the round-tripping of <see cref="WriteableBitmap"/> round-tripping as JPEGs. Or null is <paramref name="bitmap"/>
        /// is null</returns>
        public static byte[] ToJpeg(WriteableBitmap bitmap, int width, int height)
        {
            byte[] jpegData;
            if (bitmap == null)
            {
                jpegData = null;
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.SaveJpeg(ms, bitmap.PixelWidth, bitmap.PixelHeight, 0, JpegQuality);
                    jpegData = ms.ToArray();
                    Logger.Info("Saved {0} by {1} JPEG of {2} bytes", width, height, jpegData.Length);
                }
            }
            return jpegData;
        }


        /// <summary>
        /// Loads a bitmap from the XAP resources (built-in images that come with the app)
        /// </summary>
        /// <param name="xapImageUri">The URI for the iamge</param>
        /// <returns>A bitmap</returns>
        public static WriteableBitmap LoadBitmapFromXapContent(Uri xapImageUri)
        {
            WriteableBitmap wbitmap;
            if (xapImageUri == null)
            {
                Logger.Info("No image specified in xapImageUri, returning empty bitmap");
                wbitmap = new WriteableBitmap(1, 1);
            }
            else
            {
                BitmapImage bitmap = new BitmapImage();
                Logger.Info("Loading image from XAP content {0}", xapImageUri);
                // We have to use SetSource so that it loads sychronously, otherwise the creation of the WriteableBitmap may throw a NullReferenceException
                bitmap.CreateOptions = BitmapCreateOptions.None;

                StreamResourceInfo resourceInfo = Application.GetResourceStream(xapImageUri);
                bitmap.SetSource(resourceInfo.Stream);
                wbitmap = new WriteableBitmap(bitmap);
                Logger.Debug("Loaded image from XAP content {0} successfully", xapImageUri);
            }
            return wbitmap;
        }
    }
}