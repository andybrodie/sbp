using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
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
            IApplicationBarIconButton icon = ApplicationBarHelper.AddButton(ApplicationBar,
                                                                            ApplicationBarHelper.ButtonIcons["Save"],
                                                                            LocalizationHelper.GetString("SaveLevel"));
            icon.Click += SaveLevelClick;
            icon = ApplicationBarHelper.AddButton(ApplicationBar, ApplicationBarHelper.ButtonIcons["Cancel"],
                                                  LocalizationHelper.GetString("Cancel"));

            icon.Click += CancelClick;
/*            icon = ApplicationBarHelper.AddButton(ApplicationBar,
                                                    ApplicationBarHelper.ButtonIcons["Edit"],
                                                    LocalizationHelper.GetString("SelectLicense"));
  */ // TODO License selector logic, we could do with a picker
        }

        private void CancelClick(object sender, EventArgs e)
        {
            ViewModel.CancelCommand.Execute(null);
        }

        private void SaveLevelClick(object sender, EventArgs e)
        {
            ViewModel.SaveCommand.Execute(null);
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
            return new Uri(string.Format("/LevelEditor.xaml?{0}={1}&{2}={3}&{4}={5}&{6}={7}",
                                         GameTemplateIdParameterName,
                                         HttpUtility.UrlEncode(gameTemplateId),
                                         LevelIndexQueryParameterName,
                                         levelIndex,
                                         CreateNewQueryParameterName,
                                         createNew,
                                         ImageIdQueryParameterName,
                                         HttpUtility.UrlEncode(imageId)
                               ), UriKind.Relative);
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
            ViewModel.SelectImageCommand.Execute(null);
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