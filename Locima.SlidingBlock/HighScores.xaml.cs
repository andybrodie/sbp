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
    public partial class HighScores : PhoneApplicationPage
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public const string ButtonLabelQueryParameter = "ButtonTextLabel";
        public const string ButtonUriQueryParameter = "ButtonUri";

        public HighScores()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Logger.Info("OnNavigatedTo entry");
            base.OnNavigatedTo(e);
            Logger.Info("Registering view message handler and initialising view model");
            HighScoresViewModel hsvm = (HighScoresViewModel)Resources["viewModel"];

            DefaultMessageHandlers.Register(this, hsvm);
            hsvm.RegisterMessageHandler<ScrollToViewMessage>(OnScrollViewer);
            hsvm.Initialise();

            ConfigureHsvmButton(hsvm);
            Logger.Info("OnNavigatedTo exit");
        }


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
        /// Default navigation creation URI will navigate to the Highscore page configuring it to show a "Back" button
        /// </summary>
        /// <remarks>Delegates to <see cref="CreateNavigationUri(string)"/> passing <c>Back</c> as the single parameter</remarks>
        public static Uri CreateNavigationUri()
        {
            return CreateNavigationUri("Back");
        }

        /// <summary>
        /// Navigation creation URI will navigate to the highscore page and configure the button with a custom label (<paramref name="label"/>) which, when clicked, will go Back.
        /// </summary>
        /// <remarks>
        /// Delegates to <see cref="CreateNavigationUri(string, Uri)"/> passing <paramref name="label"/> and <c>null</c> as the <see cref="Uri"/></remarks>
        public static Uri CreateNavigationUri(string label)
        {
            return CreateNavigationUri(label, null);
        }


        /// <summary>
        /// Navigation creation URI will navigate to the highscore page, configure the button with a custom label (<paramref name="label"/>) which, when clicked, will navigate to
        /// <paramref name="rawUri"/>.
        /// </summary>
        public static Uri CreateNavigationUri(string label, Uri rawUri)
        {            
            string uri = string.Format("/HighScores.xaml?{0}={1}&{2}={3}", ButtonLabelQueryParameter, label,
                ButtonUriQueryParameter, rawUri==null ? String.Empty : HttpUtility.UrlEncode(rawUri.ToString()));
            return new Uri(uri, UriKind.Relative);
        }
    }
}
