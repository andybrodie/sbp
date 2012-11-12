using System;
using System.Net;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Controls;
using Locima.SlidingBlock.Messaging;
using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NLog;

namespace Locima.SlidingBlock
{
    public partial class LevelEditor : PhoneApplicationPage
    {
        public const string LevelIndexQueryParameterName = "levelIndex";
        public const string CreateNewQueryParameterName = "createNew";
        public const string GameDefinitionIdParameterName = "gameDef";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Convenience access for the view model that is initialise in the XAML
        /// </summary>
        public LevelEditorViewModel ViewModel
        {
            get { return ((LevelEditorViewModel)Resources["viewModel"]); }
        }


        /// <summary>
        /// Invokes <see cref="InitializeComponent"/>
        /// </summary>
        public LevelEditor()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Initialise();
        }

        public void Initialise()
        {
            BuildApplicationBar();
            ViewModel.GameDefinitionId = this.GetQueryParameter(GameDefinitionIdParameterName, s => s);
            ViewModel.CreateNew = this.GetQueryParameter(CreateNewQueryParameterName, s => Boolean.TrueString.Equals(s));
            ViewModel.LevelIndex = this.GetQueryParameterAsInt(LevelIndexQueryParameterName);
            ViewModel.Initialise();
            DefaultMessageHandlers.Register(this, ViewModel);
        }


        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            IApplicationBarIconButton icon = ApplicationBarHelper.AddButton(ApplicationBar,
                                                                ApplicationBarHelper.ButtonIcons["Save"],
                                                                LocalizationHelper.GetString("SavePlayer"));
            icon.Click += SaveLevelClick;
            icon = ApplicationBarHelper.AddButton(ApplicationBar, ApplicationBarHelper.ButtonIcons["Cancel"],
                                                  LocalizationHelper.GetString("Cancel"));

            icon.Click += CancelClick;
/*
            icon = ApplicationBarHelper.AddButton(ApplicationBar,
                                                    ApplicationBarHelper.ButtonIcons["Edit"],
                                                    LocalizationHelper.GetString("SelectLicense"));
  */          // TODO License selector logic, we could do with a picker
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
        /// Create a Uri to navigate to this page with parameter poroviders
        /// </summary>
        /// <param name="gameDefinitionId"></param>
        /// <param name="levelIndex"></param>
        /// <param name="createNew"></param>
        /// <returns></returns>
        public static Uri CreateNavigationUri(string gameDefinitionId, int levelIndex, bool createNew)
        {
            return new Uri(string.Format("/LevelEditor.xaml?{0}={1}&{2}={3}&{4}={5}",
                                         GameDefinitionIdParameterName,
                                         HttpUtility.UrlEncode(gameDefinitionId),
                                         LevelIndexQueryParameterName,
                                         levelIndex,
                                         CreateNewQueryParameterName,
                                         createNew
                               ), UriKind.Relative);
        }

        private void PreviewImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ViewModel.SelectImageCommand.Execute(null);
        }

    }
}