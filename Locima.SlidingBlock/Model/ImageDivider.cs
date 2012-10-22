using System.Windows.Media;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.Common;
using NLog;

namespace Locima.SlidingBlock.Model
{
    public class ImageDivider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool _dirty;

        private WriteableBitmap _imageBitmap;
        private Brush[][] _tileBrushes;
        private int _tilesAcross;

        private int _tilesHigh;

        public int TilesAcross
        {
            get { return _tilesAcross; }
            set
            {
                _tilesAcross = value;
                _dirty = true;
            }
        }

        public int TilesHigh
        {
            get { return _tilesHigh; }
            set
            {
                _tilesHigh = value;
                _dirty = true;
            }
        }


        public WriteableBitmap ImageBitmap
        {
            get { return _imageBitmap; }
            set
            {
                _imageBitmap = value;
                _dirty = true;
            }
        }


        private void Recalculate()
        {
            Logger.Info("Recalculating tile images for {0} by {1} pixel tiles using an image {2} by {3} pixels", TilesAcross, TilesHigh, ImageBitmap.PixelWidth, ImageBitmap.PixelHeight);
            if (ImageBitmap == null)
            {
                Logger.Warn("Unable to recalculate as bitmap not loaded");
                return;
            }
            // Wipe _tileBrushes and reset so there's on array cell per tile
            _tileBrushes = new Brush[TilesHigh][];
            int tileWidth = ImageBitmap.PixelWidth/TilesAcross;
            int tileHeight = ImageBitmap.PixelHeight/TilesHigh;
            Logger.Debug("Each tile image divide the full image {0} by {1} in to {0} by {1} size tiles", ImageBitmap.PixelWidth, ImageBitmap.PixelHeight, tileWidth, tileHeight);
            for (int v = 0; v < TilesHigh; v++)
            {
                _tileBrushes[v] = new Brush[TilesAcross];
                for (int a = 0; a < TilesAcross; a++)
                {
                    // Create a new bitmap for this tile and copy the image data in to it
                    WriteableBitmap tileBitmap = new WriteableBitmap(tileWidth, tileHeight);
                    for (int x = 0; x < tileWidth; x++)
                    {
                        for (int y = 0; y < tileHeight; y++)
                        {
                            int destination = (y*tileWidth + x);
                            int source = ((y + (tileHeight*v))*ImageBitmap.PixelWidth) +
                                         ((x + (tileWidth*a)));
                            tileBitmap.Pixels[destination] = ImageBitmap.Pixels[source];
                        }
                    }

                    ImageBrush tb = new ImageBrush {ImageSource = tileBitmap};
                    _tileBrushes[v][a] = tb;
                }
            }
            _dirty = false;
            Logger.Info("Completed generating {0} tile brushes at {1} x {2} pixels", _tileBrushes.Length, tileWidth, tileHeight);
        }


        public Brush GetTileBrush(int x, int y)
        {
            if (_dirty)
            {
                Logger.Debug("Image Divider is dirty (parameters changed), so recalculating tile images");
                Recalculate();
            }
            if (TilesAcross == 0)
                throw new InvalidStateException("TilesAcross is zero, therefore cannot provide tile brush");
            if (TilesHigh == 0)
                throw new InvalidStateException("TilesHigh is zero, therefore cannot provide tile brush");
            return _tileBrushes[y][x];
        }

        
    }
}