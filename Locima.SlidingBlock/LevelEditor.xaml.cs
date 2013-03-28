using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Controls;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.Messaging;
using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Locima.SlidingBlock
{

    /// <summary>
    /// The MVVM view for the basic level editor
    /// </summary>
    /// <remarks>
    /// This page allows the user to change the details of a <see cref="LevelDefinition"/> via a <see cref="LevelDefinitionViewModel"/> instance held in <see cref="ViewModel"/>.</remarks>
    public partial class LevelEditor : PhoneApplicationPage
    {
        private const string LevelIndexQueryParameterName = "levelIndex";
        private const string GameTemplateIdParameterName = "gameTemplateId";
        private const string CreateNewQueryParameterName = "createNew";
        private const string ImageIdQueryParameterName = "imageId";

        /// <summary>
        /// Invokes <see cref="InitializeComponent"/>
        /// </summary>
        public LevelEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Convenience access for the view model that is initialise in the XAML
        /// </summary>
        public LevelEditorViewModel ViewModel
        {
            get { return ((LevelEditorViewModel) Resources["ViewModel"]); }
        }


        /// <summary>
        /// Invokes <see cref="Initialise"/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Initialise();
        }


        /// <summary>
        /// Set up the <see cref="ViewModel"/> and register default view message event handlers
        /// </summary>
        public void Initialise()
        {
            BuildApplicationBar();

            ViewModel.GameTemplateId = this.GetQueryParameter(GameTemplateIdParameterName);
            ViewModel.CreateNew = this.GetQueryParameter(CreateNewQueryParameterName, s => Boolean.TrueString.Equals(s));
            ViewModel.LevelIndex = this.GetQueryParameterAsInt(LevelIndexQueryParameterName);

            ViewModel.Initialise();

            ViewModel.NewImageId = this.GetQueryParameter(ImageIdQueryParameterName);

            DefaultMessageHandlers.Register(this, ViewModel);
        }


        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            IApplicationBarIconButton saveButton = ApplicationBarHelper.AddButton(ApplicationBar,
                                                                            ApplicationBarHelper.ButtonIcons["Save"],
                                                                            LocalizationHelper.GetString("SaveLevel"));
            saveButton.Click += (sender, args) => ViewModel.SaveCommand.Execute(null);
            ViewModel.SaveCommand.CanExecuteChanged += (sender, args) => saveButton.IsEnabled = ((ICommand)sender).CanExecute(null);

            IApplicationBarIconButton cancelButton = ApplicationBarHelper.AddButton(ApplicationBar, ApplicationBarHelper.ButtonIcons["Cancel"],
                                                  LocalizationHelper.GetString("Cancel"));
            cancelButton.Click += (sender, args) => ViewModel.CancelCommand.Execute(null);
            ViewModel.CancelCommand.CanExecuteChanged += (sender, args) => cancelButton.IsEnabled = ((ICommand)sender).CanExecute(null);

/*            icon = ApplicationBarHelper.AddButton(ApplicationBar,
                                                    ApplicationBarHelper.ButtonIcons["Edit"],
                                                    LocalizationHelper.GetString("SelectLicense"));
  */
            // TODO License selector logic, we could do with a picker
        }


        /// <summary>
        /// Creates a navigation Uri to this page
        /// </summary>
        /// <param name="gameTemplateId">The ID game template ID that we want to edit a level on</param>
        /// <param name="levelIndex">The index of the level within the <paramref name="gameTemplateId"/> that we're editing</param>
        /// <param name="createNew">If true, then a new level will be inserted in the game template at <paramref name="levelIndex"/>, otherwise the existing definition will be edited</param>
        /// <param name="imageId">The ID of the image to replace the existing image for the level with (this is used when navigating here from <see cref="ImageAreaChooser"/>.  If null
        /// or empty then no action is taken.</param>
        /// <returns></returns>
        public static Uri CreateNavigationUri(string gameTemplateId, int levelIndex, bool createNew, string imageId)
        {
            UriConstructor uri = new UriConstructor("/LevelEditor.xaml", UriKind.Relative);
            uri.AddParameter(GameTemplateIdParameterName, gameTemplateId);
            uri.AddParameter(LevelIndexQueryParameterName, levelIndex);
            uri.AddParameter(CreateNewQueryParameterName, createNew);
            uri.AddParameter(ImageIdQueryParameterName, imageId);
            return uri.ToUri();
        }


        /// <summary>
        /// Creates a navigation Uri to this page
        /// </summary>
        /// <param name="gameTemplateId">The ID game template ID that we want to edit a level on</param>
        /// <param name="levelIndex">The index of the level within the <paramref name="gameTemplateId"/> that we're editing</param>
        /// <param name="createNew">If true, then a new level will be inserted in the game template at <paramref name="levelIndex"/>, otherwise the existing definition will be edited</param>
        /// <returns></returns>
        public static Uri CreateNavigationUri(string gameTemplateId, int levelIndex, bool createNew)
        {
            return CreateNavigationUri(gameTemplateId, levelIndex, createNew, String.Empty);
        }


        private void PreviewImageTap(object sender, GestureEventArgs e)
        {
            // Need to check the command is executable
            if (ViewModel.SelectImageCommand.CanExecute(null))
            {
                ViewModel.SelectImageCommand.Execute(null);
            }
        }
    

        /// <summary>
        /// Forces a binding update whenever a control is updated
        /// </summary>
        /// <remarks>Used for ensure that changes to <see cref="TextBox"/> controls are reflected when clicking application bar buttons.
        /// If you don't have this then if you edit a text string and don't dismiss the keyboard before clicking an application bar button you
        /// WON'T get the changes made to the text box saved.  This event handler forces the binding to be updated and the text entered to be read</remarks>
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