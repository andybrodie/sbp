using System;
using System.Windows.Controls;
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
    /// MVVM view to show a list of custom game types to select and edit
    /// </summary>
    public partial class GameDefinitionSelector : PhoneApplicationPage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Calls <see cref="InitializeComponent"/>
        /// </summary>
        public GameDefinitionSelector()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Convenience access for the view model that is initialise in the XAML
        /// </summary>
        public GameDefinitionSelectorViewModel ViewModel
        {
            get { return ((GameDefinitionSelectorViewModel) Resources["viewModel"]); }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            BuildApplicationBar();
            ViewModel.Initialise();
            DefaultMessageHandlers.Register(this, ViewModel);
        }


        private void BuildApplicationBar()
        {
            Logger.Info("Creating application bar");

            ApplicationBar = new ApplicationBar();
            IApplicationBarIconButton icon = ApplicationBarHelper.AddButton(ApplicationBar,
                                                                            ApplicationBarHelper.ButtonIcons["New"],
                                                                            LocalizationHelper.GetString(
                                                                                "CreateCustomGame"));
            icon.Click += (o, args) => ViewModel.CreateGameDefinitionCommand.Execute(null);
        }


        /// <summary>
        /// Creates a Uri that can be used to navigate to this page
        /// </summary>
        /// <returns></returns>
        public static Uri GetNavigationUri()
        {
            return new Uri("/GameDefinitionSelector.xaml", UriKind.Relative);
        }

        private void CustomGameListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox gameListListBox = (ListBox) sender;
            if (gameListListBox.SelectedIndex != -1)
            {
                ViewModel.SelectGameDefinitionCommand.Execute(gameListListBox.SelectedItem);
                gameListListBox.SelectedIndex = -1;
            }
        }
    }
}