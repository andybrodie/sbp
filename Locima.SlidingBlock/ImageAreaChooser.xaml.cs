using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Controls;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Persistence;
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Point _imagePosition = new Point(0, 0);

        private Point _oldFinger1;
        private Point _oldFinger2;
        private double _oldScaleFactor;
        private double _totalImageScale = 1d;

        private SaveGame _puzzle;
        private const string ImageStoredInSaveGameQueryParameterName = "imageStoredInPuzzle";
        private const string AcceptUriQueryParameterName = "acceptUri";


        /// <summary>
        /// Call <see cref="InitializeComponent"/> and build the application bar
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
        /// <param name="e"></param>

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _puzzle = SaveGameStorageManager.Instance.GetContinuableGame(PlayerStorageManager.Instance.CurrentPlayer.Id);
            ImageAreaChooserViewModel iacvm = ViewModel;
            Debug.Assert(iacvm != null);
            iacvm.CropTop = 30;
            iacvm.CropLeft = 30;
            iacvm.CropWidth = 420;
            iacvm.CropHeight = 420;
            iacvm.TotalWidth = ContentCanvas.ActualWidth;
            iacvm.TotalHeight = ContentCanvas.ActualHeight;

            string value;
            if (NavigationContext.QueryString.TryGetValue(ImageStoredInSaveGameQueryParameterName, out value))
            {
                // If loading the image from within the application, then it's already loaded, just pass a reference to the viewmodel
                if (Boolean.TrueString.Equals(value,StringComparison.InvariantCultureIgnoreCase))
                {
                    Logger.Info("Loading image from current puzzle model metadata");
                    iacvm.Image = _puzzle.CurrentLevel.Image;
                }
            }
            else if (NavigationContext.QueryString.TryGetValue("imageUrl", out value))
            {
                // If the image needs to be downloaded, then download it asynchronously and pass to the viewmodel in BitmapOnImageOpened
                if (!string.IsNullOrEmpty(value))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.CreateOptions = BitmapCreateOptions.BackgroundCreation;
                    bitmap.ImageOpened += BitmapOnImageOpened;
                    bitmap.ImageFailed += BitmapOnImageFailed;
                    
                    string imageFileUrl = HttpUtility.UrlDecode(value);
                    bitmap.UriSource = new Uri(imageFileUrl, UriKind.Relative);

                    // TODO Add a progress bar or something...
                }
            }
            else
            {
                MessageBox.Show("No image found"); // TODO Localize
            }
        }


        private void BuildApplicationBar()
        {
            IApplicationBar appBar = new ApplicationBar();

            ApplicationBarHelper.AddButton(appBar, ApplicationBarHelper.ButtonIcons["Tick"],
                                           LocalizationHelper.GetString("OK"));
            ApplicationBarHelper.AddButton(appBar, ApplicationBarHelper.ButtonIcons["Cancel"],
                                           LocalizationHelper.GetString("Cancel"));

            ApplicationBar = appBar;
        }


        private void BitmapOnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            Logger.Fatal("Image opening failed", exceptionRoutedEventArgs);
        }


        private void BitmapOnImageOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            Logger.Info("Loaded bitmap successfully");
            ViewModel.Image = new WriteableBitmap((BitmapImage) sender);
        }


        private void OnPinchStarted(object s, PinchStartedGestureEventArgs e)
        {
            _oldFinger1 = e.GetPosition(ImgZoom, 0);
            _oldFinger2 = e.GetPosition(ImgZoom, 1);
            _oldScaleFactor = 1;
        }


        private void OnPinchDelta(object s, PinchGestureEventArgs e)
        {
            double scaleFactor = e.DistanceRatio/_oldScaleFactor;

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
                         (_totalImageScale*scaleFactor));
            _totalImageScale *= scaleFactor;

            Point newPosition = new Point(_imagePosition.X + delta.X, _imagePosition.Y + delta.Y);
            Logger.Debug(string.Format("Updating image position from {0} by {1} to {2}", _imagePosition, delta,
                                       newPosition));
            _imagePosition = newPosition;

            // Calculate minimum scale factor and don't allow scale factor to mean picture can't fill square
            double minScaleFactor = ViewModel.CropWidth/ViewModel.Image.PixelWidth;
            _totalImageScale = WithinBounds(_totalImageScale, minScaleFactor, 100.00);
            Logger.Debug("WIDTH: " + ViewModel.Image.PixelWidth);

            _imagePosition = WithinBounds(_imagePosition, new Point(-480, -480), new Point(30, 30));
            // TODO Change first point to calculated image size

            CompositeTransform transform = (CompositeTransform) ImgZoom.RenderTransform;
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
                currentFinger1.X + (currentPosition.X - oldFinger1.X)*scaleFactor,
                currentFinger1.Y + (currentPosition.Y - oldFinger1.Y)*scaleFactor);

            Point newPos2 = new Point(
                currentFinger2.X + (currentPosition.X - oldFinger2.X)*scaleFactor,
                currentFinger2.Y + (currentPosition.Y - oldFinger2.Y)*scaleFactor);

            Point newPos = new Point(
                (newPos1.X + newPos2.X)/2,
                (newPos1.Y + newPos2.Y)/2);

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
//            ImageAreaChooserViewModel viewModel = ViewModel;
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

            if (imageSizeX > 480)
            {
                Logger.Debug("Resizing to 480 x 480");
                puzzleBitmap.Resize(480, 480, WriteableBitmapExtensions.Interpolation.Bilinear);
            }

            _puzzle.CurrentLevel.SetImage(puzzleBitmap); // TODO Fix this, it's messy
            SaveGameStorageManager.Instance.SaveGame(_puzzle);

            Uri acceptUri = new Uri(HttpUtility.UrlDecode(NavigationContext.QueryString[AcceptUriQueryParameterName]),UriKind.Relative);
            Logger.Info("Navigating accept to {0}", acceptUri);
            Dispatcher.BeginInvoke(() => NavigationService.Navigate(acceptUri));
        }

        
        /// <summary>
        /// Create a Uri to navigate to this page
        /// </summary>
        /// <param name="currentUri">The Uri to navigate to if if the user accepts an area of the image</param>
        /// <param name="imageStoredInSaveGame">If true then the image will be loaded from the latest save game</param>
        /// <returns></returns>
        public static Uri CreateNavigationUri(Uri currentUri, bool imageStoredInSaveGame)
        {
            return new Uri(
                string.Format(
                    "/ImageAreaChooser.xaml?&{0}={1}&{2}={3}", AcceptUriQueryParameterName,
                    HttpUtility.UrlEncode(currentUri.ToString()), ImageStoredInSaveGameQueryParameterName, imageStoredInSaveGame),
                UriKind.Relative);
        }

    }
} 