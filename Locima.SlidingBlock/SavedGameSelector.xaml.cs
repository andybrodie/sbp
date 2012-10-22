using System;
using System.Windows.Controls;
using Locima.SlidingBlock.Messaging;
using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;
using NLog;

namespace Locima.SlidingBlock
{
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


        public SavedGameSelector()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DefaultMessageHandlers.Register(this, ViewModel);
            ViewModel.Initialise();
        }
    

        private void SaveGameListBoxItemChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveGameMenuItem selectedItem = (SaveGameMenuItem) SaveGameListbox.SelectedItem;
            Logger.Debug("Save game selected {0}", selectedItem);
            if (selectedItem.LaunchGameCommand.CanExecute(selectedItem))
            {             
                Logger.Info("Invoking launch game action using {0}", selectedItem);
                selectedItem.LaunchGameCommand.Execute(selectedItem);
            }
        }


        public static Uri CreateNavigationUri()
        {
            return new Uri("/SavedGameSelector.xaml", UriKind.Relative);
        }
    }
}