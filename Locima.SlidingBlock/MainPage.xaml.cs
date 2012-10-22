using System;
using System.Windows.Controls;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Controls;
using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NLog;

namespace Locima.SlidingBlock
{
    public partial class MainPage : PhoneApplicationPage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
//            NavigationService.Navigate(EndGameTestSetup.TestEndGameScreen());
//            NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
            LittleWatson.CheckForPreviousException();

            string pageId;
            if (!NavigationContext.QueryString.TryGetValue("menuPage", out pageId))
            {
                pageId = "MainMenu";
            }

            CreateApplicationBar();

            MenuPageViewModel page = MenuPageBroker.RetrieveMenuPage(pageId);

            DataContext = page;
        }


        /// <summary>
        ///   Builds the application page
        /// </summary>
        /// <remarks>
        ///   Invoked from the page navigated to event handler <see cref="OnNavigatedTo" />
        /// </remarks>
        private void CreateApplicationBar()
        {
            Logger.Info("Creating application bar");
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar {Mode = ApplicationBarMode.Minimized};

            // Settings page
            IApplicationBarMenuItem item = ApplicationBarHelper.AddMenuItem(ApplicationBar,
                                                                            LocalizationHelper.GetString(
                                                                                "AboutMenuOption"));
            item.Click +=
                (sender, args) =>
                NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));

            // Player selection
            item = ApplicationBarHelper.AddMenuItem(ApplicationBar, LocalizationHelper.GetString("SelectPlayer"));
            item.Click +=
                (sender, args) => NavigationService.Navigate(new Uri("/PlayerSelector.xaml", UriKind.Relative));

            // Highscore review
            item = ApplicationBarHelper.AddMenuItem(ApplicationBar, LocalizationHelper.GetString("GoToHighscores"));
            item.Click += (sender, args) => NavigationService.Navigate(HighScores.CreateNavigationUri());
        }


        /// <summary>
        ///   Handle the user selecting a menu option (which is a listbox item)
        /// </summary>
        private void MainMenuListboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MenuItemViewModel item = (MenuItemViewModel) mainMenuListbox.SelectedItem;
            if (item != null)
            {
                if (item.IsEnabled)
                {
                    Uri navUri = item.Invoke();
                    NavigationService.Navigate(navUri);
                }
                else
                {
                    Logger.Debug("Ignoring selection change to disabled item");
                }
                // Best Practice: Reset selected index to -1 to ensure that if the Listbox contains duplicates that when moving from one to another the event is detected
                // Looks like a defect in the Listbox implementation
                mainMenuListbox.SelectedIndex = -1;
            }
        }


        public static Uri CreateNavigationUri()
        {
            return new Uri("/MainPage.xaml", UriKind.Relative);
        }
    }
}