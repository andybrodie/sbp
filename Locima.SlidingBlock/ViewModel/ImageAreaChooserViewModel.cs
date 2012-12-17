using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Messaging;
using Microsoft.Phone.Controls;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// The MVVM view model for the <see cref="ImageAreaChooser"/> page.
    /// </summary>
    public class ImageAreaChooserViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <see cref="CropHeight"/>
        /// </summary>
        private double _cropHeight;
        /// <summary>
        /// Backing field for <see cref="CropLeft"/>
        /// </summary>
        private double _cropLeft;
        /// <summary>
        /// Backing field for <see cref="CropTop"/>
        /// </summary>
        private double _cropTop;
        /// <summary>
        /// Backing field for <see cref="CropWidth"/>
        /// </summary>
        private double _cropWidth;
        /// <summary>
        /// Backing field for <see cref="Image"/>
        /// </summary>
        private WriteableBitmap _image;
        /// <summary>
        /// Backing field for <see cref="MinScaleFactor"/>
        /// </summary>
        private double _minScaleFactor;
        /// <summary>
        /// Backing field for <see cref="Scale"/>
        /// </summary>
        private double _scale;
        /// <summary>
        /// Backing field for <see cref="TotalHeight"/>
        /// </summary>
        private double _totalHeight;
        /// <summary>
        /// Backing field for <see cref="TotalWidth"/>
        /// </summary>
        private double _totalWidth;

        /// <summary>
        /// The distance from the left hand side of the screen (pixel offset 0) to the left edge of the crop area, used fro the dark rectangle on the left of the crop area
        /// </summary>
        public double CropLeft
        {
            get { return _cropLeft; }
            set
            {
                _cropLeft = value;
                OnNotifyPropertyChanged("CropLeft");
            }
        }

        /// <summary>
        /// The distance from the top of the screen to the the top of the crop area, used for the dark rectangle to the top of the crop area
        /// </summary>
        public double CropTop
        {
            get { return _cropTop; }
            set
            {
                _cropTop = value;
                OnNotifyPropertyChanged("CropTop");
            }
        }


        /// <summary>
        /// The width of the crop area that image that will be cut out from the whole image
        /// </summary>
        public double CropWidth
        {
            get { return _cropWidth; }
            set
            {
                _cropWidth = value;
                OnNotifyPropertyChanged("CropWidth");
            }
        }


        /// <summary>
        /// The height of the image that will be cut out from the whole image
        /// </summary>
        public double CropHeight
        {
            get { return _cropHeight; }
            set
            {
                _cropHeight = value;
                OnNotifyPropertyChanged("CropHeight");
            }
        }


        /// <summary>
        /// The total width of the available canvas containing the image
        /// </summary>
        public double TotalWidth
        {
            get { return _totalWidth; }
            set
            {
                _totalWidth = value;
                OnNotifyPropertyChanged("TotalWidth");
            }
        }

        /// <summary>
        /// The total height of the available canvas containing the image
        /// </summary>
        public double TotalHeight
        {
            get { return _totalHeight; }
            set
            {
                _totalHeight = value;
                OnNotifyPropertyChanged("TotalHeight");
            }
        }

        /// <summary>
        /// The UI element that contains the image
        /// </summary>
        /// <remarks>
        /// We need this for calls to <see cref="GestureEventArgs.GetPosition(UIElement)"/> calls, which are reuqired to be relative to the image</remarks>
        public UIElement ImageControl { get; set; }

        /// <summary>
        /// The bottom of the image
        /// </summary>
        public double CropBottom
        {
            get { return Math.Abs(CropTop + CropHeight); }
        }

        /// <summary>
        /// The right hand edge of the image
        /// </summary>
        public double CropRight
        {
            get { return Math.Abs(CropLeft + CropWidth); }
        }


        /// <summary>
        /// The difference between the <see cref="TotalWidth"/> and the right and edge of the image (<see cref="CropRight"/>), used for the dark rectangle to the right of the crop area
        /// </summary>
        public double CropRightToRightEdge
        {
            get { return Math.Abs(TotalWidth - CropRight); }
        }

        /// <summary>
        /// The difference between the <see cref="TotalHeight"/> and the right and edge of the image (<see cref="CropBottom"/>), used for the dark rectangle underneath the crop area
        /// </summary>
        public double CropBottomToBottomEdge
        {
            get { return Math.Abs(TotalHeight - CropBottom); }
        }

        /// <summary>
        /// The whole image that we're cropping
        /// </summary>
        public WriteableBitmap Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnNotifyPropertyChanged("Image");
                ResetScaleAndTranslateBoundaries();
            }
        }


        /// <summary>
        /// Invoked on the setter for <see cref="Image"/>, this resets the <see cref="MinScaleFactor"/> property based on the 
        /// dimensions of the image
        /// </summary>
        private void ResetScaleAndTranslateBoundaries()
        {
            // Calculate minimum scale factor and don't allow scale factor to mean picture can't fill square
            double minScaleFactorX = CropWidth / Image.PixelWidth;
            double minScaleFactorY = CropHeight / Image.PixelHeight;
            MinScaleFactor = Math.Max(minScaleFactorX, minScaleFactorY);
            Scale = MinScaleFactor;
            ImagePosition = new Point(CropLeft, CropTop);
            
        }

        /// <summary>
        /// The identity of the image to edit
        /// </summary>
        /// <remarks>Set by the view, image then loaded in to <see cref="Image"/></remarks>
        /// <remarks>For use with <see cref="IImageStorageManager"/></remarks>
        public string ImageId { get; set; }


        /// <summary>
        /// The smallest scale factor that is permitted that ensures that the image fills the selection area
        /// </summary>
        public double MinScaleFactor
        {
            get { return _minScaleFactor; }
            set
            {
                _minScaleFactor = value;
                OnNotifyPropertyChanged("MinScaleFactor");
                if (_scale < _minScaleFactor)
                {
                    Scale = _minScaleFactor;
                }
            }
        }

        /// <summary>
        /// The current scale (zoom factor) applied to the image
        /// </summary>
        public double Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                OnNotifyPropertyChanged("Scale");
            }
        }

        /// <summary>
        /// Handy debug string which updates each time something observable on the VM changes
        /// </summary>
        public string DebugString { get; set; }

        /// <summary>
        /// The ID of the game template that we're editing
        /// </summary>
        public string GameTemplateId { private get; set; }

        /// <summary>
        /// The index of the level within the <see cref="GameTemplateId"/> that we're editing
        /// </summary>
        public int LevelIndex { private get; set; }

        /// <summary>
        /// Hooks our own <see cref="ViewModelBase.PropertyChanged"/> events with <see cref="OnPropertyChanged"/>
        /// </summary>
        public void Initialise()
        {
            PropertyChanged -= OnPropertyChanged;
            PropertyChanged += OnPropertyChanged;
            Image = ImageStorageManager.Instance.Load(ImageId);
        }

        /// <summary>
        ///   Raises <see cref="ViewModelBase.PropertyChanged" /> events for properties which are dependent on other properties within this viewmodel
        /// </summary>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName != "DebugString")
            {
                UpdateDebugString();
                OnNotifyPropertyChanged("DebugString");
            }
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "CropTop":
                    OnNotifyPropertyChanged("CropBottom");
                    break;
                case "CropHeight":
                    OnNotifyPropertyChanged("CropBottom");
                    break;
                case "CropLeft":
                    OnNotifyPropertyChanged("CropRight");
                    break;
                case "CropWidth":
                    OnNotifyPropertyChanged("CropRight");
                    break;
                case "TotalWidth":
                    OnNotifyPropertyChanged("CropRightToRightEdge");
                    break;
                case "CropRight":
                    OnNotifyPropertyChanged("CropRightToRightEdge");
                    break;
                case "TotalHeight":
                    OnNotifyPropertyChanged("CropBottomToBottomEdge");
                    break;
                case "CropBottom":
                    OnNotifyPropertyChanged("CropBottomToBottomEdge");
                    break;
            }
        }

        private void UpdateDebugString()
        {
            DebugString =
                string.Format("CL:{0} CR:{1} CT:{2} CB:{3} CBTBE:{4} CRTRE:{5} CW:{6} CH:{7} TX:{8} TY:{9} S:{10}",
                              CropLeft, CropRight, CropTop, CropBottom, CropBottomToBottomEdge, CropRightToRightEdge,
                              CropWidth,
                              CropHeight, ImagePosition.X, ImagePosition.Y, Scale);
        }


        /// <summary>
        /// When the user accepts the pan and zoom settings that are currently on screen, we need to update the image to be only what's inside the square
        /// </summary>
        public void AcceptImage(object sender, EventArgs e)
        {
            WriteableBitmap croppedImage = GetCroppedImage();

            // Save the image
            ImageStorageManager.Instance.Save(ImageId, croppedImage);

            // Update the GameTemplate's Level image to the new image
            GameTemplate template = GameTemplateStorageManager.Instance.Load(GameTemplateId);
            LevelDefinition currentLevel = template.Levels[LevelIndex];
            Logger.Info("Saving level {0} of {1} XapImageUri from {2} to null and ImageId from {3} to {4}", LevelIndex, template, currentLevel.XapImageUri, currentLevel.ImageId ?? "null", ImageId);
            currentLevel.XapImageUri = null;
            currentLevel.ImageId = ImageId;
            GameTemplateStorageManager.Instance.Save(template);

            // Go back to the Level Editor, skipping over the ImageChooser page    
            Logger.Info("Navingating back to the LevelEditor page, suppressing the ImageChooser page on the top of the backstack");
            SendViewMessage(new NavigationMessageArgs { NavigationMode = NavigationMode.Back, DeleteBackstackEntries = 1 });
        }


        /// <summary>
        /// Retrieve the cropped image as a factor of the translation amounts and scale factor
        /// </summary>
        /// <returns>A bitmap, scaled to <see cref="LevelDefinition.ImageSizeX"/> by <see cref="LevelDefinition.ImageSizeY"/> that is in the crop window</returns>
        private WriteableBitmap GetCroppedImage()
        {
            // Work out the amount of image that's within the bounding box, take in to account translation and scale factors
            int offsetX = (int)(Math.Abs((ImagePosition.X-CropLeft)/Scale));
            int offsetY = (int)(Math.Abs((ImagePosition.Y-CropTop)/Scale));
            int imageSizeX = (int) Math.Round(CropWidth/Scale);
            int imageSizeY = (int) Math.Round(CropHeight/Scale);

            Logger.Info("Creating cropped image from {0},{1} of width {2} and height {3} pixels.", offsetX, offsetY,
                        imageSizeX, imageSizeY);

            WriteableBitmap sourceBitmap = new WriteableBitmap(Image);
            WriteableBitmap puzzleBitmap = sourceBitmap.Crop(offsetX, offsetY, imageSizeX, imageSizeY);

            // Scale the image to the right size for the puzzle
            sourceBitmap.Resize(LevelDefinition.ImageSizeX, LevelDefinition.ImageSizeY, WriteableBitmapExtensions.Interpolation.Bilinear);

            // Reduce the size of the image to the largest size it will ever be displayed at
            if (imageSizeX > LevelDefinition.ImageSizeX || imageSizeY > LevelDefinition.ImageSizeY)
            {
                Logger.Debug("Resizing to {0} x {1}", LevelDefinition.ImageSizeX, LevelDefinition.ImageSizeY);
                puzzleBitmap.Resize(LevelDefinition.ImageSizeX, LevelDefinition.ImageSizeY,
                                    WriteableBitmapExtensions.Interpolation.Bilinear);
            }

            return puzzleBitmap;
        }


        #region Image Pan and Zoom

        private Point _imagePosition = new Point(0, 0);

        private Point _oldFinger1;
        private Point _oldFinger2;
        private double _oldScaleFactor;

        /*
         * This region contains all the calculations to manage the user panning and zooming the image to get the part of the image they want to use for the puzzle
         */

        /// <summary>
        /// The X and Y offset of the image relative to the Canvas.  This changes the image is panned across the screen (and zoomed, indrectly)
        /// </summary>
        public Point ImagePosition
        {
            get { return _imagePosition; }
            set
            {
                _imagePosition = value;
                OnNotifyPropertyChanged("ImagePosition");
            }
        }

        /// <summary>
        /// Invoked when a pinch zoom operation is started
        /// </summary>
        /// <remarks>Simply records the position of the two fingers controlling the zoom and set our initial scale factor delta to 1 (i.e. no change)</remarks>
        /// <param name="currentFinger1">The position of the first zoom point</param>
        /// <param name="currentFinger2">The position of the second zoom point</param>
        public void OnPinchStarted(Point currentFinger1, Point currentFinger2)
        {
            _oldFinger1 = currentFinger1;
            _oldFinger2 = currentFinger2; 
            _oldScaleFactor = 1;
        }


        /// <summary>
        /// Invoked when the distance between two fingers on the image changes, causing the scale factor to be updated 
        /// </summary>
        /// <param name="currentFinger1">The new position of the first zoom point</param>
        /// <param name="currentFinger2">The new position of the second zoom point</param>
        /// <param name="distanceRatio">TODO FIND OUT WHAT THIS ACTUALLY IS</param>
        public void OnPinchDelta(Point currentFinger1, Point currentFinger2, double distanceRatio)
        {
            double scaleFactor = distanceRatio/_oldScaleFactor;

            // Calculate the amount of translation required to ensure that the same parts of the image stay under your fingers as you pinch
            Point translationDelta = GetTranslationDelta(currentFinger1, currentFinger2, _oldFinger1, _oldFinger2,
                                                         ImagePosition, scaleFactor);

            Logger.Debug("PinchDelta DR:{0} SF:{1} OSF:{2} OF1:{3} OF2:{4} CF1:{5} CF2:{6} TD:{8}", distanceRatio,
                         scaleFactor, _oldScaleFactor, _oldFinger1, _oldFinger2, currentFinger1, currentFinger2,
                         translationDelta);

            // Reset our starting positions, ready for the next delta
            _oldFinger1 = currentFinger1;
            _oldFinger2 = currentFinger2;
            _oldScaleFactor = distanceRatio;

            // Finally update the image display translation and scale factors to take acocunt of the pinch change
            UpdateImage(scaleFactor, translationDelta);
        }



        /// <summary>
        /// Calculate the amount of translation required to ensure that the points of the image underneath the fingers in the middle of a pinch zoom operation
        /// stay the same (i.e. natural zooming)
        /// </summary>
        /// <param name="currentFinger1">The current position of the first pinch point</param>
        /// <param name="currentFinger2">The current position of the second pinch point</param>
        /// <param name="oldFinger1">The original position of the first pinch point (since the last update)</param>
        /// <param name="oldFinger2">The original position of the second point point (since the last update)</param>
        /// <param name="currentPosition">The current translation offset</param>
        /// <param name="scaleFactor">The new scale (zoom) factor applied to the image</param>
        /// <returns>A new translation offset for the image</returns>
        private Point GetTranslationDelta(
            Point currentFinger1, Point currentFinger2,
            Point oldFinger1, Point oldFinger2,
            Point currentPosition, double scaleFactor)
        {
            Point newPos1 = new Point(
                currentFinger1.X + (currentPosition.X - oldFinger1.X)*scaleFactor,
                currentFinger1.Y + (currentPosition.Y - oldFinger1.Y)*scaleFactor);

            Point newPos2 = new Point(
                currentFinger2.X + (currentPosition.X - oldFinger2.X)*scaleFactor,
                currentFinger2.Y + (currentPosition.Y - oldFinger2.Y)*scaleFactor);

            Point newPos = new Point(
                (newPos1.X + newPos2.X)/2,
                (newPos1.Y + newPos2.Y)/2);

            Point translationDelta = new Point(newPos.X - currentPosition.X, newPos.Y - currentPosition.Y);
            Logger.Debug("Translation delta as a result of zoom: {0}, {1}", translationDelta.X, translationDelta.Y);
            return translationDelta;
        }



        /// <summary>
        /// Update the image based on the <paramref name="scaleFactorDelta"/> (change to zoom amount) and <paramref name="translationDelta"/> (change to current position)
        /// </summary>
        /// <param name="scaleFactorDelta">The factor (multiply) by which the zoom has changed on the image (as the result of a pinch operation)</param>
        /// <param name="translationDelta">The amount (add) that the image position has moved by, relative to the top left corner of the Image control</param>
        private void UpdateImage(double scaleFactorDelta, Point translationDelta)
        {
            Logger.Info("Updating image by scale factor delta {0} and translation delta {1}", scaleFactorDelta, translationDelta);

            // Update the scale (zoom), but hold in a temporary variable as we're going to perform multiple operations on it and don't want the view to flicker
            double newScale = Scale;
            newScale *= scaleFactorDelta;
            Logger.Debug("Updating scale factor {0} by {1} to {2}", Scale, scaleFactorDelta, newScale);

            // Keep the total image zoom such that the image always fills the crop area
            newScale = WithinBounds(newScale, MinScaleFactor, 10); // 10 is our arbitrary maximum zoom limit

            Point newPosition = new Point(ImagePosition.X + translationDelta.X, ImagePosition.Y + translationDelta.Y);
            Logger.Debug("Updating image position from {0} by {1} to {2}", ImagePosition, translationDelta, newPosition);

            // Based on the size of the image at the current scale factor, ensure that the image is always within bounds
            double scaledImageHeight = Image.PixelHeight * newScale;
            double scaledImageWidth = Image.PixelWidth * newScale;

            Point leftUpDragExtent = new Point(CropRight - scaledImageWidth, CropBottom - scaledImageHeight);
            Point downRightDragExtent = new Point(CropLeft, CropTop);
            ImagePosition = WithinBounds(newPosition, leftUpDragExtent, downRightDragExtent);

            // Update the scale and cause the view to be updated
            Scale = newScale;
        }


        /// <summary>
        /// Invoked when the user has dragged their finger to pan the image
        /// </summary>
        /// <remarks>
        /// This just updates the translation of the image without change the scale factor</remarks>
        /// <param name="horizontalChange">The amount of change across the image has been moved by</param>
        /// <param name="verticalChange">The amount of change certically the image have been moved by</param>
        public void OnDragDelta(double horizontalChange, double verticalChange)
        {
            Logger.Debug("Drag delta by {0} across and {1} high", horizontalChange, verticalChange);
            // No change to the scale factor, so just pass 1
            UpdateImage(1, new Point(horizontalChange, verticalChange));
        }


        /// <summary>
        /// <para>If <paramref name="value"/> &lt; <paramref name="min"/> return <paramref name="min"/></para>
        /// <para>If <paramref name="value"/> &gt; <paramref name="max"/> return <paramref name="max"/></para>
        /// <para>else return <paramref name="value"/></para>
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns></returns>
        private static double WithinBounds(double value, double min, double max)
        {
            if (value < min)
            {
                Logger.Debug("Limited {0} to min {1}", value, min);
                return min;
            }
            if (value > max)
            {
                Logger.Debug("Limited {0} to max {1}", value, max);
                return max;
            }
            return value;
        }

        /// <summary>
        /// Set <paramref name="point"/> such that <paramref name="point"/>.<see cref="Point.X"/> is between <paramref name="topLeft"/>.<see cref="Point.X"/>
        /// and <paramref name="bottomRight"/>.<see cref="Point.X"/>; and that <paramref name="point"/>.<see cref="Point.Y"/> is between
        /// <paramref name="topLeft"/>.<see cref="Point.Y"/> and <paramref name="bottomRight"/>.<see cref="Point.Y"/>
        /// </summary>
        /// <param name="point">The point to bind the co-ordinates of</param>
        /// <param name="topLeft">The top-left most point <paramref name="point"/> must be within</param>
        /// <param name="bottomRight">The bottom-right most point <paramref name="point"/> must be within</param>
        /// <returns>A new point</returns>
        private static Point WithinBounds(Point point, Point topLeft, Point bottomRight)
        {
            return new Point(WithinBounds(point.X, topLeft.X, bottomRight.X),
                             WithinBounds(point.Y, topLeft.Y, bottomRight.Y));
        }

        #endregion

    }
}