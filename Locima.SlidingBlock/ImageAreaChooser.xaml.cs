using System;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Controls;
using Locima.SlidingBlock.Messaging;
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

        /// <summary>
        /// Call <see cref="InitializeComponent"/> and <see cref="BuildApplicationBar"/>
        /// </summary>
        public ImageAreaChooser()
        {
            InitializeComponent();
            BuildApplicationBar();
            SizeChanged += delegate
                                    {
                                        ViewModel.TotalWidth = ContentCanvas.ActualWidth;
                                        ViewModel.TotalHeight = ContentCanvas.ActualHeight;
                                    };

        }

        /// <summary>
        /// Convenience access for the view model, initalised in XAML
        /// </summary>
        private ImageAreaChooserViewModel ViewModel
        {
            get { return Resources["ImageChooserViewModel"] as ImageAreaChooserViewModel; }
        }


        /// <summary>
        /// Initialise the view model constants
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DefaultMessageHandlers.Register(this, ViewModel);

            string imageId = this.GetQueryParameter(ImageIdQueryParameterName);
            string gameTemplateId = this.GetQueryParameter(GameTemplateIdQueryParameterName);
            int levelIndex = this.GetQueryParameterAsInt(LevelIndexQueryParameterName);

            ImageAreaChooserViewModel vm = ViewModel;

            vm.GameTemplateId = gameTemplateId;
            vm.LevelIndex = levelIndex;
            vm.ImageControl = ImgZoom;

            // Dark rectangle surrounding the selectable image is 30, 30 
            // TODO Remove these hard-coded values and calculate them on the fly
            vm.CropTop = 30;
            vm.CropLeft = 30;
//            vm.CropWidth = vm.TotalWidth - vm.CropLeft;
//            vm.CropHeight = vm.TotalHeight - vm.CropTop;
            vm.CropWidth = 420;
            vm.CropHeight = 420;
            vm.TotalWidth = ContentCanvas.ActualWidth;
            vm.TotalHeight = ContentCanvas.ActualHeight;
            vm.ImageId = imageId;
            vm.Scale = 2;
            vm.Initialise();
        }


        private void BuildApplicationBar()
        {
            IApplicationBar appBar = new ApplicationBar();

            IApplicationBarIconButton button = ApplicationBarHelper.AddButton(appBar,
                                                                              ApplicationBarHelper.ButtonIcons["Tick"],
                                                                              LocalizationHelper.GetString("OK"));
            button.Click += ViewModel.AcceptImage;

            button = ApplicationBarHelper.AddButton(appBar, ApplicationBarHelper.ButtonIcons["Cancel"],
                                                    LocalizationHelper.GetString("Cancel"));
            button.Click += (sender, args) => Dispatcher.BeginInvoke(() => NavigationService.GoBack());

            ApplicationBar = appBar;
        }


        /// <summary>
        /// Create a Uri to navigate to this page
        /// </summary>
        /// <returns></returns>
        public static Uri CreateNavigationUri(string gameTemplateId, int levelIndex, string imageId)
        {
            return new Uri(
                string.Format("/ImageAreaChooser.xaml?{0}={1}&{2}={3}&{4}={5}", GameTemplateIdQueryParameterName,
                              HttpUtility.UrlEncode(gameTemplateId), LevelIndexQueryParameterName, levelIndex,
                              ImageIdQueryParameterName,
                              HttpUtility.UrlEncode(imageId)), UriKind.Relative);
        }


        /// <summary>
        /// Delegates to <see cref="ImageAreaChooserViewModel.OnPinchStarted"/>
        /// </summary>
        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            Point currentFinger1 = e.GetPosition(ImgZoom, 0);
            Point currentFinger2 = e.GetPosition(ImgZoom, 1);
            Logger.Info("Starting pinch operation from ({0}, {1}) and ({2},{3})", currentFinger1.X, currentFinger1.Y,
                        currentFinger2.X, currentFinger2.Y);
            ViewModel.OnPinchStarted(currentFinger1, currentFinger2);
        }


        /// <summary>
        /// Delegates to <see cref="ImageAreaChooserViewModel.OnPinchDelta"/>
        /// </summary>
        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            Point currentFinger1 = e.GetPosition(ImgZoom, 0);
            Point currentFinger2 = e.GetPosition(ImgZoom, 1);
            Logger.Info("Pinch delta detected at ({0}, {1}) and ({2},{3}) and distance ratio {4}", currentFinger1.X, currentFinger1.Y,
                        currentFinger2.X, currentFinger2.Y, e.DistanceRatio);
            ViewModel.OnPinchDelta(currentFinger1,currentFinger2, e.DistanceRatio);
        }

        /// <summary>
        /// Logged for debugging
        /// </summary>
        private void OnDragStarted(object sender, DragStartedGestureEventArgs e)
        {
            Logger.Debug("Drag started in direction {0}", e.Direction);
        }

        /// <summary>
        /// Delegates to <see cref="ImageAreaChooserViewModel.OnDragDelta"/>
        /// </summary>
        private void OnDragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            Logger.Debug("Drag delta H:{0} V:{1} D:{2}", e.HorizontalChange, e.VerticalChange, e.Direction);
            ViewModel.OnDragDelta(e.HorizontalChange, e.VerticalChange);
        }
    }
}