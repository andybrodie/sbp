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
    public partial class GameEditor : PhoneApplicationPage
    {
        public const string GameDefinitionQueryParameterName = "gameDefId";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public GameEditor()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Convenience access for the view model that is initialise in the XAML
        /// </summary>
        public AddEditGameDefinitionViewModel ViewModel
        {
            get { return ((AddEditGameDefinitionViewModel) Resources["viewModel"]); }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            BuildApplicationBar();
            DefaultMessageHandlers.Register(this, ViewModel);

            string gameDefinitionId = this.GetQueryParameter(GameDefinitionQueryParameterName, s => s);
            ViewModel.GameDefinitionId = gameDefinitionId;

            ViewModel.Initialise();
        }


        private void BuildApplicationBar()
        {
            Logger.Info("Creating application bar");

            ApplicationBar = new ApplicationBar();
            IApplicationBarIconButton icon = ApplicationBarHelper.AddButton(ApplicationBar,
                                                                            ApplicationBarHelper.ButtonIcons["New"],
                                                                            LocalizationHelper.GetString(
                                                                                "AppendLevelButton"));

            icon.Click += (o, args) => ViewModel.AddEditLevel(true, LevelsListBox.Items.Count);
        }


        public static Uri CreateNavigationUri()
        {
            return CreateNavigationUri(null);
        }


        public static Uri CreateNavigationUri(string gameDefinitionId)
        {
            string baseUri = "/GameEditor.xaml";
            if (!string.IsNullOrEmpty(gameDefinitionId))
            {
                baseUri = string.Format("{0}?{1}={2}", baseUri, GameDefinitionQueryParameterName, gameDefinitionId);
            }
            return new Uri(baseUri, UriKind.Relative);
        }


        private void LevelsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (ListBox) sender;
            if (lb.SelectedIndex != -1)
            {
                ViewModel.AddEditLevel(false, lb.SelectedIndex);
            }
        }
    }
}