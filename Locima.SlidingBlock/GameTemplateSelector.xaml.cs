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
    public partial class GameTemplateSelector : PhoneApplicationPage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Calls <see cref="InitializeComponent"/>
        /// </summary>
        public GameTemplateSelector()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Convenien
        /// ce access for the view model that is initialise in the XAML
        /// </summary>
        public GameTemplateSelectorViewModel ViewModel
        {
            get { return ((GameTemplateSelectorViewModel) Resources["ViewModel"]); }
        }


        /// <summary>
        /// Loads available templates, configures the view model (<see cref="GameTemplateSelectorViewModel"/>) and builds the application bar (<see cref="BuildApplicationBar"/>
        /// </summary>
        /// <param name="e"></param>
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
                                                                            LocalizationHelper.GetString("CreateGameTemplate"));
            icon.Click += (o, args) => ViewModel.CreateGameTemplateCommand.Execute(null);                       
        }


        /// <summary>
        /// Creates a Uri that can be used to navigate to this page
        /// </summary>
        /// <returns></returns>
        public static Uri CreateNavigationUri()
        {
            return new Uri("/GameTemplateSelector.xaml", UriKind.Relative);
        }


        private void CustomGameListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox gameListListBox = (ListBox) sender;
            if (gameListListBox.SelectedIndex != -1)
            {
                ViewModel.SelectGameTemplateCommand.Execute(gameListListBox.SelectedItem);
                gameListListBox.SelectedIndex = -1;
            }
        }
    }
}