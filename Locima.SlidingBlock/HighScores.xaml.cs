using System;
using System.Net;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Messaging;
using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;
using NLog;

namespace Locima.SlidingBlock
{

    /// <summary>
    /// Simple "code-behind" style page that shows the high score table, containing a configurable button in the application bar that allows the user to proceed
    /// </summary>
    public partial class HighScores : PhoneApplicationPage
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The Uri query parameter that contains the label for the single application bar button that allows the user to proceed
        /// </summary>
        public const string ButtonLabelQueryParameter = "ButtonTextLabel";

        /// <summary>
        /// The Uri query parameter that contains the Uri for the page to navigate to that allows the user to proceed
        /// </summary>
        public const string ButtonUriQueryParameter = "ButtonUri";


        /// <summary>
        /// Calls <see cref="InitializeComponent"/>
        /// </summary>
        public HighScores()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Perform view model initialisation
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Logger.Info("OnNavigatedTo entry");
            base.OnNavigatedTo(e);
            Logger.Info("Registering view message handler and initialising view model");
            HighScoresViewModel hsvm = (HighScoresViewModel)Resources["ViewModel"];

            DefaultMessageHandlers.Register(this, hsvm);
            hsvm.RegisterMessageHandler<ScrollToViewMessage>(OnScrollViewer);
            hsvm.Initialise();

            ConfigureHsvmButton(hsvm);
            Logger.Info("OnNavigatedTo exit");
        }


        /// <summary>
        /// Configure the button to proceed, based on parameters passed using the query parameter
        /// </summary>
        /// <param name="hsvm"></param>
        private void ConfigureHsvmButton(HighScoresViewModel hsvm)
        {
            string buttonLabel;
            if (NavigationContext.QueryString.TryGetValue(ButtonLabelQueryParameter, out buttonLabel))
            {
                if (string.IsNullOrEmpty(buttonLabel))
                {
                    Logger.Debug("Using button label {0}", buttonLabel);
                    hsvm.ButtonLabel = LocalizationHelper.GetString(buttonLabel);
                }
                else
                {
                    Logger.Error("Empty parameter value found for {0}, replacing with \"Back\"",
                                 ButtonLabelQueryParameter);
                    hsvm.ButtonLabel = LocalizationHelper.GetString("Back");
                }
            }

            string rawUri;
            if (NavigationContext.QueryString.TryGetValue(ButtonUriQueryParameter, out rawUri))
            {
                if (!string.IsNullOrEmpty(rawUri))
                {
                    Logger.Debug("Found {0} value {1}, converting to Uri", ButtonUriQueryParameter, rawUri);
                    hsvm.ButtonUri = new Uri(HttpUtility.UrlDecode(rawUri), UriKind.Relative);
                }
                else
                {
                    Logger.Debug("Ignoring empty parameter value for {0}, button will navigate back", ButtonUriQueryParameter);
                }
            }
        }


        private void OnScrollViewer(object sender, ScrollToViewMessage scrollToViewMessage)
        {
            if (HighScoresViewModel.HighScoreTableViewObjectName == scrollToViewMessage.Scrollable)
            {
                Logger.Info("Scrolling high score table to {0}", scrollToViewMessage.ItemToScrollTo);
                SaveGameListbox.ScrollIntoView(scrollToViewMessage.ItemToScrollTo);
            }
            else
            {
                Logger.Warn(
                    "Unexpected object specified in scrollToViewMessage, expected \"{0}\" but got {1}",HighScoresViewModel.HighScoreTableViewObjectName,
                    scrollToViewMessage.Scrollable);
            }
        }


        /// <summary>
        /// Default navigation creation URI will navigate to the HighScore page configuring it to show a "Back" button
        /// </summary>
        /// <remarks>Delegates to <see cref="CreateNavigationUri(string)"/> passing <c>Back</c> as the single parameter</remarks>
        public static Uri CreateNavigationUri()
        {
            return CreateNavigationUri("Back");
        }

        /// <summary>
        /// Navigation creation URI will navigate to the high score page and configure the button with a custom label (<paramref name="label"/>) which, when clicked, will go Back.
        /// </summary>
        /// <remarks>
        /// Delegates to <see cref="CreateNavigationUri(string, Uri)"/> passing <paramref name="label"/> and <c>null</c> as the <see cref="Uri"/></remarks>
        public static Uri CreateNavigationUri(string label)
        {
            return CreateNavigationUri(label, null);
        }


        /// <summary>
        /// Navigation creation URI will navigate to the high score page, configure the button with a custom label (<paramref name="label"/>) which, when clicked, will navigate to
        /// <paramref name="rawUri"/>.
        /// </summary>
        public static Uri CreateNavigationUri(string label, Uri rawUri)
        {            
            UriConstructor uri = new UriConstructor("/Highscores.xaml", UriKind.Relative);
            uri.AddParameter(ButtonLabelQueryParameter, label);
            uri.AddParameter(ButtonUriQueryParameter, rawUri==null ? String.Empty : rawUri.ToString());
            return uri.ToUri();
        }
    }
}
