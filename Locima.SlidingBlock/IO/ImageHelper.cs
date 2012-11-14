using System.IO;
using System.Windows.Media.Imaging;
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
                    Logger.Info("Saved thumbnail for {0} using JPEG of {1} bytes", jpegData.Length);
                }
            }
            return jpegData;
        }

    }
}