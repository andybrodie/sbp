using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;
using NLog;

namespace Locima.SlidingBlock.Common
{
    /// <summary>
    /// Handler functions for dealing with the live tiles that make up this application
    /// </summary>
    public class TileUpdater
    {
        /// <summary>
        /// The width of a tile in Windows Phone 7.1
        /// </summary>
        private const int TileWidth = 173;

        /// <summary>
        /// The height  of a tile in Windows Phone 7.1
        /// </summary>
        private const int TileHeight = 173;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static TileUpdater()
        {
            Instance = new TileUpdater();
        }

        /// <summary>
        /// Prevents more instances being created
        /// </summary>
        private TileUpdater()
        {
        }

        /// <summary>
        /// Singleton instance, use this for access to the methods of this class
        /// </summary>
        public static TileUpdater Instance { get; private set; }


        /// <summary>
        /// Convenience accessor for the main application tile
        /// </summary>
        /// <remarks>
        /// This is never null</remarks>
        private ShellTile ApplicationTile
        {
            get { return ShellTile.ActiveTiles.First(); }
        }

        /// <summary>
        /// Updates the main application tile, replacing the <see cref="StandardTileData.BackBackgroundImage"/> with the <paramref name="image"/> provided (typically
        /// a thumbnail of the puzzle in its current state)
        /// </summary>
        /// <param name="image">The image to use as the <see cref="StandardTileData.BackBackgroundImage"/>.  Image is uautomatically resized.</param>
        public void UpdateApplicationTile(WriteableBitmap image)
        {
            if (ApplicationTile != null)
            {
                try
                {
                    const string filename = "/Shared/ShellContent/MainTileBackBackground.jpg";
                    Uri tileImageUri = SaveTileImage(image, filename);
                    StandardTileData updatedTileData = new StandardTileData
                        {
                            Title = null,
                            BackgroundImage = null,
                            Count = null,
                            BackTitle = String.Empty,
                            BackContent = String.Empty,
                            BackBackgroundImage = tileImageUri
                        };
                    ApplicationTile.Update(updatedTileData);
                }
                catch (Exception e)
                {
                    // Tile background setting is a nice to have, any errors should be logged but don't let this failure kill the app!
                    Logger.ErrorException("Error updating application tile", e);
                }
            }
        }


        /// <summary>
        /// Saves the image passed as the default tile image (i.e. an image of <see cref="TileWidth"/> x <see cref="TileHeight"/> pixels.
        /// </summary>
        /// <param name="image">The image to save as the tile image</param>
        /// <param name="filename">The absolute filename on the file system that the image should be saved to</param>
        /// <returns>The absolute <see cref="Uri"/> of the image once it has been saved</returns>
        private Uri SaveTileImage(WriteableBitmap image, string filename)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream s = store.CreateFile(filename))
                {
                    Logger.Info("Saving JPEG to {0}", filename);
                    image.SaveJpeg(s, TileWidth, TileHeight, 0, 75);
                }
            }
            Uri imageUri = new Uri("isostore:" + filename, UriKind.Absolute);
            Logger.Info("Saved tile image, returning Uri: {0}", imageUri);
            return imageUri;
        }
    }
}