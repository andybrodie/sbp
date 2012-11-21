using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Controls;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NLog;

namespace Locima.SlidingBlock
{
    /// <summary>
    /// MVVM view for the Image Area Chooser, that allows the user to select part of an image to use for a puzzle
    /// </summary>
    public partial class ImageAreaChooser : PhoneApplicationPage
    {
        private const string ImageIdQueryParameterName = "ImageId";
        private const string LevelIndexQueryParameterName = "levelIndex";
        private const string GameTemplateIdQueryParameterName = "gameTemplateId";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Point _imagePosition = new Point(0, 0);
        private Point _oldFinger1;
        private Point _oldFinger2;
        private double _oldScaleFactor;
        private double _totalImageScale = 1d;
        private string _gameTemplateId;
        private int _levelIndex;

        /// <summary>
        /// Call <see cref="InitializeComponent"/> and <see cref="BuildApplicationBar"/>
        /// </summary>
        public ImageAreaChooser()
        {
            InitializeComponent();
            BuildApplicationBar();
        }

        /// <summary>
        /// Convenience access for the view model, initalised in XAML
        /// </summary>
        private ImageAreaChooserViewModel ViewModel
        {
            get { return Resources["imageChooserViewModel"] as ImageAreaChooserViewModel; }
        }


        /// <summary>
        /// Initialise the view model constants
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _gameTemplateId = this.GetQueryParameter(GameTemplateIdQueryParameterName);
            _levelIndex = this.GetQueryParameterAsInt(LevelIndexQueryParameterName);

            ImageAreaChooserViewModel iacvm = ViewModel;
            Debug.Assert(iacvm != null);
            iacvm.CropTop = 30;
            iacvm.CropLeft = 30;
            iacvm.CropWidth = 420;
            iacvm.CropHeight = 420;
            iacvm.TotalWidth = ContentCanvas.ActualWidth;
            iacvm.TotalHeight = ContentCanvas.ActualHeight;
            iacvm.ImageId = this.GetQueryParameter(ImageIdQueryParameterName);
            iacvm.Initialise();
        }


        private void BuildApplicationBar()
        {
            IApplicationBar appBar = new ApplicationBar();

            IApplicationBarIconButton button = ApplicationBarHelper.AddButton(appBar,
                                                                              ApplicationBarHelper.ButtonIcons["Tick"],
                                                                              LocalizationHelper.GetString("OK"));
            button.Click += AcceptImage;

            button = ApplicationBarHelper.AddButton(appBar, ApplicationBarHelper.ButtonIcons["Cancel"],
                                                    LocalizationHelper.GetString("Cancel"));
            button.Click += (sender, args) => Dispatcher.BeginInvoke(() => NavigationService.GoBack());

            ApplicationBar = appBar;
        }
        

        private void AcceptImage(object sender, EventArgs e)
        {
            // Grab the contents of the window, scale to the required amount and save the image in isolated storage as this is our "master image" for the puzzle
            int offsetX = (int) (_imagePosition.X - ViewModel.CropLeft);
            int offsetY = (int) (_imagePosition.Y - ViewModel.CropTop);
            int imageSizeX = (int) (_totalImageScale*ViewModel.CropWidth);
            int imageSizeY = (int) (_totalImageScale*ViewModel.CropHeight);

            Logger.Info("Creating cropped image from {0},{1} of width {2} and height {3} pixels.", offsetX, offsetY,
                        imageSizeX, imageSizeY);

            WriteableBitmap sourceBitmap = new WriteableBitmap(ViewModel.Image);
            WriteableBitmap puzzleBitmap = sourceBitmap.Crop(offsetX, offsetY, imageSizeX, imageSizeY);

            // Reduce the size of the image to the largest size it will ever be displayed at
            if (imageSizeX > LevelDefinition.ImageSizeX || imageSizeY > LevelDefinition.ImageSizeY)
            {
                Logger.Debug("Resizing to {0} x {1}", LevelDefinition.ImageSizeX, LevelDefinition.ImageSizeY);
                puzzleBitmap.Resize(LevelDefinition.ImageSizeX, LevelDefinition.ImageSizeY,
                                    WriteableBitmapExtensions.Interpolation.Bilinear);
            }

            // Save the image
            ImageStorageManager.Instance.Save(ViewModel.ImageId, puzzleBitmap);

            // Update the GameTemplate's Level image to the new image
            GameTemplate template = GameTemplateStorageManager.Instance.Load(_gameTemplateId);
            template.Levels[_levelIndex].XapImageUri = null;
            template.Levels[_levelIndex].ImageId = ViewModel.ImageId;
            GameTemplateStorageManager.Instance.Save(template);

            // Go back to the Level Editor, suppressing the ImageChooser page
            NavigationService.RemoveBackEntry();
            Dispatcher.BeginInvoke(() => NavigationService.GoBack());
//                NavigationService.Navigate(LevelEditor.CreateNavigationUri(_gameTemplateId, _levelIndex, false,
//                                                                           ViewModel.ImageId)));
        }


        /// <summary>
        /// Create a Uri to navigate to this page
        /// </summary>
        /// <returns></returns>
        public static Uri CreateNavigationUri(string gameTemplateId, int levelIndex, string imageId)
        {
            return new Uri(
                string.Format("/ImageAreaChooser.xaml?&{0}={1}&{2}={3}&{4}={5}", GameTemplateIdQueryParameterName,
                              HttpUtility.UrlEncode(gameTemplateId), LevelIndexQueryParameterName, levelIndex, ImageIdQueryParameterName,
                              HttpUtility.UrlEncode(imageId)), UriKind.Relative);
        }

        #region Image Pan and Zoom

        private void OnPinchStarted(object s, PinchStartedGestureEventArgs e)
        {
            _oldFinger1 = e.GetPosition(ImgZoom, 0);
            _oldFinger2 = e.GetPosition(ImgZoom, 1);
            _oldScaleFactor = 1;
        }


        private void OnPinchDelta(object s, PinchGestureEventArgs e)
        {
            double scaleFactor = e.DistanceRatio / _oldScaleFactor;

            Point currentFinger1 = e.GetPosition(ImgZoom, 0);
            Point currentFinger2 = e.GetPosition(ImgZoom, 1);

            Point translationDelta = GetTranslationDelta(
                currentFinger1,
                currentFinger2,
                _oldFinger1,
                _oldFinger2,
                _imagePosition,
                scaleFactor);

            _oldFinger1 = currentFinger1;
            _oldFinger2 = currentFinger2;
            _oldScaleFactor = e.DistanceRatio;

            UpdateImage(scaleFactor, translationDelta);
        }


        private void UpdateImage(double scaleFactor, Point delta)
        {
            Logger.Debug("Updating scale factor {0} by {1} to {2}", _totalImageScale, scaleFactor,
                         (_totalImageScale * scaleFactor));
            _totalImageScale *= scaleFactor;

            Point newPosition = new Point(_imagePosition.X + delta.X, _imagePosition.Y + delta.Y);
            Logger.Debug(string.Format("Updating image position from {0} by {1} to {2}", _imagePosition, delta,
                                       newPosition));
            _imagePosition = newPosition;

            // Calculate minimum scale factor and don't allow scale factor to mean picture can't fill square
            double minScaleFactor = ViewModel.CropWidth / ViewModel.Image.PixelWidth;
            _totalImageScale = WithinBounds(_totalImageScale, minScaleFactor, 100.00);
            Logger.Debug("WIDTH: " + ViewModel.Image.PixelWidth);

            _imagePosition = WithinBounds(_imagePosition, new Point(-480, -480), new Point(30, 30));
            // TODO Change first point to calculated image size

            CompositeTransform transform = (CompositeTransform)ImgZoom.RenderTransform;
            transform.ScaleX = _totalImageScale;
            transform.ScaleY = _totalImageScale;
            transform.TranslateX = _imagePosition.X;
            transform.TranslateY = _imagePosition.Y;
        }


        private Point GetTranslationDelta(
            Point currentFinger1, Point currentFinger2,
            Point oldFinger1, Point oldFinger2,
            Point currentPosition, double scaleFactor)
        {
            Point newPos1 = new Point(
                currentFinger1.X + (currentPosition.X - oldFinger1.X) * scaleFactor,
                currentFinger1.Y + (currentPosition.Y - oldFinger1.Y) * scaleFactor);

            Point newPos2 = new Point(
                currentFinger2.X + (currentPosition.X - oldFinger2.X) * scaleFactor,
                currentFinger2.Y + (currentPosition.Y - oldFinger2.Y) * scaleFactor);

            Point newPos = new Point(
                (newPos1.X + newPos2.X) / 2,
                (newPos1.Y + newPos2.Y) / 2);

            return new Point(
                newPos.X - currentPosition.X,
                newPos.Y - currentPosition.Y);
        }


        private void OnDragStarted(object sender, DragStartedGestureEventArgs e)
        {
            Logger.Info("Drag started: {0}", e.Direction);
        }


        private void OnDragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            Logger.Debug("Drag delta in {2}.  H:{0}.  V:{1}", e.HorizontalChange, e.VerticalChange, e.Direction);
            UpdateImage(1, new Point(e.HorizontalChange, e.VerticalChange));
        }


        private double WithinBounds(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private Point WithinBounds(Point point, Point topLeft, Point bottomRight)
        {
            if (point.X < topLeft.X) point.X = topLeft.X;
            if (point.X > bottomRight.X) point.X = bottomRight.X;
            if (point.Y < topLeft.Y) point.Y = topLeft.Y;
            if (point.Y > bottomRight.Y) point.Y = bottomRight.Y;
            return point;
        }

        #endregion 


    }
}