using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Persistence;
using Microsoft.Phone.Controls;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// The view model for <see cref="PlayerSelector"/>
    /// </summary>
    /// <remarks>
    /// This is for displaying a list of players, each individual player has its own view model object of type <see cref="PlayerViewModel"/></remarks>
    public class PlayerSelectorViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <See cref="PlayerList"/>
        /// </summary>
        private string _activePlayerName;

        /// <summary>
        /// Backing field for <see cref="PlayerList"/>
        /// </summary>
        private ObservableCollection<PlayerViewModel> _playerList = new ObservableCollection<PlayerViewModel>();

        /// <summary>
        /// The list of all the players
        /// </summary>
        public ObservableCollection<PlayerViewModel> PlayerList
        {
            get { return _playerList; }
        }

        /// <summary>
        /// A note on the UI on who the active player is
        /// </summary>
        public string ActivePlayerName
        {
            get { return _activePlayerName; }
            set
            {
                _activePlayerName = value;
                OnNotifyPropertyChanged("ActivePlayerName");
            }
        }


        /// <summary>
        /// Ensures that the delete context menu item for each player has set the <see cref="Control.IsEnabled"/> property appropriately
        /// and that <see cref="IPlayerStorageManager.CurrentPlayer"/> is updated if the current player has been deleted.
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="args">unused</param>
        private void PlayerListOnCollectionChanged(object sender,
                                                   NotifyCollectionChangedEventArgs args)
        {
            // Need to change the IsEnabled state of the Delete option on the context menu of PlayerList items to disable if count==1 or enable if count==2
            // This ensures that the user can't delete the last player
            if ((PlayerList.Count == 1) || (PlayerList.Count == 2))
            {
                foreach (PlayerViewModel player in PlayerList)
                {
                    ((DelegateCommand) player.DeletePlayerCommand).RaiseCanExecuteChanged();
                }
            }

        }


        /// <summary>
        /// Sets <see cref="PlayerList"/> up
        /// </summary>
        public void Initialise()
        {
            PlayerList.CollectionChanged += PlayerListOnCollectionChanged;
            PlayerList.Clear();
            foreach (PlayerDetails player in PlayerStorageManager.Instance.GetAvailablePlayers())
            {
                Logger.Debug("Adding player {0} to the list of selectable players", player.Name);
                PlayerList.Add(new PlayerViewModel(this, player));
            }
            ActivePlayerName = PlayerStorageManager.Instance.CurrentPlayer.Name;
        }
    }
}