using System;
using System.Collections.ObjectModel;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Persistence;
using Microsoft.Phone.Controls;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    public class PlayerSelectorViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private ObservableCollection<PlayerSelectorItem> _playerList;

        public ObservableCollection<PlayerSelectorItem> PlayerList
        {
            get { return _playerList; }
            private set
            {
                _playerList = value;
                OnNotifyPropertyChanged("PlayerList");
            }
        }

        public void Initialise()
        {
            PlayerList = GetPlayers();
        }
        

        /// <summary>
        ///   Retrieve a list of all the players, in a nice <see cref="ObservableCollection{T}" /> of <see
        ///    cref="PlayerSelectorItem" />.
        /// </summary>
        /// <returns> Never returns null, but may return an empty collection </returns>
        private ObservableCollection<PlayerSelectorItem> GetPlayers()
        {
            ObservableCollection<PlayerSelectorItem> list = new ObservableCollection<PlayerSelectorItem>();
            foreach (PlayerDetails player in PlayerStorageManager.Instance.GetAvailablePlayers())
            {
                Logger.Debug("Adding player {0} to the list of selectable players", player.Name);
                list.Add(new PlayerSelectorItem(this, player));
            }
            return list;
        }

    }
}