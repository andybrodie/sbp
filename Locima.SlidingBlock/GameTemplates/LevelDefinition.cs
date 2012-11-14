using System;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Scrambles;
using NLog;

namespace Locima.SlidingBlock.GameTemplates
{
    /// <summary>
    /// Defines a single level of a <see cref="GameTemplate"/></summary>
    [DataContract]
    public class LevelDefinition
    {
        /// <summary>
        /// The width of the image as it should be stored for a level (this is the maximum size that the display will ever need to show the image at.
        /// </summary>
        public static readonly int ImageSizeX = 480;

        /// <summary>
        /// The height of the image as it should be stored for a level (this is the maximum size that the display will ever need to show the image at.
        /// </summary>
        public static readonly int ImageSizeY = 480;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private WriteableBitmap _image;

        /// <summary>
        /// If the image for the level is managed by the <see cref="ImageStorageManager"/> (e.g. if a download image has been cropped), then this is set
        /// </summary>
        [DataMember]
        public string ImageId { get; set; }

        /// <summary>
        /// If the image for the level is contained as content of the XAP file (i.e. it's a built-in level), then this is set
        /// </summary>
        [DataMember]
        public Uri XapImageUri { get; set; }

        /// <summary>
        /// The license covering the image
        /// </summary>
        [DataMember]
        public LicenseDefinition License { get; set; }

        /// <summary>
        /// A link associated with image (.e.g to the license holders home page)
        /// </summary>
        [DataMember]
        public Uri OwnerUri { get; set; }

        /// <summary>
        /// The name of the license holder for the image
        /// </summary>
        [DataMember]
        public string OwnerName { get; set; }

        /// <summary>
        /// A description that goes with the image (i.e. what's the image a picture of, e.g. "My pet dog")
        /// </summary>
        [DataMember]
        public string ImageText { get; set; }

        /// <summary>
        /// A title for the image
        /// </summary>
        [DataMember]
        public string ImageTitle { get; set; }

        /// <summary>
        /// THe type of scramble to be applied to the image, i.e. how will the tiles be jumbled up at the beginning of the level.  Different scrambles can make
        /// the level easier or more difficult.
        /// </summary>
        [DataMember]
        public Scrambler.ScrambleType ScrambleType { get; set; }

        /// <summary>
        /// Retrieves the image for the level definition
        /// </summary>
        /// <returns>The imgae for the level</returns>
        public WriteableBitmap GetImage()
        {
            if (_image == null)
            {
                _image = !string.IsNullOrEmpty(ImageId)
                             ? ImageStorageManager.Instance.Load(ImageId)
                             : ImageStorageManager.Instance.Load(XapImageUri);
            }
            return _image;
        }


        /// <summary>
        /// Create a resized thumbnail of the image
        /// </summary>
        /// <param name="width">The width of the thumbnail</param>
        /// <param name="height">The height of the thumbnail</param>
        /// <returns>A thumbnail image</returns>
        public WriteableBitmap CreateThumbnail(int width, int height)
        {
            WriteableBitmap bitmap = GetImage();
            Logger.Info("Creating a {0}x{1} thumbnail of the {2}x{3} iamge", width, height, _image.PixelWidth,
                        _image.PixelHeight);
            WriteableBitmap thumbnail = bitmap.Clone();
            thumbnail.Resize(width, height, WriteableBitmapExtensions.Interpolation.Bilinear);
            return thumbnail;
        }
    }
}