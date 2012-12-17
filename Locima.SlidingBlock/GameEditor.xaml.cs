using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
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
    /// Edits the title and author of a game template and allows the user to add more levels or edit existing ones using <see cref="LevelEditor"/>
    /// </summary>
    public partial class GameEditor : PhoneApplicationPage
    {

        /// <summary>
        /// The Uri query parameter for the game template ID
        /// </summary>
        public const string GameTemplateQueryParameterName = "gameTemplateId";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Invokes <see cref="InitializeComponent"/>
        /// </summary>
        public GameEditor()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Convenience access for the view model that is initialise in the XAML
        /// </summary>
        public GameEditorViewModel ViewModel
        {
            get { return ((GameEditorViewModel) Resources["ViewModel"]); }
        }


        /// <summary>
        /// Builds the application bar (<see cref="BuildApplicationBar"/> and initialises the view model (<see cref="GameEditorViewModel"/>)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            BuildApplicationBar();
            DefaultMessageHandlers.Register(this, ViewModel);

            string gameTemplateId = this.GetQueryParameter(GameTemplateQueryParameterName);
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

            icon.Click += (sender, args) => ViewModel.SaveFinalTemplateChanges();

            // Cancel changes made to the game template
            // TODO Include an "are you sure?" dialog if the game template has been modified and we'll lose changes
            icon = ApplicationBarHelper.AddButton(ApplicationBar,
                                                  ApplicationBarHelper.ButtonIcons["Cancel"],
                                                  LocalizationHelper.GetString("Cancel"));

            icon.Click += (sender, args) => ViewModel.ConfirmCancelCommand.Execute(null);
        }



        /// <summary>
        /// Creates a Uri that navigates to this page with no game template ID (i.e. create new game template)
        /// </summary>
        /// <returns></returns>
        public static Uri CreateNavigationUri()
        {
            return CreateNavigationUri(null, 0);
        }


        /// <summary>
        /// Creates a Uri that navigates to this page with a game template ID passed for editing
        /// </summary>
        /// <param name="gameTemplateId">The ID of the game template to edit</param>
        /// <param name="suppressPreviousPageCount">The number of pages to suppress after the new page has been navigated to</param>
        /// <returns>A Uri that navigates to this page</returns>
        public static Uri CreateNavigationUri(string gameTemplateId, int suppressPreviousPageCount)
        {
            const string baseUri = "/GameEditor.xaml";
            UriConstructor uriCons = new UriConstructor(baseUri, UriKind.Relative);
            if (!string.IsNullOrEmpty(gameTemplateId))
            {
                uriCons.AddParameter(GameTemplateQueryParameterName, gameTemplateId);
            }
            if (suppressPreviousPageCount>0)
            {
                uriCons.AddParameter(App.SuppressBackQueryParameterName, suppressPreviousPageCount);
            }
            return uriCons.ToUri();
        }


        /// <summary>
        /// Invoked when the user selects a level
        /// </summary>
        /// <param name="sender">The <see cref="ListBox"/> that sent this event</param>
        /// <param name="e">Unused</param>
        private void LevelsListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (ListBox) sender;
            if (lb.SelectedIndex != -1)
            {
                ViewModel.AddEditLevel(false, lb.SelectedIndex);
            }
        }

        /// <summary>
        /// Forces a binding update whenever a control is updated
        /// </summary>
        /// <remarks>Used for ensure that changes to <see cref="TextBox"/> controls are reflected when clicking application bar buttons</remarks>
        /// <param name="sender">The control that was changed</param>
        /// <param name="unused">unused</param>
        private void ControlChanged(object sender, TextChangedEventArgs unused)
        {
            DependencyProperty property = null;
            Control control = (Control) sender;

            if (control is TextBox) property = TextBox.TextProperty;
            if (control is PasswordBox) property = PasswordBox.PasswordProperty;
            if (control is CheckBox) property = ToggleButton.IsCheckedProperty;

            if (property != null)
            {
                BindingExpression be = control.GetBindingExpression(property);
                if (be != null)
                {
                    be.UpdateSource();
                }
            }
        }
    }
}