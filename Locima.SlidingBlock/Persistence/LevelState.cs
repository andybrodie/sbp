using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Microsoft.Phone.Tasks;
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
        public static readonly int ThumbnailSize = 64;

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
        /// <remarks>
        /// Unlike the <see cref="Image"/> this is persisted within this object as it's pretty small and won't adversely affect performed</remarks>
        [DataMember]
        public byte[] ThumbnailData { get; set; }

        /// <summary>
        /// The amount of time the player has been playing this level for
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
                    _thumbnail = ImageHelper.FromJpeg(ThumbnailData, ThumbnailSize, ThumbnailSize);
                }
                return _thumbnail;
            }
            set
            {
                _thumbnail = value;
                ThumbnailData = ImageHelper.ToJpeg(_thumbnail, ThumbnailSize, ThumbnailSize);
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
        /// <remarks>
        /// This is lazily instantiated as the image file is not actually held within this object.</remarks>
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
                            if (string.IsNullOrEmpty(ImageId))
                            {
                                _image = !string.IsNullOrEmpty(ImageId)
                                             ? ImageStorageManager.Instance.LoadImage(ImageId)
                                             : ImageStorageManager.Instance.LoadImage(XapImageUri);
                            }
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
        /// If the image is stored by the <see cref="ImageStorageManager"/>, then this is set to the Id of the image
        /// </summary>
        /// <remarks>
        /// If this is set, then <see cref="XapImageUri"/> should be null and vice versa.
        /// </remarks>
        [DataMember]
        public string ImageId { get; set; }

        /// <summary>
        /// If the image is stored in the Xap as content, then this is set
        /// </summary>
        /// <remarks>
        /// If this is set, then <see cref="ImageId"/> should be null and vice versa.
        /// </remarks>
        [DataMember]
        public Uri XapImageUri { get; set; }


        /// <summary>
        ///   Re-initialises any non-serialised objects which require initialisation (typically via a constructor which isn't called during deserialisation)
        /// </summary>
        [OnDeserializing]
        public void OnDeserialise(StreamingContext ctx)
        {
            _lockObject = new object();
        }


        /// <summary>
        /// Initialises the image for this level using the image stream <paramref name="stream"/>
        /// </summary>
        /// <remarks>
        /// <para>This method saves image to the <see cref="ImageStorageManager"/> and records the automatically generated ID</para>
        /// <para>This is useful when using the <see cref="PhotoChooserTask"/>, the chosen photo comes back as a stream</para></remarks>
        /// <param name="stream">A stream that an image can be read from</param>
        public void SetImage(Stream stream)
        {
            ImageId = ImageStorageManager.Instance.Save(stream);
        }

        /// <summary>
        /// Sets the image for this level using a bitmap <paramref name="bitmap"/>
        /// </summary>
        /// <remarks>
        /// <para>This method saves image to the <see cref="ImageStorageManager"/> and records the automatically generated ID</para>
        /// <para>This is useful when using the <see cref="PhotoChooserTask"/>, the chosen photo comes back as a stream</para></remarks>
        /// <param name="bitmap">A bitmap</param>
        public void SetImage(WriteableBitmap bitmap)
        {
            ImageId = ImageStorageManager.Instance.Save(bitmap);
            _image = bitmap;
        }
    }
}