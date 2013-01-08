using System;
using System.Windows.Controls;
using Locima.SlidingBlock.Messaging;
using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;
using NLog;

namespace Locima.SlidingBlock
{

    /// <summary>
    /// MVVM view that shows a list of save games, along the user to select one to load that game and continue playing from where they left off
    /// </summary>
    public partial class SavedGameSelector : PhoneApplicationPage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private SaveGameSelectorViewModel ViewModel
        {
            get
            {
                return (SaveGameSelectorViewModel)
                       Resources["viewModel"];
            }
        }


        /// <summary>
        /// Invokes <see cref="InitializeComponent"/>
        /// </summary>
        public SavedGameSelector()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Registers default view message handlers (<see cref="DefaultMessageHandlers.Register"/> and initialises the <see cref="ViewModel"/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DefaultMessageHandlers.Register(this, ViewModel);
            ViewModel.Initialise();
        }
    

        private void SaveGameListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveGameMenuItem selectedItem = (SaveGameMenuItem) SaveGameListbox.SelectedItem;
            if (selectedItem != null)
            {
                Logger.Debug("Save game selected {0}", selectedItem);
                if (selectedItem.LaunchGameCommand.CanExecute(selectedItem))
                {
                    Logger.Info("Invoking launch game action using {0}", selectedItem);
                    selectedItem.LaunchGameCommand.Execute(selectedItem);
                }
            }
        }


        /// <summary>
        /// Creates a Uri that can be navigated to, to launch this page
        /// </summary>
        public static Uri CreateNavigationUri()
        {
            return new Uri("/SavedGameSelector.xaml", UriKind.Relative);
        }
    }
}