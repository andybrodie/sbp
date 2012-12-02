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
        public const string GameTemplateQueryParameterName = "gameTemplateId";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public GameEditor()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Convenience access for the view model that is initialise in the XAML
        /// </summary>
        public GameTemplateEditorViewModel ViewModel
        {
            get { return ((GameTemplateEditorViewModel) Resources["ViewModel"]); }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            BuildApplicationBar();
            DefaultMessageHandlers.Register(this, ViewModel);

            string gameTemplateId = this.GetQueryParameter(GameTemplateQueryParameterName, s => s);
            ViewModel.GameTemplateId = gameTemplateId;

            ViewModel.Initialise();
        }


        private void BuildApplicationBar()
        {
            Logger.Info("Creating application bar");

            ApplicationBar = new ApplicationBar();

            // Add a new level to the end of the set of levels
            IApplicationBarIconButton icon = ApplicationBarHelper.AddButton(ApplicationBar,
                                                                            ApplicationBarHelper.ButtonIcons["New"],
                                                                            LocalizationHelper.GetString(
                                                                                "AppendLevelButton"));

            icon.Click += (o, args) => ViewModel.AddEditLevel(true, LevelsListBox.Items.Count);

            // Save changes made to the game template
            icon = ApplicationBarHelper.AddButton(ApplicationBar,
                                                  ApplicationBarHelper.ButtonIcons["Save"],
                                                  LocalizationHelper.GetString(
                                                      "SaveGameTemplateButton"));

            icon.Click += (o, args) => ViewModel.SaveTemplate();

            // Cancel changes made to the game template
            // TODO Include an "are you sure?" dialog if the game template has been modified and we'll lose changes
            icon = ApplicationBarHelper.AddButton(ApplicationBar,
                                                  ApplicationBarHelper.ButtonIcons["Cancel"],
                                                  LocalizationHelper.GetString("Cancel"));

            icon.Click += (sender, args) => NavigationService.GoBack();
        }


        public static Uri CreateNavigationUri()
        {
            return CreateNavigationUri(null);
        }


        public static Uri CreateNavigationUri(string gameTemplateId)
        {
            string baseUri = "/GameEditor.xaml";
            if (!string.IsNullOrEmpty(gameTemplateId))
            {
                baseUri = string.Format("{0}?{1}={2}", baseUri, GameTemplateQueryParameterName, gameTemplateId);
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