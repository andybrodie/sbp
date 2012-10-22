using System;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Controls;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Persistence;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Locima.SlidingBlock
{

    /// <summary>
    /// Very simple (non-MVVM) page that allows an existing player to be edited or a new one created.
    /// </summary>
    public partial class AddPlayer : PhoneApplicationPage
    {
        /// <summary>
        /// The query parameter to specify an existing player to edit
        /// </summary>
        private const string PlayerIdQueryParameterName = "PlayerId";

        private PlayerDetails _player;

        public AddPlayer()
        {
            InitializeComponent();
            Loaded += FocusNameElement;
        }


        /// <summary>
        /// This will pop up the keyboard immediately and set the caret position to be at the end of the string
        /// </summary>
        /// <remarks>
        /// If you wanted to select all the text, then use NameTextBox.SelectAll(), but that will inhibit capitals being set as the default character (annoyingly)
        /// </remarks>
        void FocusNameElement(object sender, RoutedEventArgs e)
        {
            NameTextBox.Focus();
            NameTextBox.SelectionStart = NameTextBox.Text.Length;
            NameTextBox.SelectionLength = 0;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ApplicationBar = new ApplicationBar();

            string playerId;
            if (NavigationContext.QueryString.TryGetValue(PlayerIdQueryParameterName, out playerId))
            {
                // An existing player is to be edited
                _player = PlayerStorageManager.Instance.Load(HttpUtility.UrlDecode(playerId));
                NameTextBox.Text = _player.Name;
                PageTitle.Text = LocalizationHelper.GetString("EditPlayerTitle");
            }
            else
            {
                PageTitle.Text = LocalizationHelper.GetString("AddNewPlayerTitle");
            }

            IApplicationBarIconButton icon = ApplicationBarHelper.AddButton(ApplicationBar,
                                                                            ApplicationBarHelper.Buttons["Save"],
                                                                            LocalizationHelper.GetString("SavePlayer"));
            icon.Click += SavePlayerClick;
            icon = ApplicationBarHelper.AddButton(ApplicationBar, ApplicationBarHelper.Buttons["Cancel"],
                                                  LocalizationHelper.GetString("Cancel"));
            icon.Click += CancelClick;

        }


        private void CancelClick(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }


        private void SavePlayerClick(object sender, EventArgs eventArgs)
        {
            // If we're editing an existing player, _player is not null, so update the existing player and save it.  If not, create a new player from scratch
            if (_player == null)
            {
                PlayerStorageManager.Instance.SavePlayer(new PlayerDetails
                    {
                        Name = NameTextBox.Text,
                        PreferredColor = (Color) Application.Current.Resources["PhoneAccentColor"]
                    });
            } else
            {
                if (string.IsNullOrEmpty(NameTextBox.Text))
                {
                    NameTextBox.Text = LocalizationHelper.GetString("UnnamedPlayer");
                }
                _player.Name = NameTextBox.Text;
                PlayerStorageManager.Instance.SavePlayer(_player);
            }
            NavigationService.GoBack();
        }


        /// <summary>
        /// Creates a Uri that navigates to this page to edit the player passed by <paramref name="playerId"/>, or createe a new player
        /// </summary>
        /// <param name="playerId">The ID of the player to edit or <c>null</c> if a new player is to be created</param>
        /// <returns>A relative Uri that can be passed to <see cref="NavigationService"/></returns>
        public static Uri CreateNavigationUri(string playerId)
        {
            return string.IsNullOrEmpty(playerId)
                       ? new Uri("/AddPlayer.xaml", UriKind.Relative)
                       : new Uri(
                             string.Format("/AddPlayer.xaml?{0}={1}", PlayerIdQueryParameterName,
                                           HttpUtility.UrlEncode(playerId)), UriKind.Relative);
        }
    }
}