using System.Windows;
using System.Windows.Controls;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Controls;
using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NLog;
using Locima.SlidingBlock.Messaging;

namespace Locima.SlidingBlock
{
    /// <summary>
    ///   <c>PlayerSelector</c> shows a list of the available players and allows the user to add a new one.
    /// </summary>
    /// <remarks>
    ///   It has been implemented in the simplest way I could find, so ignores MVVM.  You may find this useful as a starting point to understanding how Windows Phone pages work.
    /// </remarks>
    public partial class PlayerSelector : PhoneApplicationPage
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();



        /// <summary>
        /// Calls <see cref="InitializeComponent"/>
        /// </summary>
        public PlayerSelector()
        {
            InitializeComponent();
        }

        
        /// <summary>
        /// Convenience access for the view model that is initialise in the XAML
        /// </summary>
        public PlayerSelectorViewModel ViewModel
        {
            get { return ((PlayerSelectorViewModel) Resources["ViewModel"]); }
        }


        /// <summary>
        /// Sets up the application bar and registered the default message handlers
        /// </summary>
        /// <param name="e">unused</param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            BuildApplicationBar();
            this.RegisterDefaultMessageHandlers(ViewModel);
            
            ViewModel.Initialise();

        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            IApplicationBarIconButton icon = ApplicationBarHelper.AddButton(ApplicationBar,
                                                                            ApplicationBarHelper.ButtonIcons["New"],
                                                                            LocalizationHelper.GetString("AddPlayer"));
            icon.Click += (o, args) => NavigationService.Navigate(PlayerEditor.CreateNavigationUri(null));

        }


        private void PlayerListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayerSelectorItem selectedItem = (PlayerSelectorItem)((ListBox)sender).SelectedItem;
            Logger.Debug("Player selected {0}", selectedItem);
            if (selectedItem.SelectPlayerCommand.CanExecute(selectedItem))
            {
                Logger.Info("Invoking select player action using {0}", selectedItem);
                selectedItem.SelectPlayerCommand.Execute(selectedItem);
            }            
        }

    }

}