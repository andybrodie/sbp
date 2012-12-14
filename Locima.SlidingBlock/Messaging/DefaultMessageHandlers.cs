using System;
using System.Windows;
using System.Windows.Navigation;
using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;
using NLog;

namespace Locima.SlidingBlock.Messaging
{

    /// <summary>
    /// Some useful default message handlers to save each view having to provide its own handler.  Most handlers are the same, e.g. <see cref="OnNavigationMessage"/>, there's only so
    /// many things you can do with that!
    /// </summary>
    public class DefaultMessageHandlers
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly PhoneApplicationPage _page;

        /// <summary>
        /// Saves <paramref name="page"/> for later use
        /// </summary>
        /// <param name="page"></param>
        public DefaultMessageHandlers(PhoneApplicationPage page)
        {
            _page = page;
        }


        /// <summary>
        /// Default navigation request handler
        /// </summary>
        /// <remarks>
        /// This method has extra handling such that if the <see cref="Uri"/> specified by <paramref name="args"/> (<see cref="NavigationMessageArgs.Uri"/>) is the same as the current Url,
        /// then a random <see cref="Guid"/> will be added to the total navigation Url.  This is because if you don't then the navigation service will essentailly ignore you! </remarks>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnNavigationMessage(object sender, NavigationMessageArgs args)
        {
            if (args.DeleteBackstackEntries != 0)
            {
                Logger.Info("NavigationMessageArgs requests deleting {0} backstack entries", args.DeleteBackstackEntries);
                for (int i = 0; i < args.DeleteBackstackEntries; i++)
                {
                    JournalEntry entry = _page.NavigationService.RemoveBackEntry();
                    Logger.Info("Removed backstack entry {0} ({1}) from the backstack", i, entry.Source);
                }
            }

            if (args.NavigationMode == NavigationMode.Back)
            {
                Logger.Info("Navigating Back based on NavigationMessage from {0}", sender);
                _page.NavigationService.GoBack();
            }
            else
            {
                Uri navUri;

                // Force a refresh if navigating to the same Uri by adding a random unique query parameter (a GUID)
                if (_page.NavigationService.Source == args.Uri)
                {
                    Logger.Debug("Appending random query parameter to force refresh");
                    UriKind uriKind = args.Uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative;
                    string separator;
                    if (args.Uri.ToString().EndsWith("?"))
                    {
                        separator = String.Empty;
                    }
                    else if (args.Uri.ToString().Contains("?"))
                    {
                        separator = "&";
                    }
                    else
                    {
                        separator = "?";
                    }
                    navUri = new Uri(string.Format("{0}{1}random={2}", args.Uri, separator, Guid.NewGuid()), uriKind);
                }
                else
                {
                    navUri = args.Uri;
                }
                Logger.Info("Navigating to {0} based on NavigationMessage from {1}", navUri, sender);
                bool result = _page.NavigationService.Navigate(navUri);
                Logger.Info("Navigation result {0}", result);
            }
        }


        /// <summary>
        /// Default confirmation message handler that pops up a <see cref="MessageBox"/> instance.
        /// </summary>
        /// <param name="sender">The view model object that sent the message</param>
        /// <param name="confirmArgs">Arguments to confire the message box</param>
        public void OnConfirmationMessage(object sender, ConfirmationMessageArgs confirmArgs)
        {
            Logger.Info("Displaying MessageBox based on ConfirmationMessage command: {0} from {1}", confirmArgs.Title, sender);
            MessageBoxButton style = (confirmArgs.OnCancelCommand == null)
                                         ? MessageBoxButton.OK
                                         : MessageBoxButton.OKCancel;
            MessageBoxResult result = MessageBox.Show(confirmArgs.Message, confirmArgs.Title, style);
            if (MessageBoxResult.OK == result)
            {
                Logger.Debug("OK clicked, so checking action {0}", confirmArgs.OnOkCommand);
                if (confirmArgs.OnOkCommand != null && confirmArgs.OnOkCommand.CanExecute(null))
                {
                    Logger.Info("Invoking OK action on confirm message \"{0}\" from {1} and action is available ", confirmArgs.Title, sender);
                    confirmArgs.OnOkCommand.Execute(null);
                }
                else
                {
                    Logger.Debug("No action specified or CanExecute returned false");
                }
            }
            else
            {
                Logger.Debug("Cancel clicked, so checking action {0}", confirmArgs.OnCancelCommand);
                if (confirmArgs.OnCancelCommand != null && confirmArgs.OnCancelCommand.CanExecute(null))
                {
                    Logger.Info("Invoking Cancel action on confirm message \"{0}\" from {1} and action is available ",
                                confirmArgs.Title, sender);
                    confirmArgs.OnCancelCommand.Execute(null);
                }
                else
                {
                    Logger.Debug("No action specified or CanExecute returned false");
                }
            }
        }


        /// <summary>
        /// Registers the default handlers for <paramref name="page"/> on the <paramref name="viewModel"/>
        /// </summary>
        /// <param name="page"></param>
        /// <param name="viewModel"></param>
        public static void Register(PhoneApplicationPage page, ViewModelBase viewModel)
        {
            Logger.Debug("Registering default handlers for NavigationMessageArgs and ConfirmationMessageArgs");
            DefaultMessageHandlers pageHandler = new DefaultMessageHandlers(page);
            viewModel.RegisterMessageHandler<NavigationMessageArgs>(pageHandler.OnNavigationMessage);
            viewModel.RegisterMessageHandler<ConfirmationMessageArgs>(pageHandler.OnConfirmationMessage);
        }

    }
}