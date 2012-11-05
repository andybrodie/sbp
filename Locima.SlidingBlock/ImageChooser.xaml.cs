using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Persistence;
using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using NLog;

namespace Locima.SlidingBlock
{

    /// <summary>
    /// A regular "code-behind" page which allows the user the pick an image from their albums, or use the camera to take a new image
    /// </summary>
    public partial class ImageChooser : PhoneApplicationPage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly PhotoChooserTask _photoChooserTask;

        /// <summary>
        /// Calls <see cref="InitializeComponent"/> and sets up the <see cref="_photoChooserTask"/>
        /// </summary>
        public ImageChooser()
        {
            InitializeComponent();
            _photoChooserTask = new PhotoChooserTask {ShowCamera = true};
            _photoChooserTask.Completed += PhotoChooserTaskCompleted;
        }


        /// <summary>
        /// Creates the data context for the page (which is a list of places where images can be obtained from) and sets it to <see cref="FrameworkElement.DataContext"/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataContext = GetImageProviders();
        }


        private MenuPageViewModel GetImageProviders()
        {
            MenuPageViewModel mpvm = new MenuPageViewModel();
            mpvm.MenuItems = new ObservableCollection<MenuItemViewModel>();
            MenuItemViewModel item = new MenuItemViewModel();
            item.Title = LocalizationHelper.GetString("LocalPictures");
            item.SelectedAction = delegate
                                      {
                                          LaunchChooserSafely(_photoChooserTask);
                                          return null;
                                      };
            item.Text = LocalizationHelper.GetString("LocalPicturesDescription");
            mpvm.MenuItems.Add(item);

            item = new MenuItemViewModel();
            item.Title = "Enter URL";
            item.Text = "Enter or paste a URL to an image on line";
            mpvm.MenuItems.Add(item);

            item = new MenuItemViewModel();
            item.Title = "Twitter Search";
            item.Text = "Search for tweets containing puzzle definitions";
            mpvm.MenuItems.Add(item);
            return mpvm;
        }


        /// <summary>
        ///   Launches a chooser task, suppressing the <see cref="InvalidOperationException" /> thrown if the chooser is launched twice (where the user double-taps the icon to choose)
        /// </summary>
        private static void LaunchChooserSafely<TTaskEventArgs>(ChooserBase<TTaskEventArgs> chooser)
            where TTaskEventArgs : TaskEventArgs
        {
            try
            {
                chooser.Show();
            }
            catch (InvalidOperationException)
            {
                Logger.Debug(
                    "Supressed InvalidOperationException caused by user double-tapping the control that launches a chooser");
            }
        }


        private void PhotoChooserTaskCompleted(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                // We don't have to be too careful here with the format or size of the file.  This is coming out of the phone image chooser, so we're going
                // to assume that 1) it gives us valid images; 2) the images aren't too big for the phone to handle.  TODO Verify whether these assumptions are valid?
                ISaveGameStorageManager psm = SaveGameStorageManager.Instance;
                SaveGame puzzle = null; // BUG psm.GetContinuableGame();
                puzzle.EnsureLevels(1);
                puzzle.Levels[0].SetAndSaveImage(e.ChosenPhoto); // TODO Make this level aware
                psm.SaveGame(puzzle);

                // Navigate to the Image Area Chooser (which allows you to select the portion of the image used in the puzzle)
                // Note that you can only navigate off the main UI thread, so we have to use an async invocation
                // Note we have to tell the image chooser where to come back to

                JournalEntry back = NavigationService.BackStack.Reverse().First();
                Uri currentUri = back.Source;

                Uri areaChooserUri = ImageAreaChooser.CreateNavigationUri(currentUri, true);

                Dispatcher.BeginInvoke(() => NavigationService.Navigate(areaChooserUri));
            }
        }


        private void MainMenuListboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            MenuItemViewModel item = mainMenuListbox.SelectedItem as MenuItemViewModel;
            Debug.Assert(item != null,
                         string.Format(
                             "Selected item {0} was null or not an instance of MenuItemViewModel.  This is a bug.",
                             mainMenuListbox.SelectedItem));
            if (item.SelectedAction == null)
            {
                Uri navUri = item.TargetUri ??
                             new Uri("/MainPage.xaml?menuPage=" + HttpUtility.UrlEncode(item.TargetPage),
                                     UriKind.Relative);
                NavigationService.Navigate(navUri);
            }
            else
            {
                item.SelectedAction();
            }

            // Reset selected index to -1
            // TODO Work out why I have to do this
            mainMenuListbox.SelectedIndex = -1;
        }


        /// <summary>
        /// Creates a Uri to navigate to this page
        /// </summary>
        /// <returns>An Uri </returns>
        public static Uri CreateNavigationUri()
        {
            return new Uri("/ImageChooser.xaml", UriKind.Relative);
        }
    }
}