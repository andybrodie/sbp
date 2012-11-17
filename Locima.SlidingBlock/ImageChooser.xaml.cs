using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.IO;
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
        private const string LevelIndexQueryParameterName = "levelIndex";
        private const string GameTemplateIdQueryParameterName = "gameTemplateId";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly PhotoChooserTask _photoChooserTask;
        private string _gameTemplateId;
        private int _levelIndex;

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
            Initialise();
        }


        private void Initialise()
        {
            _gameTemplateId = this.GetQueryParameter(GameTemplateIdQueryParameterName, s => s);
            _levelIndex = this.GetQueryParameterAsInt(LevelIndexQueryParameterName);
            DataContext = GetImageProviders();
        }


        private MenuPageViewModel GetImageProviders()
        {
            MenuPageViewModel mpvm = new MenuPageViewModel {MenuItems = new ObservableCollection<MenuItemViewModel>()};
            MenuItemViewModel item = new MenuItemViewModel
                                         {
                                             Title = LocalizationHelper.GetString("LocalPictures"),
                                             SelectedAction = delegate
                                                                  {
                                                                      this.LaunchChooserSafely(_photoChooserTask);
                                                                      return null;
                                                                  },
                                             Text = LocalizationHelper.GetString("LocalPicturesDescription")
                                         };
            mpvm.MenuItems.Add(item);

            item = new MenuItemViewModel {Title = "Enter URL", Text = "Enter or paste a URL to an image on line"};
            mpvm.MenuItems.Add(item);

            return mpvm;
        }


        private void PhotoChooserTaskCompleted(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                string photoId = ImageStorageManager.Instance.SaveTemporary(e.ChosenPhoto);
                Uri areaChooserUri = ImageAreaChooser.CreateNavigationUri(_gameTemplateId, _levelIndex, photoId);
                Dispatcher.BeginInvoke(() => NavigationService.Navigate(areaChooserUri));
            }
        }


        private void MainMenuListboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = (ListBox) sender;
            if (listBox.SelectedIndex > -1)
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
                    item.SelectedAction(item.Parameter);
                }

                // Reset selected index back to -1 so if we navigate back to this page it won't instantly fire this
                mainMenuListbox.SelectedIndex = -1;
            }
            else
            {
                Logger.Debug("Ignoring selection changed event as CurrentIndex is -1");
            }
        }


        /// <summary>
        /// Creates a Uri to navigate to this page and prompt the user for a source to find an image
        /// </summary>
        /// <param name="gameTemplateId">The ID of the game template to replace the picture of</param>
        /// <param name="levelIndex"></param>
        /// <returns>A Uri to navigate to this page</returns>
        public static Uri CreateNavigationUri(string gameTemplateId, int levelIndex)
        {
            return new Uri(
                string.Format("/ImageChooser.xaml?{0}={1}&{2}={3}", GameTemplateIdQueryParameterName,
                              HttpUtility.UrlEncode(gameTemplateId), LevelIndexQueryParameterName, levelIndex
                    ), UriKind.Relative);
        }
    }
}