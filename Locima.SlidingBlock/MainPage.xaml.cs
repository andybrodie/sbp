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

    /// <summary>
    /// The MVVM view for the first page of the App, which presents all the menus to the user
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The query parameter on the Uri for this page that determines which menu page to display
        /// </summary>
        public const string MenuPageQueryParamName = "MenuPage";

        /// <summary>
        /// Calls <See cref="InitializeComponent"/>
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates the application bar and sets up the view model for the desired menu page (passed as a query parameter)
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
//            NavigationService.Navigate(EndGameTestSetup.TestEndGameScreen());
//            NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
//            NavigationService.Navigate(new Uri("/LevelEditor.xaml?gameTemplateId=GameTemplates%5cba4d2757-dc0e-4621-ac0a-9cfcd173c074&levelIndex=0&createNew=False", UriKind.Relative));
            LittleWatson.CheckForPreviousException();

            string pageId;
            if (!NavigationContext.QueryString.TryGetValue(MenuPageQueryParamName, out pageId))
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
                NavigationService.Navigate(GetAboutPageUri());

            // Player selection
            item = ApplicationBarHelper.AddMenuItem(ApplicationBar, LocalizationHelper.GetString("SelectPlayer"));
            item.Click +=
                (sender, args) => NavigationService.Navigate(new Uri("/PlayerSelector.xaml", UriKind.Relative));

            // Highscore review
            item = ApplicationBarHelper.AddMenuItem(ApplicationBar, LocalizationHelper.GetString("GoToHighscores"));
            item.Click += (sender, args) => NavigationService.Navigate(HighScores.CreateNavigationUri());
        }


        /// <summary>
        /// Returns the Uri to the YLAD page (it's here because YLAD is in an included DLL
        /// </summary>
        /// <returns></returns>
        private static Uri GetAboutPageUri()
        {
            return new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative);
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


        /// <summary>
        /// Creates a Uri that navigates to this page
        /// </summary>
        /// <returns></returns>
        public static Uri CreateNavigationUri(string menuPageName)
        {
            string uriString = "/MainPage.xaml";
            if (!string.IsNullOrEmpty(menuPageName))
            {
                uriString = String.Format("{0}?{1}={2}", uriString, MenuPageQueryParamName, menuPageName);
            }

            return new Uri(uriString, UriKind.Relative);
        }
    }
}