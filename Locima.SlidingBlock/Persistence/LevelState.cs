using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Microsoft.Phone;
using NLog;

namespace Locima.SlidingBlock.Persistence
{
    /// <summary>
    ///   Represents the "in-flight" state of a level in progress
    /// </summary>
    /// <remarks>
    ///   The definition of a level can be found in <see cref="LevelDefinition" />
    /// </remarks>
    [DataContract]
    public class LevelState
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///   The size of the <see cref="ThumbnailData" /> image
        /// </summary>
        public static readonly int ThumbnailSize = 32;

        private volatile WriteableBitmap _image;

        /// <summary>
        ///   A lock used to ensure that multiple threads don't cause the hard work done in initialising <see cref="Image" /> isn't redone.
        /// </summary>
        /// <remarks>
        ///   Remember, on deserialisation this will not be re-initialised (constructors are not called), therefore this object is reset in <see
        ///    cref="OnDeserialise" />
        /// </remarks>
        private object _lockObject = new object();

        /// <summary>
        /// Backing field for <see cref="Thumbnail"/>
        /// </summary>
        private WriteableBitmap _thumbnail;

        /// <summary>
        /// The data for the thumbnail image
        /// </summary>
        [DataMember]
        public byte[] ThumbnailData { get; set; }

        /// <summary>
        /// The name of the image file when it's held in isolated storage
        /// </summary>
        [DataMember]
        public string IsolatedStorageFilename { get; set; }

        /// <summary>
        /// The Uri of the image file when it's held in the XAP distribution
        /// </summary>
        [DataMember]
        public Uri XapImageUri { get; set; }

        /// <summary>
        /// The amount of time the player 
        /// </summary>
        [DataMember]
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// The state of the puzzle tiles
        /// </summary>
        [DataMember]
        public Position[][] SolvedTilePositions { get; set; }


        /// <summary>
        /// Gets the bitmap thumbnail for the level
        /// </summary>
        public WriteableBitmap Thumbnail
        {
            get
            {
                if (_thumbnail == null)
                {
                    _thumbnail = SerialisedJpegToWriteableBitmap(ThumbnailData);
                }
                return _thumbnail;
            }
            set
            {
                _thumbnail = value;
                ThumbnailData = WriteableBitmapToSerialisedJpeg(_thumbnail);
            }
        }

        /// <summary>
        /// The number of tiles across this level has
        /// </summary>
        public int TilesAcross
        {
            get
            {
                return (SolvedTilePositions != null && SolvedTilePositions.Length > 0 && SolvedTilePositions[0] != null)
                           ? SolvedTilePositions[0].Length
                           : 0;
            }
        }

        /// <summary>
        /// The number of tiles high this level has
        /// </summary>
        public int TilesHigh
        {
            get { return SolvedTilePositions != null ? SolvedTilePositions.Length : 0; }
        }


        /// <summary>
        /// The image that the player is trying to arrange
        /// </summary>
        public WriteableBitmap Image
        {
            get
            {
                if (_image == null)
                {
                    lock (_lockObject)
                    {
                        if (_image == null)
                        {
                            WriteableBitmap loadedImage = LoadImage();
                            _image = loadedImage;
                        }
                    }
                }
                return _image;
            }
        }


        /// <summary>
        /// The count of moves so far the player has made on this level
        /// </summary>
        [DataMember]
        public int MoveCount { get; set; }

        /// <summary>
        ///   Re-initialises any non-serialised objects which require initialisation (typically via a constructor which isn't called during deserialisation)
        /// </summary>
        [OnDeserializing]
        public void OnDeserialise(StreamingContext ctx)
        {
            _lockObject = new object();
        }

        /// <summary>
        /// Loads the bitmap for the image in to this object
        /// </summary>
        /// <returns></returns>
        public WriteableBitmap LoadImage()
        {
            WriteableBitmap bitmap;
            if (!string.IsNullOrEmpty(IsolatedStorageFilename))
            {
                bitmap = ImageStorageManager.Instance.LoadImage(IsolatedStorageFilename);
            }
            else
            {
                bitmap = ImageStorageManager.Instance.LoadImage(XapImageUri);
            }
            return bitmap;
        }

        
        /// <summary>
        /// Writes the image passed to a 480 x 480 image as a JPEG to.... NOWHERE!
        /// </summary>
        /// <param name="image">The image to save</param>
        public void SetAndSaveImage(WriteableBitmap image)
        {
            // Save the selected image area back to to the puzzle metadata
            MemoryStream ms = new MemoryStream();
            image.SaveJpeg(ms, 480, 480, 0, 70);
            ms.Close();
            throw new NotImplementedException();
        }

 
        /// <summary>
        /// NOT IMPLEMENTED YET (<see cref="ImageChooser"/>)
        /// </summary>
        /// <param name="imageDataStream"></param>
        public void SetAndSaveImage(Stream imageDataStream)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///   Convert image from a JPEG stored as a byte array back in to a <see cref="WriteableBitmap" />
        /// </summary>
        /// <param name="imageData"> </param>
        /// <returns> </returns>
        private static WriteableBitmap SerialisedJpegToWriteableBitmap(byte[] imageData)
        {
            Logger.Info("Converting input array to WriteableBitmap");
            if (imageData == null) return null;
            using (MemoryStream imageDataStream = new MemoryStream())
            {
                imageDataStream.Write(imageData, 0, imageData.Length);
                imageDataStream.Seek(0, SeekOrigin.Begin);
                Logger.Info("Converting input array to WriteableBitmap compelted");
                return PictureDecoder.DecodeJpeg(imageDataStream);
            }
        }


        private static byte[] WriteableBitmapToSerialisedJpeg(WriteableBitmap image)
        {
            Logger.Info("Converting image to JPEG byte array");
            MemoryStream jpegStream = new MemoryStream();
            image.SaveJpeg(jpegStream, image.PixelWidth, image.PixelHeight, 0, 70);
            jpegStream.Close();
            byte[] result = jpegStream.ToArray();
            Logger.Info("Created {0} bytes from input image", result.Length);
            return result;
        }
    }
}